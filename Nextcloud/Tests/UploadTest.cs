#if(DEBUG)

using FileFlows.Nextcloud.FlowElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Nextcloud.Tests;

[TestClass]
public class UploadTest : TestBase
{
    [TestMethod]
    public void Test()
    {
        var args = new NodeParameters("/home/john/src/ff-files/test-files/images/heic1.heic", Logger, false, string.Empty, new LocalFileService());
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../../../nextcloud.json");
        };

        var ele = new UploadToNextcloud();
        ele.DestinationPath = "ff-test/" + Guid.NewGuid() + ".heic";
        Assert.AreEqual(1, ele.Execute(args));
    }
}
#endif