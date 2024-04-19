
#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.Plugin;
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
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty, null);
        args.Variables["file.Name"] = "tv4a-starwarsrebels.s01e15-1080p.mkv";
        args.Variables["file.NameNoExtension"] = "tv4a-starwarsrebels.s01e15-1080p";
        args.Variables["ext"] = "mkv";
        args.Variables["file.Extension"] = "mkv";
        
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}");

        Assert.AreEqual(@"D:\test\tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    
    [TestMethod]
    public void MoveTests_Variable_Filename_Linux()
    {
        var logger = new TestLogger();
        string shortname = Guid.NewGuid().ToString();
        var args = new FileFlows.Plugin.NodeParameters($@"D:\fileflows\temp\runner\{shortname}.mp4", logger, false, string.Empty, null);
        
        args.Variables["file.Orig.Name"] = "tv4a-starwarsrebels.s01e15-1080p.mkv";
        args.Variables["file.Orig.NameNoExtension"] = "tv4a-starwarsrebels.s01e15-1080p";
        args.Variables["file.Orig.Extension"] = "mkv";
        args.Variables["file.Orig.FullName"] = "/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv";

        args.Variables["file.Name"] = shortname + ".mp4";
        args.Variables["file.NameNoExtension"] = shortname;
        args.Variables["ext"] = "mp4";
        args.Variables["file.Extension"] = "mp4";
        
        string dest = MoveFile.GetDestinationPath(args, @"\\10.0.0.1\media\converted", "{file.Name}");

        Assert.AreEqual($@"\\10.0.0.1\media\converted\{shortname}.mp4", dest);
    }
    
    [TestMethod]
    public void MoveTests_Variable_FilenameExt()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty, null);
        args.Variables["file.Name"] = "tv4a-starwarsrebels.s01e15-1080p.mkv";
        args.Variables["file.NameNoExtension"] = "tv4a-starwarsrebels.s01e15-1080p";
        args.Variables["ext"] = "mkv";
        args.Variables["file.Extension"] = "mkv";
        
        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}{file.Extension}");

        Assert.AreEqual(@"D:\test\tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_FilenameNoExtension()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty, null);
        args.Variables["file.Name"] = "tv4a-starwarsrebels.s01e15-1080p.mkv";
        args.Variables["file.NameNoExtension"] = "tv4a-starwarsrebels.s01e15-1080p";
        args.Variables["ext"] = "mkv";
        args.Variables["file.Extension"] = "mkv";
        
        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.NameNoExtension}");

        Assert.AreEqual(@"D:\test\tv4a-starwarsrebels.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_Ext()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty, null);
        args.Variables["file.Name"] = "tv4a-starwarsrebels.s01e15-1080p.mkv";
        args.Variables["file.NameNoExtension"] = "tv4a-starwarsrebels.s01e15-1080p";
        args.Variables["ext"] = "mkv";
        args.Variables["file.Extension"] = "mkv";

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}{ext}");

        Assert.AreEqual(@"D:\test\tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_Original_Filename()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty, null);
        args.Variables["file.Orig.Name"] = "tv4a-starwarsrebels.s01e15-1080p.mkv";
        args.Variables["file.Orig.NameNoExtension"] = "tv4a-starwarsrebels.s01e15-1080p";
        args.Variables["ext"] = "mkv";
        args.Variables["file.Orig.Extension"] = "mkv";
        
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.Name}");

        Assert.AreEqual(@"D:\test\tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    [TestMethod]
    public void MoveTests_Variable_Original_FilenameExt()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty, null);
        args.Variables["file.Orig.Name"] = "tv4a-starwarsrebels.s01e15-1080p.mkv";
        args.Variables["file.Orig.NameNoExtension"] = "tv4a-starwarsrebels.s01e15-1080p";
        args.Variables["ext"] = "mkv";
        args.Variables["file.Orig.Extension"] = "mkv";
        
        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.Name}{file.Orig.Extension}");

        Assert.AreEqual(@"D:\test\tv4a-starwarsrebels.s01e15-1080p.mkv", dest);
    }
    [TestMethod]
    public void MoveTests_Variable_Original_NoExtension()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"/home/user/test/tv4a-starwarsrebels.s01e15-1080p.mkv", logger, false, string.Empty, null);
        args.Variables["file.Orig.Name"] = "tv4a-starwarsrebels.s01e15-1080p.mkv";
        args.Variables["file.Orig.NameNoExtension"] = "tv4a-starwarsrebels.s01e15-1080p";
        args.Variables["ext"] = "mkv";
        args.Variables["file.Orig.Extension"] = "mkv";
        
        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.NameNoExtension}");

        Assert.AreEqual(@"D:\test\tv4a-starwarsrebels.mkv", dest);
    }
    
    [TestMethod]
    public void MoveTests_MoveFolder()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"\\tower\downloads\downloaded\tv\The.Walking.Dead.Dead.City.S01E04\some-file.mkv", logger, false, string.Empty, null);
        args.RelativeFile = @"The.Walking.Dead.Dead.City.S01E04\some-file.mkv";
        args.Variables["file.Name"] = "some-file.mkv";
        args.Variables["file.NameNoExtension"] = "some-file";
        args.Variables["ext"] = "mkv";
        args.Variables["file.Orig.Extension"] = "mkv";

        string dest = MoveFile.GetDestinationPath(args, @"\\tower\downloads\converted\tv", null, moveFolder:true);

        Assert.AreEqual(@"\\tower\downloads\converted\tv\The.Walking.Dead.Dead.City.S01E04\some-file.mkv", dest);
    }
    
}

#endif