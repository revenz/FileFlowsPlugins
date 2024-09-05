#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TouchTests : TestBase
{
    FileFlows.Plugin.NodeParameters Args;

    protected override void TestStarting()
    {
        Args = new FileFlows.Plugin.NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
    }

    [TestMethod]
    public void Touch_File()
    {
        Touch node = new ();
        node.FileName = TempFile;
        var result = node.Execute(Args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Touch_Folder()
    {
        Touch node = new();
        node.FileName = TempPath;
        var result = node.Execute(Args);
        Assert.AreEqual(1, result);
    }
}

#endif