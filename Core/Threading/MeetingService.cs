using System;
using System.Windows;
using System.Windows.Threading;
using ZOOM_SDK_DOTNET_WRAP;

namespace ZoomJWAssistant.Core.Threading
{

    class MeetingService : BaseUIDispatchingService, IMeetingServiceDotNetWrap
    {
        private IMeetingServiceDotNetWrap meetingServiceDotNetWrap;

        public MeetingService(IMeetingServiceDotNetWrap meetingServiceDotNetWrap)
        {
            this.meetingServiceDotNetWrap = meetingServiceDotNetWrap;
        }

        public void Add_CB_onMeetingSecureKeyNotification(onMeetingSecureKeyNotification cb)
        {
            InvokeAPI(() => meetingServiceDotNetWrap.Add_CB_onMeetingSecureKeyNotification(cb));
        }

        public void Add_CB_onMeetingStatisticsWarningNotification(onMeetingStatisticsWarningNotification cb)
        {
            InvokeAPI(() => meetingServiceDotNetWrap.Add_CB_onMeetingStatisticsWarningNotification(cb));
        }

        public void Add_CB_onMeetingStatusChanged(onMeetingStatusChanged cb)
        {
            InvokeAPI(() => meetingServiceDotNetWrap.Add_CB_onMeetingStatusChanged(cb));
        }

        public IAnnotationControllerDotNetWrap GetAnnotationController()
        {
            throw new NotImplementedException();
        }

        public ConnectionQuality GetAudioConnQuality()
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.GetAudioConnQuality());
        }

        public IMeetingH323HelperDotNetWrap GetH323Helper()
        {
            throw new NotImplementedException();
        }

        public IMeetingAudioControllerDotNetWrap GetMeetingAudioController()
        {
            throw new NotImplementedException();
        }

        public IMeetingBreakoutRoomsControllerDotNetWrap GetMeetingBreakoutRoomsController()
        {
            throw new NotImplementedException();
        }

        public IMeetingChatControllerDotNetWrap GetMeetingChatController()
        {
            throw new NotImplementedException();
        }

        public IMeetingConfigurationDotNetWrap GetMeetingConfiguration()
        {
            return meetingServiceDotNetWrap.GetMeetingConfiguration();
        }

        public IMeetingInfo GetMeetingInfo()
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.GetMeetingInfo());
        }

        public IMeetingLiveStreamControllerDotNetWrap GetMeetingLiveStreamController()
        {
            throw new NotImplementedException();
        }

        public IMeetingParticipantsControllerDotNetWrap GetMeetingParticipantsController()
        {
            return new MeetingParticipantsController(meetingServiceDotNetWrap.GetMeetingParticipantsController());
        }

        public IMeetingPhoneHelperDotNetWrap GetMeetingPhoneHelper()
        {
            throw new NotImplementedException();
        }

        public IMeetingRecordingControllerDotNetWrap GetMeetingRecordingController()
        {
            throw new NotImplementedException();
        }

        public IMeetingRemoteControllerDotNetWrap GetMeetingRemoteController()
        {
            throw new NotImplementedException();
        }

        public IMeetingShareControllerDotNetWrap GetMeetingShareController()
        {
            throw new NotImplementedException();
        }

        public MeetingStatus GetMeetingStatus()
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.GetMeetingStatus());
        }

        public IMeetingVideoControllerDotNetWrap GetMeetingVideoController()
        {
            throw new NotImplementedException();
        }

        public IMeetingWaitingRoomControllerDotNetWrap GetMeetingWaitingRoomController()
        {
            return meetingServiceDotNetWrap.GetMeetingWaitingRoomController();
        }

        public ConnectionQuality GetSharingConnQuality()
        {
            throw new NotImplementedException();
        }

        public IMeetingUIControllerDotNetWrap GetUIController()
        {
            throw new NotImplementedException();
        }

        public ConnectionQuality GetVideoConnQuality()
        {
            throw new NotImplementedException();
        }

        public bool IsMeetingLocked()
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.IsMeetingLocked());
        }

        public SDKError Join(JoinParam joinParam)
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.Join(joinParam));
        }

        public SDKError Leave(LeaveMeetingCmd leaveCmd)
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.Leave(leaveCmd));
        }

        public SDKError LockMeeting()
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.LockMeeting());
        }

        public void Remove_CB_onMeetingSecureKeyNotification(onMeetingSecureKeyNotification cb)
        {
            InvokeAPI(() => meetingServiceDotNetWrap.Remove_CB_onMeetingSecureKeyNotification(cb));
        }

        public void Remove_CB_onMeetingStatisticsWarningNotification(onMeetingStatisticsWarningNotification cb)
        {
            InvokeAPI(() => meetingServiceDotNetWrap.Remove_CB_onMeetingStatisticsWarningNotification(cb));
        }

        public void Remove_CB_onMeetingStatusChanged(onMeetingStatusChanged cb)
        {
            InvokeAPI(() => meetingServiceDotNetWrap.Remove_CB_onMeetingStatusChanged(cb));
        }

        public SDKError Start(StartParam startParam)
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.Start(startParam));
        }

        public SDKError UnlockMeeting()
        {
            return InvokeAPI(() => meetingServiceDotNetWrap.UnlockMeeting());
        }
    }
}
