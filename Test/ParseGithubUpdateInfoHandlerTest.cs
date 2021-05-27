using NUnit.Framework;
using System;
using System.IO;
using ZoomJWAssistant.Update;

namespace ZoomJWAssistant.Test
{
    [TestFixture]
    public class ParseGithubUpdateInfoHandlerTest
    {
        [Test]
        public void UpdateJsonIsParsedCorrectly()
        {
            var testJsonFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, @"..\..\Test\test-response-release.json");
            string testJson = File.ReadAllText(testJsonFile);

            var updateInfo = new AutoUpdaterDotNET.ParseUpdateInfoEventArgs(testJson);
            ParseGithubUpdateInfoHandler.ParseUpdateInfoHandler(updateInfo);

            Console.WriteLine(updateInfo.UpdateInfo.CurrentVersion);
            Console.WriteLine(updateInfo.UpdateInfo.ChangelogURL);
            Console.WriteLine(updateInfo.UpdateInfo.DownloadURL);
        }
    }
}
