#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TouchTests
{
    FileFlows.Plugin.NodeParameters Args;

    [TestInitialize]
    public void TestStarting()
    {
        Args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", new TestLogger(), false, string.Empty);
    }

    [TestMethod]
    public void Touch_File()
    {
        Touch node = new ();
        node.FileName = @"D:\videos\testfiles\basic.mkv";
        var result = node.Execute(Args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Touch_Folder()
    {
        Touch node = new();
        node.FileName = @"D:\videos\testfiles";
        var result = node.Execute(Args);
        Assert.AreEqual(1, result);
    }
}

#endif