using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using ZoomJWAssistant.Core;
using ZoomJWAssistant.Views;
using ZoomJWAssistant.Models;
using AvalonDock.Layout;
using System.Diagnostics;
using AvalonDock;
using AutoUpdaterDotNET;
using ZoomJWAssistant.Core.Threading;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Configuration;
using AvalonDock.Layout.Serialization;
using System.IO;
using System.IO.Compression;
using System.Text;
using ZoomJWAssistant.Update;

namespace ZoomJWAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public readonly ZoomService ZoomService = ZoomService.Instance;

        private bool _shutdown;
        private readonly MainWindowViewModel _viewModel;

        TextBoxOutputter consoleOutputter;
        private byte[] DefaultDockLayoutSerialized;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            InitializeComponent();

            consoleOutputter = new TextBoxOutputter(ConsoleTextBox);
            Console.SetOut(consoleOutputter);

            //AutoUpdater.CheckForUpdateEvent += (e) =>
            //{
            //    if (e.Error != null)
            //    {
            //        Console.WriteLine("Fehler beim Update-Check! " + e.Error.Message);
            //    }
            //    AutoUpdater.ShowUpdateForm(e.);
            //};
        }

        protected async override void OnContentRendered(EventArgs e)
        {
            CheckForUpdate();
            await CheckForSdkKeyData();
            try
            {
                await ZoomService.AuthenticateSDK();
                JoinMeeting();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Probleme beim Initialisieren des Zoom SDK! " + ex.Message);
            }
        }

        private void MetroWindow_OnSourceInitialized(object sender, EventArgs e)
        {
            var rightEdge = (Properties.Settings.Default.Left + Properties.Settings.Default.Width);
            var bottomEdge = (Properties.Settings.Default.Top + Properties.Settings.Default.Height);

            if (rightEdge >= 1.0d && rightEdge <= SystemParameters.VirtualScreenWidth &&
                bottomEdge >= 1.0d && bottomEdge <= SystemParameters.VirtualScreenHeight)
            {
                this.Top = Properties.Settings.Default.Top;
                this.Left = Properties.Settings.Default.Left;
                this.Height = Properties.Settings.Default.Height;
                this.Width = Properties.Settings.Default.Width;
                // Very quick and dirty - but it does the job
                if (Properties.Settings.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }

            KeepCurrentDockLayout();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.DockLayout))
            {
                try
                {
                    MemoryStream mem = new MemoryStream(System.Convert.FromBase64String(Properties.Settings.Default.DockLayout));
                    using (GZipStream gz = new GZipStream(mem, CompressionMode.Decompress, false))
                    using (var reader = new StreamReader(gz))
                    {
                        new XmlLayoutSerializer(this.dockManager).Deserialize(reader);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Kein zuvor gespeichertes Layout wiederhergestellt");
                }
            }
        }

        private async Task CheckForSdkKeyData()
        {
            var sdkKey = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["sdkKey"]);
            var sdkSecret = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["sdkSecret"]);

            if (string.IsNullOrWhiteSpace(sdkKey) || sdkKey.Contains("%ZOOM"))
            {
                sdkKey = Properties.Settings.Default.sdkKey;
                if (!string.IsNullOrWhiteSpace(sdkKey))
                {
                    ConfigurationManager.AppSettings["sdkKey"] = sdkKey;
                }
                else
                {
                    var result = await this.ShowInputAsync("SDK Key", "Bitte den SDK Key eingeben");
                    if (result != null) //user pressed cancel
                    {
                        ConfigurationManager.AppSettings["sdkKey"] = result;
                        Properties.Settings.Default.sdkKey = result;
                        Properties.Settings.Default.Save();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(sdkSecret) || sdkSecret.Contains("%ZOOM"))
            {
                sdkSecret = Properties.Settings.Default.sdkSecret;
                if (!string.IsNullOrWhiteSpace(sdkSecret))
                {
                    ConfigurationManager.AppSettings["sdkSecret"] = sdkSecret;
                }
                else
                {
                    var result = await this.ShowInputAsync("SDK Secret", "Bitte das SDK Secret eingeben");
                    if (result != null) //user pressed cancel
                    {
                        ConfigurationManager.AppSettings["sdkSecret"] = result;
                        Properties.Settings.Default.sdkSecret = result;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        private static void CheckForUpdate()
        {
            Console.WriteLine("Überprüfe auf Update für neue Version...");
            AutoUpdater.HttpUserAgent = "AutoUpdater";
            AutoUpdater.ParseUpdateInfoEvent += ParseGithubUpdateInfoHandler.ParseUpdateInfoHandler;
            AutoUpdater.Start("https://api.github.com/repos/r-oldenburg/zoom-jw-assistant/releases/latest");
        }

        public async Task JoinMeetingAsync(string meetingId, string meetingPassword, string userName) {
            var controller = await this.ShowProgressAsync("Anmeldung", "Meeting wird verbunden...");
            controller.SetIndeterminate();
            controller.SetCancelable(true);
            controller.Canceled += (e, s) => {
                ZoomService.LeaveMeeting();
            };
            ZoomService.JoinMeeting(ulong.Parse(meetingId.Trim().Replace(" ", "")), meetingPassword, userName, this.VideoCanvas);
            await TaskEx.WaitUntil(() => {
                controller.SetMessage(ZoomConstants.MeetingStatusDecoder(ZoomService.MeetingStatus));
                return controller.IsCanceled || ZoomService.InMeeting;
            });
            await controller.CloseAsync();
        }

        private void DockManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to close the document?", "AvalonDock Sample", MessageBoxButton.YesNo) == MessageBoxResult.No)
                e.Cancel = true;
        }

        private void OnLayoutRootPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var activeContent = ((LayoutRoot)sender).ActiveContent;
            if (e.PropertyName == "ActiveContent")
            {
                Debug.WriteLine(string.Format("ActiveContent-> {0}", activeContent));
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ZoomService.Instance.Shutdown();

            SaveWindowLayout();
            SaveDockLayout();

            if (e.Cancel)
            {
                return;
            }

            _viewModel.Dispose();
        }

        private void SaveDockLayout()
        {
            MemoryStream mem = new MemoryStream();
            using (mem)
            using (GZipStream gz = new GZipStream(mem, CompressionMode.Compress, false))
            using (var writer = new StreamWriter(gz))
            {
                new XmlLayoutSerializer(this.dockManager).Serialize(writer);
            }

            // after usings have closed the streams, otherwise error on decompression
            Properties.Settings.Default.DockLayout = Convert.ToBase64String(mem.ToArray());
            Properties.Settings.Default.Save();
        }

        private void KeepCurrentDockLayout()
        {
            MemoryStream mem = new MemoryStream();
            using (mem)
            using (var writer = new StreamWriter(mem))
            {
                new XmlLayoutSerializer(this.dockManager).Serialize(writer);
            }

            // after usings have closed the streams, otherwise error on decompression
            this.DefaultDockLayoutSerialized = mem.ToArray();
        }

        private void RestoreDefaultDockLayout()
        {
            using (var mem = new MemoryStream(this.DefaultDockLayoutSerialized))
            using (var reader = new StreamReader(mem))
            {
                new XmlLayoutSerializer(this.dockManager).Deserialize(reader);
            }
        }

        private void SaveWindowLayout()
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }
            Properties.Settings.Default.Save();
        }

        private async Task ConfirmShutdown()
        {
            var mySettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Quit",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = false
            };

            var result = await this.ShowMessageAsync("Quit application?",
                                                        "Sure you want to quit application?",
                                                        MessageDialogStyle.AffirmativeAndNegative, mySettings);

            _shutdown = result == MessageDialogResult.Affirmative;

            if (_shutdown)
            {
                Application.Current.Shutdown();
            }
        }

        private void ResetLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreDefaultDockLayout();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomService.ToggleHost();
        }

        private void VersionButton_Click(object sender, RoutedEventArgs e)
        {
            AutoUpdater.ReportErrors = true;
            CheckForUpdate();
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            JoinMeeting();
        }

        private void JoinMeeting()
        {
            var customDialog = new CustomDialog() { Title = "Mit Meeting verbinden" };

            var dataContext = new MeetingInfoContent(async instance =>
            {
                await this.HideMetroDialogAsync(customDialog);

                if (instance.Remember)
                {
                    Properties.Settings.Default.RememberLastMeetingDetails = instance.Remember;
                    Properties.Settings.Default.LastMeetingId = instance.MeetingId;
                    Properties.Settings.Default.LastMeetingPassword = StringCipher.Encrypt(instance.MeetingPassword, instance.MeetingId);
                    Properties.Settings.Default.LastUserName = instance.UserName ?? "JW Admin";
                    Properties.Settings.Default.Save();
                }

                await this.JoinMeetingAsync(instance.MeetingId, instance.MeetingPassword, instance.UserName);
            }, instance => this.HideMetroDialogAsync(customDialog));

            dataContext.Remember = Properties.Settings.Default.RememberLastMeetingDetails;
            dataContext.MeetingId = Properties.Settings.Default.LastMeetingId;
            if (!string.IsNullOrWhiteSpace(dataContext.MeetingId) && !string.IsNullOrWhiteSpace(Properties.Settings.Default.LastMeetingPassword))
            {
                dataContext.MeetingPassword = StringCipher.Decrypt(Properties.Settings.Default.LastMeetingPassword, dataContext.MeetingId);
            }
            dataContext.UserName = string.IsNullOrWhiteSpace(Properties.Settings.Default.LastUserName) ? "JW Admin" : Properties.Settings.Default.LastUserName;

            customDialog.Content = new MeetingInfoDialog { DataContext = dataContext };
            ((MeetingInfoDialog)customDialog.Content).MeetingPassword.Password = dataContext.MeetingPassword;

            this.ShowMetroDialogAsync(customDialog).ContinueWith((e) =>
            {
                TextBox textBox = customDialog.FindChild<TextBox>("MeetingIdTextBox");
                Dispatcher.BeginInvoke(DispatcherPriority.Input,
                    new Action(delegate () {
                    Keyboard.ClearFocus();
                    textBox.Focus();
                    Keyboard.Focus(textBox);
                }));
            });

        }
    }
}
