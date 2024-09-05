#if(DEBUG)

using FileFlows.Web.FlowElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

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
    
    [TestMethod]
    public void InputFile()
    {
        var args = new NodeParameters("https://images.pexels.com/photos/45201/kitty-cat-kitten-pet-45201.jpeg", Logger, false, string.Empty, new LocalFileService());

        var element = new InputUrl();
        element.Download = true;
        var result = element.Execute(args);
        Assert.AreEqual(1, result);
    }
}

#endif