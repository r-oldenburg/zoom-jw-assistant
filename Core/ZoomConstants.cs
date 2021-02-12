using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZOOM_SDK_DOTNET_WRAP;

namespace ZoomJWAssistant.Core
{
    public enum SdkError
    {
        SDKERR_SUCCESS = 0, // Success
        SDKERR_NO_IMPL = 1, // This feature is currently not available
        SDKERR_WRONG_USEAGE = 2, // Incorrect usage of the feature
        SDKERR_INVALID_PARAMETER = 3, // Wrong parameter
        SDKERR_MODULE_LOAD_FAILED = 4, // Loading module failed
        SDKERR_MEMORY_FAILED = 5, // No memory allocated
        SDKERR_SERVICE_FAILED = 6, // Internal service error
        SDKERR_UNINITIALIZE = 7, // SDK is not initialized before the use
        SDKERR_UNAUTHENTICATION = 8, // SDK is not authorized before the use
        SDKERR_NORECORDINGINPROCESS = 9, // No recording is in process
        SDKERR_TRANSCODER_NOFOUND = 10, // Transcoder module is not found
        SDKERR_VIDEO_NOTREADY = 11, // The video service is not ready
        SDKERR_NO_PERMISSION = 12, // No permission
        SDKERR_UNKNOWN = 13, // Unknown error
        SDKERR_OTHER_SDK_INSTANCE_RUNNING = 14, // Another SDK instance is in process
        SDKERR_INTERNAL_ERROR = 15, // SDK internal error
        SDKERR_NO_AUDIODEVICE_ISFOUND = 16, // No audio device is found
        SDKERR_NO_VIDEODEVICE_ISFOUND = 17, // No video device is found
        SDKERR_TOO_FREQUENT_CALL = 18, // API calls too frequent
        SDKERR_FAIL_ASSIGN_USER_PRIVILEGE = 19, // User cannot be assigned with the new privilege
        SDKERR_MEETING_DONT_SUPPORT_FEATURE = 20, // The current meeting does not support the request feature
    }

    class ZoomConstants
    {
        public static string DecodeSdkError(SDKError status) {
            switch (status)
            {
                case SDKError.SDKERR_SUCCESS: return "Success";
                case SDKError.SDKERR_NO_IMPL: return "This feature is currently not available";
                case SDKError.SDKERR_WRONG_USEAGE: return "Incorrect usage of the feature";
                case SDKError.SDKERR_INVALID_PARAMETER: return "Wrong parameter";
                case SDKError.SDKERR_MODULE_LOAD_FAILED: return "Loading module failed";
                case SDKError.SDKERR_MEMORY_FAILED: return "No memory allocated";
                case SDKError.SDKERR_SERVICE_FAILED: return "Internal service error";
                case SDKError.SDKERR_UNINITIALIZE: return "SDK is not initialized before the use";
                case SDKError.SDKERR_UNAUTHENTICATION: return "SDK is not authorized before the use";
                case SDKError.SDKERR_NORECORDINGINPROCESS: return "No recording is in process";
                case SDKError.SDKERR_TRANSCODER_NOFOUND: return "Transcoder module is not found";
                case SDKError.SDKERR_VIDEO_NOTREADY: return "The video service is not ready";
                case SDKError.SDKERR_NO_PERMISSION: return "No permission";
                case SDKError.SDKERR_UNKNOWN: return "Unknown error";
                case SDKError.SDKERR_OTHER_SDK_INSTANCE_RUNNING: return "Another SDK instance is in process";
                case (SDKError) 15: return "SDK internal error";
                case (SDKError) 16: return "No audio device is found";
                case (SDKError) 17: return "No video device is found";
                case (SDKError) 18: return "API calls too frequent";
                case (SDKError) 19: return "User cannot be assigned with the new privilege";
                case (SDKError) 20: return "The current meeting does not support the request feature";
            }

            return "Unknown";
        }
    }
}
