#if(DEBUG)

using FileFlows.Nextcloud.FlowElements;
using FileFlows.Nextcloud.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PluginTestLibrary;

namespace FileFlows.Nextcloud.Tests;

[TestClass]
public class UploadTest : TestBase
{
    [TestMethod]
    public void Test()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"Username": "user", "Password": "password", "Url": "http://nextcloud.test" }""";
        string destPath = "ff-test/" + Guid.NewGuid() + ".heic";
        Mock<INextcloudUploader> mockUploader = new();
        mockUploader.Setup(x => x.UploadFile(
                It.Is<string>(y => y == TempFile),
                It.Is<string>(y => y == destPath)))
            .Returns(true);
        
        var element = new UploadToNextcloud();
        element.GetUploader = (_, _, _, _) => mockUploader.Object;
        element.DestinationPath = destPath;
        Assert.AreEqual(1, element.Execute(args));
    }
}
#endif