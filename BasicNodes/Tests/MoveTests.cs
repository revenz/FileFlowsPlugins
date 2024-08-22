
#if(DEBUG)


using System.IO;
using FileFlows.Plugin;
using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class MoveTests : TestBase
{
    [TestMethod]
    public void MoveTests_Variable_Filename()
    {
        var args = new NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", Logger, false, string.Empty, new LocalFileService());

        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    
    [TestMethod]
    public void MoveTests_Variable_FilenameExt()
    {
        var args = new NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", Logger, false, string.Empty, new LocalFileService());

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}{file.Extension}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_FilenameNoExtension()
    {
        var tempFileNoExtension = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var tempFile = tempFileNoExtension + ".tmp";
        System.IO.File.WriteAllText(tempFile, "this is a temp file");
        var args = new NodeParameters(tempFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, Path.GetTempPath(), "{file.NameNoExtension}");

        Assert.AreEqual(tempFileNoExtension, dest);
    }

    [TestMethod]
    public void MoveTests_Variable_Ext()
    {
        var args = new NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", Logger, false, string.Empty, new LocalFileService());

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}{ext}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_Original_Filename()
    {
        var args = new NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", Logger, false, string.Empty, new LocalFileService());

        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileName}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    
    [TestMethod]
    public void MoveTests_Variable_Original_FilenameExt()
    {
        var args = new NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", Logger, false, string.Empty, new LocalFileService());

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileName}{file.Orig.Extension}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    [TestMethod]
    public void MoveTests_Variable_Original_NoExtension()
    {
        
        var args = new NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", Logger, false, string.Empty, new LocalFileService());

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileNameNoExtension}");

        Assert.AreEqual(@"D:/test/tv4a-starwarsrebels.mkv", dest);
    }
    
    [TestMethod]
    public void MoveTests_MoveFolder()
    {
        
        var args = new NodeParameters(@"\\tower\downloads\downloaded\tv\The.Walking.Dead.Dead.City.S01E04\some-file.mkv", Logger, false, string.Empty, new LocalFileService());
        args.RelativeFile = @"The.Walking.Dead.Dead.City.S01E04\some-file.mkv";

        string dest = MoveFile.GetDestinationPath(args, @"\\tower\downloads\converted\tv", null, moveFolder:true);

        Assert.AreEqual(@"\\tower\downloads\converted\tv\The.Walking.Dead.Dead.City.S01E04\some-file.mkv", dest);
    }
    
    
    /// <summary>
    /// Tests that confirms additional files are moved
    /// </summary>
    [TestMethod]
    public void MoveTests_AdditionalFiles()
    {
        var args = new NodeParameters(@"/home/john/Videos/move-me/dir/basic.mkv", Logger, false, string.Empty, new LocalFileService());

        var ele = new MoveFile();
        ele.AdditionalFiles = new[] { "*.srt" };
        ele.DestinationPath = "/home/john/Videos/converted";
        var result = ele.Execute(args);
        Assert.AreEqual(1, result);
    }
}

#endif