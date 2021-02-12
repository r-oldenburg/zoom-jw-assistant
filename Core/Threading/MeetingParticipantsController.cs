using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZOOM_SDK_DOTNET_WRAP;

namespace ZoomJWAssistant.Core.Threading
{
    class MeetingParticipantsController : BaseUIDispatchingService, IMeetingParticipantsControllerDotNetWrap
    {
        private IMeetingParticipantsControllerDotNetWrap wrappedController;

        public MeetingParticipantsController(IMeetingParticipantsControllerDotNetWrap wrappedController)
        {
            this.wrappedController = wrappedController;
        }

        public void Add_CB_onHostChangeNotification(onHostChangeNotification cb)
        {
            InvokeAPI(() => wrappedController.Add_CB_onHostChangeNotification(cb));
        }

        public void Add_CB_onCoHostChangeNotification(onCoHostChangeNotification cb)
        {
            InvokeAPI(() => wrappedController.Add_CB_onCoHostChangeNotification(cb));
        }

        public void Add_CB_onLowOrRaiseHandStatusChanged(onLowOrRaiseHandStatusChanged cb)
        {
            InvokeAPI(() => wrappedController.Add_CB_onLowOrRaiseHandStatusChanged(cb));
        }

        public void Add_CB_onUserJoin(onUserJoin cb)
        {
            InvokeAPI(() => wrappedController.Add_CB_onUserJoin(cb));
        }

        public void Add_CB_onUserLeft(onUserLeft cb)
        {
            InvokeAPI(() => wrappedController.Add_CB_onUserLeft(cb));
        }

        public void Add_CB_onUserNameChanged(onUserNameChanged cb)
        {
            InvokeAPI(() => wrappedController.Add_CB_onUserNameChanged(cb));
        }

        public SDKError AssignCoHost(uint userId)
        {
            return InvokeAPI(() => wrappedController.AssignCoHost(userId));
        }

        public SDKError CanBeCoHost(uint userId)
        {
            return InvokeAPI(() => wrappedController.CanBeCoHost(userId));
        }

        public SDKError CanReclaimHost(ref ValueType bCanReclaimHost)
        {
            //ValueType result = false;
            //var errorCode = InvokeAPI(() => {
            //    return wrappedController.CanReclaimHost(ref result);
            //});
            //return errorCode;
            throw new NotImplementedException();
        }

        public SDKError ChangeUserName(uint userId, string userName, bool bSaveUserName)
        {
            return InvokeAPI(() => wrappedController.ChangeUserName(userId, userName, bSaveUserName));
        }

        public SDKError ExpelUser(uint userId)
        {
            return InvokeAPI(() => wrappedController.ExpelUser(userId));
        }

        public uint[] GetParticipantsList()
        {
            return InvokeAPI(() => wrappedController.GetParticipantsList());
        }

        public IUserInfoDotNetWrap GetUserByUserID(uint userId)
        {
            return InvokeAPI(() => wrappedController.GetUserByUserID(userId));
        }

        public SDKError LowerAllHands()
        {
            return InvokeAPI(() => wrappedController.LowerAllHands());
        }

        public SDKError LowerHand(uint userId)
        {
            return InvokeAPI(() => wrappedController.LowerHand(userId));
        }

        public SDKError MakeHost(uint userId)
        {
            return InvokeAPI(() => wrappedController.MakeHost(userId));
        }

        public SDKError RaiseHand()
        {
            return InvokeAPI(() => wrappedController.RaiseHand());
        }

        public SDKError ReclaimHost()
        {
            return InvokeAPI(() => wrappedController.ReclaimHost());
        }

        public SDKError ReclaimHostByHostKey(string host_key)
        {
            return InvokeAPI(() => wrappedController.ReclaimHostByHostKey(host_key));
        }

        public void Remove_CB_onHostChangeNotification(onHostChangeNotification cb)
        {
            InvokeAPI(() => wrappedController.Remove_CB_onHostChangeNotification(cb));
        }

        public void Remove_CB_onCoHostChangeNotification(onCoHostChangeNotification cb)
        {
            InvokeAPI(() => wrappedController.Remove_CB_onCoHostChangeNotification(cb));
        }

        public void Remove_CB_onLowOrRaiseHandStatusChanged(onLowOrRaiseHandStatusChanged cb)
        {
            InvokeAPI(() => wrappedController.Remove_CB_onLowOrRaiseHandStatusChanged(cb));
        }

        public void Remove_CB_onUserJoin(onUserJoin cb)
        {
            InvokeAPI(() => wrappedController.Remove_CB_onUserJoin(cb));
        }

        public void Remove_CB_onUserLeft(onUserLeft cb)
        {
            InvokeAPI(() => wrappedController.Remove_CB_onUserLeft(cb));
        }

        public void Remove_CB_onUserNameChanged(onUserNameChanged cb)
        {
            InvokeAPI(() => wrappedController.Remove_CB_onUserNameChanged(cb));
        }

        public SDKError RevokeCoHost(uint userId)
        {
            return InvokeAPI(() => wrappedController.RevokeCoHost(userId));
        }
    }
}
