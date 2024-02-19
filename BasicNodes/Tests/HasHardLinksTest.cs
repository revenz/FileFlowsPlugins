#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class HasHardLinksTest
{
    [TestMethod]
    public void HasHardLink()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/john/temp/test.file", logger, false, string.Empty, new LocalFileService());;
        
        HasHardLinks element = new ();
        
        var result = element.Execute(args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void NoHardLinks()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/john/temp/other.test", logger, false, string.Empty, new LocalFileService());;
        
        HasHardLinks element = new ();
        
        var result = element.Execute(args);
        Assert.AreEqual(2, result);
    }
}

#endif