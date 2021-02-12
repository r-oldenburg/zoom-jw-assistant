using ZoomJWAssistant.Core;

namespace ZoomJWAssistant.Models
{
    class MeetingAttendeeRename : ViewModelBase
    {
        public string fromName;
        public string toName;

        public MeetingAttendeeRename(string x)
        {
            var parts = x.Split('>');
            if (parts.Length > 1)
            {
                this.fromName = x.Substring(0, x.LastIndexOf('>') - 1).Trim();
                this.toName = parts[parts.Length - 1].Trim();
            }
        }
    }
}
