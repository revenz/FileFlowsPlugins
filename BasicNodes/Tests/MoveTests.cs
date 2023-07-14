using FileFlows.Plugin;

#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;


[TestClass]
public class MoveTests
{

    [TestMethod]
    public void MoveTests_Variable_Filename()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty);

        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    [TestMethod]
    public void MoveTests_Variable_FilenameExt()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty);

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}{file.Extension}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_FilenameNoExtension()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty);

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.NameNoExtension}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_Ext()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty);

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}{ext}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_Original_Filename()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty);

        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileName}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    [TestMethod]
    public void MoveTests_Variable_Original_FilenameExt()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty);

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileName}{file.Orig.Extension}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    [TestMethod]
    public void MoveTests_Variable_Original_NoExtension()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty);

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileNameNoExtension}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.mkv", dest);
    }
    
    [TestMethod]
    public void MoveTests_MoveFolder()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"\\tower\downloads\downloaded\tv\The.Walking.Dead.Dead.City.S01E04\some-file.mkv", logger, false, string.Empty);
        args.RelativeFile = @"The.Walking.Dead.Dead.City.S01E04\some-file.mkv";

        string dest = MoveFile.GetDestinationPath(args, @"\\tower\downloads\converted\tv", null, moveFolder:true);

        Assert.AreEqual(@"\\tower\downloads\converted\tv\The.Walking.Dead.Dead.City.S01E04\some-file.mkv", dest);
    }
}

#endif