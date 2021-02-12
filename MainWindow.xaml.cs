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

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            InitializeComponent();

            consoleOutputter = new TextBoxOutputter(ConsoleTextBox);
            Console.SetOut(consoleOutputter);
        }

        protected async override void OnContentRendered(EventArgs e)
        {
            AutoUpdater.Start("https://raw.githubusercontent.com/r-oldenburg/zoom-jw-assistant/master/Release.xml");

            var customDialog = new CustomDialog() { Title = "Mit Meeting verbinden" };

            var dataContext = new MeetingInfoContent(async instance =>
            {
                await this.HideMetroDialogAsync(customDialog);
                Properties.Settings.Default.RememberLastMeetingDetails = instance.Remember;
                Properties.Settings.Default.LastMeetingId = instance.MeetingId;
                Properties.Settings.Default.LastMeetingPassword = StringCipher.Encrypt(instance.MeetingPassword, instance.MeetingId);
                Properties.Settings.Default.LastUserName = instance.UserName ?? "JW Admin";
                if (instance.Remember)
                {
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
            dataContext.UserName = Properties.Settings.Default.LastUserName;

            customDialog.Content = new MeetingInfoDialog { DataContext = dataContext };
            ((MeetingInfoDialog)customDialog.Content).MeetingPassword.Password = dataContext.MeetingPassword;

            await this.ShowMetroDialogAsync(customDialog);
        }

        public async Task JoinMeetingAsync(string meetingId, string meetingPassword, string userName) {
            var controller = await this.ShowProgressAsync("Anmeldung", "Meeting wird verbunden...");
            controller.SetIndeterminate();

            await ZoomService.JoinMeetingAsync(ulong.Parse(meetingId.Trim().Replace(" ", "")), meetingPassword, userName, this.VideoCanvas);

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

            if (e.Cancel)
            {
                return;
            }

            _viewModel.Dispose();
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

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomService.ToggleHost();
        }
    }
}
