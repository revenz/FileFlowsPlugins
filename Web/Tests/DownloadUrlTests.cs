#if(DEBUG)

using FileFlows.Web.FlowElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Web.Tests;

[TestClass]
public class DownloadUrlTests : TestBase
{
    [TestMethod]
    public void Basic()
    {
        var args = new NodeParameters("https://www.reddit.com/r/FileFlows/", Logger, false, string.Empty, new LocalFileService());

        var element = new Downloader();
        var result = element.Execute(args);
        Assert.AreEqual(1, result);
    }
}

#endif