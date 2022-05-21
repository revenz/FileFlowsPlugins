using FileFlows.Plugin;

#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CopyTests
{

    [TestMethod]
    public void CopyTests_Dir_Mapping()
    {            
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
        args.PathMapper = s => s;
        
        CopyFile node = new ();
        node.CopyFolder = true;
        node.DestinationPath = "/mnt/tempNAS/media/dvd/output";
        node.DestinationFile = "{file.Orig.FileName}p.DVD.x264.slow.CRF16{ext}";
        var result = node.Execute(args);
        Assert.AreEqual(2, result);
    }

}

#endif