using AutoUpdaterDotNET;
using Newtonsoft.Json;

namespace ZoomJWAssistant.Update
{
    public class ParseGithubUpdateInfoHandler
    {
        public static void ParseUpdateInfoHandler(ParseUpdateInfoEventArgs args)
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);

            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = json.tag_name,
                ChangelogURL = json.html_url,
                DownloadURL = json.assets.First.browser_download_url
            };
        }
    }
}
