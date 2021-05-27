using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using ZOOM_SDK_DOTNET_WRAP;
using ZoomJWAssistant.Models;
using System.Windows;
using System.Windows.Threading;
using ZoomJWAssistant.Core.Threading;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using ZoomJWAssistant.Test;

namespace ZoomJWAssistant.Core
{
    public class ZoomService : ViewModelBase
    {
        public static ZoomService Instance { get; } = new ZoomService();

        private MeetingAttendee _myAttendee = new MeetingAttendee();
        private ICustomizedVideoContainerDotNetWrap videoContainer;
        private Canvas videoControl;

        private INormalVideoRenderElementDotNetWrap normalVideo;

        private ObservableCollection<MeetingAttendee> _attendees = new ObservableCollection<MeetingAttendee>();
        private LinkedList<MeetingAttendeeRename> _defaultRenames = new LinkedList<MeetingAttendeeRename>();

        public MeetingAttendee MyAttendee
        {
            get => this._myAttendee;
            set => this.Set(ref this._myAttendee, value);
        }

        public ObservableCollection<MeetingAttendee> Attendees
        {
            get => this._attendees;
            set => this.Set(ref this._attendees, value);
        }

        public int TotalNumberOfPersons
        {
            get => this._attendees.Select(x => x.NumberOfPersons).Sum();
        }

        public MeetingStatus MeetingStatus
        {
            get => this._meetingStatus;
            set
            {
                _meetingStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InMeeting));
            }
        }

        public string DefaultRenames
        {
            get => string.Join("\n", this._defaultRenames.Select(x => x.fromName + " > " + x.toName));
            set
            {
                _defaultRenames.Clear();
                value.Split('\n')
                    .Where(x => x != null && x.Trim().Contains(">"))
                    .Select(x => new MeetingAttendeeRename(x))
                    .ToList()
                    .ForEach(x => _defaultRenames.AddLast(x));
                Properties.Settings.Default.DefaultRenames = this.DefaultRenames;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                ReApplyAllRenamings();
            }
        }

        public bool InMeeting
        {
            get => this._meetingStatus == MeetingStatus.MEETING_STATUS_INMEETING;
        }

        private bool authenticatedSDK = false;
        private bool processIncomingNameChanges = true;

        private MeetingStatus _meetingStatus;
        private IMeetingServiceDotNetWrap meetingService;
        private IMeetingWaitingRoomControllerDotNetWrap waitingRoomService;
        private IAuthServiceDotNetWrap authService;
        private IMeetingParticipantsControllerDotNetWrap participantsController;

        private Dispatcher uiDispatcher = Application.Current.Dispatcher;

        private ZoomService() {
            _attendees.RegisterPropertyChangeHook(AttendeeChanged);
            _attendees.CollectionChanged += (e, o) => OnPropertyChanged(nameof(TotalNumberOfPersons));

            PropertyChanged += OnPropertyChange;

            this.DefaultRenames = Properties.Settings.Default.DefaultRenames;

            InitParam initParam = new InitParam();
            initParam.web_domain = "https://zoom.us";
            initParam.config_opts = new ConfigurableOptions
            {
                optionalFeatures = (1 << 5) //Using Customized UI
            };
            SDKError err = InvokeAPI(() => CZoomSDKeDotNetWrap.Instance.Initialize(initParam));
                   
            if (SDKError.SDKERR_SUCCESS == err)
            {
                meetingService = new MeetingService(CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap());
                waitingRoomService = meetingService.GetMeetingWaitingRoomController();
                authService = CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap();

                Console.WriteLine("Zoom SDK initialisiert");
                //TestData.sampleData.ForEach(Attendees.Add);
                return;
            }
            else
            {
                Console.WriteLine("Zoom SDK Fehler: " + err.ToString());
                throw new Exception("Zoom SDK Init Failure: " + err.ToString());
            }
        }

        public Task<AuthResult> AuthenticateSDK() {

            if (authenticatedSDK)
            {
                return Task.FromResult(AuthResult.AUTHRET_SUCCESS);
            }

            var sdkKey = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["sdkKey"]);
            var sdkSecret = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["sdkSecret"]);
            var token = JWTTokenCreator.CreateToken(sdkKey, sdkSecret);

            var t = new TaskCompletionSource<AuthResult>();

            authService.Add_CB_onAuthenticationReturn(err =>
            {
                if (AuthResult.AUTHRET_SUCCESS == err)
                {
                    Console.WriteLine("Token: " + token);
                    Console.WriteLine("Zoom SDK erfolgreich authentifiziert. " + token.Length);
                    authenticatedSDK = true;
                    t.TrySetResult(err);
                }
                else
                {
                    Console.WriteLine("Zoom SDK Authentifikations-Fehler: " + err.ToString());
                    Console.WriteLine("Token: " + token);
                    t.TrySetException(new Exception("Zoom SDK Auth Failure: " + err.ToString()));
                }
            });


            InvokeAPI(() => authService.SDKAuth(new AuthContext { jwt_token = token }));

            return t.Task;
        }

        public void JoinMeeting(ulong meetingId, string meetingPassword, string userName, Canvas videoCanvas)
        {
            JoinParam4WithoutLogin join_api_param = new JoinParam4WithoutLogin();
            join_api_param.meetingNumber = meetingId;
            join_api_param.userName = userName;
            join_api_param.isDirectShareDesktop = false;
            join_api_param.isAudioOff = true;
            join_api_param.isVideoOff = false;

            JoinParam param = new JoinParam();
            param.userType = SDKUserType.SDK_UT_WITHOUT_LOGIN;
            param.withoutloginJoin = join_api_param;

            meetingService.GetMeetingConfiguration().EnableInputMeetingScreenNameDlg(false);
            meetingService.GetMeetingConfiguration().Add_CB_onInputMeetingPasswordAndScreenNameNotification((handler) =>
            {
                if (handler.GetRequiredInfoType() == RequiredInfoType.REQUIRED_INFO_TYPE_Password)
                {
                    handler.InputMeetingPasswordAndScreenName(meetingPassword, userName);
                }
            });
            meetingService.Add_CB_onMeetingStatusChanged((MeetingStatus status, int iResult) => MeetingStatus = status);
            SDKError err = meetingService.Join(param);
            if (SDKError.SDKERR_SUCCESS == err)
            {
                videoControl = videoCanvas;

                participantsController = meetingService.GetMeetingParticipantsController();
                participantsController.Add_CB_onHostChangeNotification(HandleHostChanged);
                participantsController.Add_CB_onCoHostChangeNotification(HandleCoHostChanged);
                participantsController.Add_CB_onUserJoin(HandleUsersJoining);
                participantsController.Add_CB_onUserNameChanged(HandleUserNameChange);
                participantsController.Add_CB_onUserLeft(HandleUsersLeaving);

                waitingRoomService.Add_CB_onWatingRoomUserJoin(HandleUserJoinWaitingRoom);
                waitingRoomService.Add_CB_onWatingRoomUserLeft(HandleUserLeftWaitingRoom);

                GetAllParticipants().ForEach(Attendees.Add);
            }
            else //error handle
            {
                throw new Exception("Zoom Join Meeting Failure: " + err.ToString());
            }
        }

        internal void ToggleHost()
        {
            if (InMeeting)
            {
                if (MyAttendeeIsAnyHost())
                {
                    Console.WriteLine("Host-Rechte aktiv");
                }
                else
                {
                    Console.WriteLine("Keine Host-Rechte bisher");
                    //participantsController.ReclaimHost();
                    //participantsController.AssignCoHost(MyAttendee.UserId);
                }
            }
        }

        public void LeaveMeeting()
        {
            meetingService.Leave(LeaveMeetingCmd.LEAVE_MEETING);
        }

        public void Shutdown()
        {
            this.InvokeAPI<object>(() =>
            {
                if (InMeeting)
                {
                    LeaveMeeting();
                }
                authService.LogOut();
                return null;
            });
        }

        private void HandleHostChanged(uint newHostUserId)
        {
            var attendee = GetAttendee(newHostUserId);
            if (attendee != null)
            {
                Attendees.ToList().ForEach(x => x.IsHost = false);
                Console.WriteLine("Neuer Host! " + attendee.Name);
                attendee.IsHost = true;
            }
        }
        
        private void HandleCoHostChanged(uint newHostUserId, bool isCoHost)
        {
            var attendee = GetAttendee(newHostUserId);
            if (attendee != null)
            {
                Console.WriteLine("Neuer CoHost! " + attendee.Name);
                attendee.IsCoHost = isCoHost;
            }
        }

        private void HandleUserNameChange(uint userId, string newUserName)
        {
            if (processIncomingNameChanges)
            {
                var attendee = GetAttendee(userId);
                if (attendee != null && attendee.CurrentTechnicalName != newUserName)
                {
                    attendee.CurrentTechnicalName = newUserName;
                }
            }
        }

        private void HandleUserLeftWaitingRoom(uint userID)
        {
            var attendee = GetAttendee(userID);
            if (attendee != null)
            {
                Console.WriteLine("Teilnehmer hat Warteraum verlassen: " + attendee.Name);
                if (attendee != null)
                {
                    Attendees.Remove(attendee);
                }
            }
        }

        private void HandleUsersLeaving(uint[] userIds)
        {
            if (userIds != null)
            {
                for (int i = userIds.GetLowerBound(0); i <= userIds.GetUpperBound(0); i++)
                {
                    var attendee = GetAttendee((uint)userIds.GetValue(i));
                    if (attendee != null)
                    {
                        Console.WriteLine("Teilnehmer verlassen: " + attendee.Name);
                        if (attendee != null)
                        {
                            Attendees.Remove(attendee);
                        }
                    }
                }
            }
        }

        private void HandleUserJoinWaitingRoom(uint userId)
        {
            var user = GetWaitingUserInfo(userId);
            if (user != null)
            {
                var attendee = GetAttendee(userId);
                if (attendee != null)
                {
                    Console.WriteLine("Teilnehmer geändert: " + attendee.UserId + " ZoomID (" + userId + ")");
                    attendee.MergeUserData(user);
                }
                else
                {
                    var newAttendee = new MeetingAttendee(user);
                    newAttendee.IsWaiting = true;
                    Console.WriteLine("Neuer Teilnehmer im Warteraum: " + newAttendee.CurrentTechnicalName + ", " + newAttendee.UserId + ", " + (newAttendee.IsPhone ? "Tel." : ""));
                    if (user.IsMySelf())
                    {
                        MyAttendee = newAttendee;
                    }
                    Attendees.Add(newAttendee);

                    AcceptKnownAttendeeWaiting(newAttendee);
                    ApplyRenamings(newAttendee);
                }
            }
        }

        private void HandleUsersJoining(uint[] userIds)
        {
            if (userIds != null)
            {
                for (int i = userIds.GetLowerBound(0); i <= userIds.GetUpperBound(0); i++)
                {
                    uint userId = (uint)userIds.GetValue(i);
                    if (userId > 0)
                    {
                        var user = GetUserInfo(userId);
                        if (user != null)
                        {
                            var attendee = GetAttendee(userId);
                            if (attendee != null)
                            {
                                Console.WriteLine("Teilnehmer geändert: " + attendee.UserId + " ZoomID (" + userId + ")");
                                attendee.MergeUserData(user);
                            }
                            else
                            {
                                Console.WriteLine("Neuer Teilnehmer: " + user.GetUserNameW() + ", " + userId + ", " + (user.IsPurePhoneUser() ? "Tel." : ""));
                                var newAttendee = new MeetingAttendee(user);
                                if (user.IsMySelf())
                                {
                                    MyAttendee = newAttendee;
                                }
                                Attendees.Add(newAttendee);

                                ApplyRenamings(newAttendee);
                            }
                        }
                    }
                }
            }
        }

        private void ShowAttendeeVideo(MeetingAttendee newAttendee)
        {
            if (newAttendee != null)
            {
                if (normalVideo != null)
                {
                    normalVideo.Subscribe(newAttendee.UserId);
                }
            }
        }

        private void ApplyRenamings(MeetingAttendee attendee)
        {
            var rename = this._defaultRenames.FirstOrDefault(x => x.fromName.Equals(attendee.Name));
            if (rename != null)
            {
                Console.WriteLine("-> wird umbenannt zu " + rename.toName);
                if (MeetingAttendee.IsNameWithNumber(rename.toName))
                {
                    attendee.CurrentTechnicalName = rename.toName;
                } else
                {
                    attendee.Name = rename.toName;
                }
            }
        }

        private void AcceptKnownAttendeeWaiting(MeetingAttendee attendee)
        {
            var rename = this._defaultRenames.FirstOrDefault(x => x.fromName.Equals(attendee.Name) || x.toName.Equals(attendee.Name));
            if (rename != null)
            {
                if (attendee.IsWaiting)
                {
                    Console.WriteLine("-> wird automatisch aus Warteraum zugelassen");
                    waitingRoomService.AdmitToMeeting(attendee.UserId);
                }
            }
        }

        private void ReApplyAllRenamings()
        {
            if (!InMeeting)
            {
                return;
            }

            foreach (var attendee in Attendees)
            {
                if (IsStillInMeeting(attendee.UserId))
                {
                    AcceptKnownAttendeeWaiting(attendee);
                    ApplyRenamings(attendee);
                }
            }
        }

        protected void OnPropertyChange(object s, PropertyChangedEventArgs eProp)
        {
            if (eProp.PropertyName == nameof(MeetingStatus))
            {
                if (InMeeting)
                {
                    RelayoutVideoElement();
                }
            }
        }

        private void RelayoutVideoElement()
        {
            try
            {
                if (videoContainer == null && videoControl != null)
                {
                    videoControl.DataContextChanged += (object context, DependencyPropertyChangedEventArgs args) =>
                    {
                        ShowAttendeeVideo((MeetingAttendee)videoControl.DataContext);
                    };

                    var hwndVideo = ((HwndSource)HwndSource.FromVisual(videoControl)).Handle;
                    var position = videoControl.TranslatePoint(new Point(0, 0), Window.GetWindow(videoControl));
                    RECT vcRect = new RECT { Top = (int)position.Y, Left = (int)position.X, Bottom = (int)(videoControl.RenderSize.Height + position.Y), Right = (int)(videoControl.RenderSize.Width + position.X) };

                    var uiMgr = CZoomSDKeDotNetWrap.Instance.GetCustomizedUIMgrWrap();
                    videoContainer = uiMgr.CreateVideoContainer(hwndVideo, vcRect);
                    videoContainer.Show();

                    videoControl.IsVisibleChanged += (object sender, DependencyPropertyChangedEventArgs e) =>
                    {
                        Console.WriteLine("Video SHOW/HIDE " + videoControl.IsVisible);
                        if (videoControl.IsVisible)
                        {
                            if (videoContainer == null)
                            {
                                RelayoutVideoElement();
                            }
                            videoContainer.Show();
                            Console.WriteLine("SHOW Handle " + ((HwndSource)HwndSource.FromVisual(videoControl)).Handle);
                        }
                        else
                        {
                            if (normalVideo != null)
                            {
                                normalVideo.Hide();
                            }
                            if (videoContainer != null)
                            {
                                videoContainer.DestroyAllVideoElement();
                                videoContainer.Hide();
                                videoContainer = null;
                            }
                        }
                    };
                    videoControl.SizeChanged += (object sender, SizeChangedEventArgs e) =>
                    {
                        if (!videoControl.IsVisible) return;
                        var pos = videoControl.TranslatePoint(new Point(0, 0), Window.GetWindow(videoControl));
                        videoContainer.Resize(new RECT { Top = (int)pos.Y, Left = (int)pos.X, Bottom = (int)(e.NewSize.Height + pos.Y), Right = (int)(e.NewSize.Width + pos.X) });

                        if (normalVideo != null)
                        {
                            var tmpRenderSize = (Size)PresentationSource.FromVisual(videoControl).CompositionTarget.TransformToDevice.Transform((Vector)e.NewSize);
                            normalVideo.SetPos(new RECT { Top = 0, Left = 0, Bottom = (int)(tmpRenderSize.Height), Right = (int)(tmpRenderSize.Width) });
                        }
                    };
                }

                var source = PresentationSource.FromVisual(videoControl);
                Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
                var renderSize = (Size)transformToDevice.Transform((Vector)videoControl.RenderSize);
                RECT rect = new RECT { Top = 0, Left = 0, Bottom = (int)(renderSize.Height), Right = (int)(renderSize.Width) };

                normalVideo = (INormalVideoRenderElementDotNetWrap)videoContainer.CreateVideoElement(VideoRenderElementType.VideoRenderElement_NORMAL);
                if (normalVideo != null)
                {
                    normalVideo.SetPos(rect);
                    normalVideo.Show();
                    normalVideo.EnableShowScreenNameOnVideo(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex.Message);
            }
        }

        private void AttendeeChanged(object sender, PropertyChangedEventArgs e)
        {
            MeetingAttendee attendee = (MeetingAttendee)sender;
            if (attendee == MyAttendee)
            {
                if (e.PropertyName == nameof(MeetingAttendee.IsHost) || e.PropertyName == nameof(MeetingAttendee.IsCoHost))
                {
                    ReApplyChangesToZoom();
                }
            }

            if (e.PropertyName == nameof(MeetingAttendee.CurrentTechnicalName))
            {
                if (MyAttendeeIsAnyHost() && IsStillInMeeting(attendee.UserId))
                {
                    ApplyNameToZoom(attendee);
                }
            }
            if (e.PropertyName == nameof(MeetingAttendee.NumberOfPersons))
            {
                this.OnPropertyChanged(nameof(TotalNumberOfPersons));
            }
        }

        private void ApplyNameToZoom(MeetingAttendee attendee)
        {
            Console.WriteLine("Namens-Änderung am Teilnehmer Richtung ZOOM auf: " + attendee.CurrentTechnicalName);
            processIncomingNameChanges = false;
            var result = participantsController.ChangeUserName(attendee.UserId, attendee.CurrentTechnicalName, false);
            if (result == (SDKError) 18)
            {
                Console.WriteLine("Zu schnelle Abfolge, erneuter Versuch.");
            }
            processIncomingNameChanges = true;
        }

        private void ReApplyChangesToZoom()
        {
            if (!MyAttendeeIsAnyHost() || !InMeeting)
            {
                return;
            }

            foreach (var attendee in Attendees)
            {
                if (MyAttendeeIsAnyHost() && IsStillInMeeting(attendee.UserId))
                {
                    var zoomName = GetUserInfo(attendee.UserId).GetUserNameW();
                    if (zoomName != null && zoomName != attendee.CurrentTechnicalName)
                    {
                        ApplyNameToZoom(attendee);
                    }
                }
            }
        }

        private bool MyAttendeeIsAnyHost()
        {
            return MyAttendee != null && ( MyAttendee.IsHost || MyAttendee.IsCoHost );
        }

        private MeetingAttendee GetAttendee(uint userId)
        {
            return Attendees.Where(x => x.UserId == userId).FirstOrDefault();
        }

        private bool IsStillInMeeting(uint userId)
        {
            var user = GetUserInfo(userId);
            return user != null;
        }

        private IUserInfoDotNetWrap GetWaitingUserInfo(uint userId)
        {
            return waitingRoomService.GetWaitingRoomUserInfoByID(userId);
        }

        private IUserInfoDotNetWrap GetUserInfo(uint userId)
        {
            return participantsController.GetUserByUserID(userId);
        }

        public List<MeetingAttendee> GetAllParticipants() {
            var result = new List<MeetingAttendee>();

            if (this.MeetingStatus == MeetingStatus.MEETING_STATUS_INMEETING)
            {
                var participantIds = participantsController.GetParticipantsList();

                foreach (uint id in participantIds)
                {
                    var user = participantsController.GetUserByUserID(id);
                    result.Add(new MeetingAttendee(user));
                }
            }

            return result;
        }

        private class MeetingPasswordAndScreenNameHandler : IMeetingPasswordAndScreenNameHandler
        {
            void IMeetingPasswordAndScreenNameHandler.Cancel()
            {
                Console.WriteLine("Cancel Meeting PW handling");
            }

            RequiredInfoType IMeetingPasswordAndScreenNameHandler.GetRequiredInfoType()
            {
                throw new NotImplementedException();
            }

            bool IMeetingPasswordAndScreenNameHandler.InputMeetingPasswordAndScreenName(string meetingPassword, string screenName)
            {
                throw new NotImplementedException();
            }
        }

        private T InvokeAPI<T>(Func<T> codeToInvoke)
        {
            return (T) uiDispatcher.Invoke(DispatcherPriority.Normal,
                new Func<T>(delegate
                {
                    return codeToInvoke.Invoke();
                })
            );
        }
    }
}
