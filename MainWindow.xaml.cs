using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using ZOOM_SDK_DOTNET_WRAP;
using ZoomJWAssistant.ExampleViews;
using ZoomJWAssistant.Models;

namespace ZoomJWAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool _shutdown;
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel(DialogCoordinator.Instance);
            DataContext = _viewModel;

            InitializeComponent();
        }

        protected async override void OnContentRendered(EventArgs e)
        {
            var customDialog = new CustomDialog() { Title = "Mit Meeting verbinden" };

            var dataContext = new MeetingInfoContent(async instance =>
            {
                await this.HideMetroDialogAsync(customDialog);
                Properties.Settings.Default.RememberLastMeetingDetails = instance.Remember;
                Properties.Settings.Default.LastMeetingId = instance.MeetingId;
                Properties.Settings.Default.LastUserName = instance.UserName ?? "JW Admin";
                Properties.Settings.Default.Save();

                await this.JoinMeetingAsync(instance.MeetingId, instance.UserName);
            });
            dataContext.Remember = Properties.Settings.Default.RememberLastMeetingDetails;
            dataContext.MeetingId = Properties.Settings.Default.LastMeetingId;
            dataContext.UserName = Properties.Settings.Default.LastUserName;

            customDialog.Content = new MeetingInfoDialog { DataContext = dataContext };

            await this.ShowMetroDialogAsync(customDialog);
        }

        public async Task JoinMeetingAsync(string meetingId, string userName) {
            var controller = await this.ShowProgressAsync("Anmeldung", "Meeting wird verbunden...");
            controller.SetIndeterminate();

            //init sdk
            ZOOM_SDK_DOTNET_WRAP.InitParam initParam = new ZOOM_SDK_DOTNET_WRAP.InitParam();
            initParam.web_domain = "https://zoom.us";
            ZOOM_SDK_DOTNET_WRAP.SDKError err = ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.Initialize(initParam);
            if (ZOOM_SDK_DOTNET_WRAP.SDKError.SDKERR_SUCCESS == err)
            {
            }
            else//error handle.todo
            {
                await controller.CloseAsync();
                return;
            }

            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onAuthenticationReturn(async ret =>
            {
                if (AuthResult.AUTHRET_SUCCESS == ret)
                {
                    JoinParam param = new JoinParam();
                    param.userType = SDKUserType.SDK_UT_WITHOUT_LOGIN;
                    JoinParam4WithoutLogin join_api_param = new JoinParam4WithoutLogin();

                    join_api_param.meetingNumber = UInt64.Parse(meetingId);
                    join_api_param.userName = userName;
                    join_api_param.isDirectShareDesktop = false;
                    join_api_param.isAudioOff = true;
                    join_api_param.isVideoOff = true;

                    param.withoutloginJoin = join_api_param;

                    err = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Join(param);
                    if (SDKError.SDKERR_SUCCESS == err)
                    {
                        await controller.CloseAsync();
                        //Hide();
                    }
                    else//error handle
                    {
                        await controller.CloseAsync();
                    }
                }
                else//error handle.todo
                {
                    await controller.CloseAsync();
                }
            });

            var sdkKey = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["sdkKey"]);
            var sdkSecret = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["sdkSecret"]);

            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().SDKAuth(new AuthContext
            {
                jwt_token = JWTTokenCreator.CreateToken(sdkKey, sdkSecret)
            });

        }

        //callback
        public void onAuthenticationReturn(AuthResult ret)
        {
            if (AuthResult.AUTHRET_SUCCESS == ret)
            {
                Show();
            }
            else//error handle.todo
            {

            }
        }

        private async void ShowMessageDialog(object sender, RoutedEventArgs e)
        {
            // This demo runs on .Net 4.0, but we're using the Microsoft.Bcl.Async package so we have async/await support
            // The package is only used by the demo and not a dependency of the library!
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Hi",
                NegativeButtonText = "Go away!",
                FirstAuxiliaryButtonText = "Cancel",
                ColorScheme = MetroDialogOptions.ColorScheme,
                DialogButtonFontSize = 20D
            };

            MessageDialogResult result = await this.ShowMessageAsync("Hello!", "Welcome to the world of metro!",
                                                                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, mySettings);

            if (result != MessageDialogResult.FirstAuxiliary)
                await this.ShowMessageAsync("Result", "You said: " + (result == MessageDialogResult.Affirmative
                                                ? mySettings.AffirmativeButtonText
                                                : mySettings.NegativeButtonText +
                                                    Environment.NewLine + Environment.NewLine + "This dialog will follow the Use Accent setting."));
        }

        private async void ShowProgressDialog(object sender, RoutedEventArgs e)
        {
            var mySettings = new MetroDialogSettings()
            {
                NegativeButtonText = "Close now",
                AnimateShow = false,
                AnimateHide = false,
                ColorScheme = this.MetroDialogOptions.ColorScheme
            };

            var controller = await this.ShowProgressAsync("Please wait...", "We are baking some cupcakes!", settings: mySettings);
            controller.SetIndeterminate();

            await Task.Delay(5000);

            controller.SetCancelable(true);

            double i = 0.0;
            while (i < 6.0)
            {
                double val = (i / 100.0) * 20.0;
                controller.SetProgress(val);
                controller.SetMessage("Baking cupcake: " + i + "...");

                if (controller.IsCanceled)
                    break; //canceled progressdialog auto closes.

                i += 1.0;

                await Task.Delay(2000);
            }

            await controller.CloseAsync();

            if (controller.IsCanceled)
            {
                await this.ShowMessageAsync("No cupcakes!", "You stopped baking!");
            }
            else
            {
                await this.ShowMessageAsync("Cupcakes!", "Your cupcakes are finished! Enjoy!");
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e.Cancel)
            {
                return;
            }

            if (_viewModel.QuitConfirmationEnabled
                && _shutdown == false)
            {
                e.Cancel = true;

                // We have to delay the execution through BeginInvoke to prevent potential re-entrancy
                Dispatcher.BeginInvoke(new Action(async () => await this.ConfirmShutdown()));
            }
            else
            {
                _viewModel.Dispose();
            }
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
    }
}
