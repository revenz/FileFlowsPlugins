
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
        string fileName = Guid.NewGuid().ToString();
        var tempFile = Path.Combine(Path.GetTempPath(),fileName+ ".mkv");
        System.IO.File.WriteAllText(tempFile, "this is a temp file ");
        var args = new NodeParameters(tempFile, Logger, 
            false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);
        
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}");

        Assert.AreEqual($"D:/test/{fileName}.mkv", dest);
    }
    
    [TestMethod]
    public void MoveTests_Variable_FilenameExt()
    {
        string fileName = Guid.NewGuid().ToString();
        var tempFile = Path.Combine(Path.GetTempPath(),fileName+ ".mkv");
        System.IO.File.WriteAllText(tempFile, "this is a temp file ");
        var args = new NodeParameters(tempFile, Logger, 
            false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);
        
        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}{file.Extension}");

        Assert.AreEqual($"D:/test/{fileName}.mkv", dest);
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
        string fileName = Guid.NewGuid().ToString();
        var tempFile = Path.Combine(Path.GetTempPath(),fileName+ ".mkv");
        System.IO.File.WriteAllText(tempFile, "this is a temp file ");
        var args = new NodeParameters(tempFile, Logger, 
            false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);

        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Name}{ext}");

        Assert.AreEqual($"D:/test/{fileName}.mkv", dest);
    }

    [TestMethod]
    public void MoveTests_Variable_Original_Filename()
    {
        string fileName = Guid.NewGuid().ToString();
        var tempFile = Path.Combine(Path.GetTempPath(),fileName+ ".mkv");
        System.IO.File.WriteAllText(tempFile, "this is a temp file ");
        var args = new NodeParameters(tempFile, Logger, 
            false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);
        
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileName}");

        Assert.AreEqual($@"D:/test/{fileName}.mkv", dest);
    }
    
    [TestMethod]
    public void MoveTests_Variable_Original_FilenameExt()
    {
        string fileName = Guid.NewGuid().ToString();
        var tempFile = Path.Combine(Path.GetTempPath(),fileName+ ".mkv");
        System.IO.File.WriteAllText(tempFile, "this is a temp file ");
        var args = new NodeParameters(tempFile, Logger, 
            false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);
        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileName}{file.Orig.Extension}");

        Assert.AreEqual($@"D:/test/{fileName}.mkv", dest);
    }
    [TestMethod]
    public void MoveTests_Variable_Original_NoExtension()
    {
        string fileName = Guid.NewGuid().ToString();
        var tempFile = Path.Combine(Path.GetTempPath(),fileName+ ".mkv.mkv");
        System.IO.File.WriteAllText(tempFile, "this is a temp file ");
        var args = new NodeParameters(tempFile, Logger, 
            false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);
        
        // ensure we dont double up the extension after FF-154
        string dest = MoveFile.GetDestinationPath(args, @"D:\test", "{file.Orig.FileNameNoExtension}");

        Assert.AreEqual($@"D:/test/{fileName}.mkv", dest);
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
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var tempDir2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir2);
        
        var tempFilePrefix = Path.Combine(tempDir, Guid.NewGuid().ToString());
        var tempFile = tempFilePrefix + ".mkv";
        System.IO.File.WriteAllText(tempFile, "test file");
        var tempFileSrt = tempFilePrefix + ".srt";
        System.IO.File.WriteAllText(tempFileSrt, "srt file");
        var args = new NodeParameters(tempFile, Logger, false, string.Empty, new LocalFileService());

        var ele = new MoveFile();
        ele.AdditionalFiles = new[] { "*.srt" };
        ele.DestinationPath = tempDir2;
        var result = ele.Execute(args);
        Assert.AreEqual(1, result);
    }
}

#endif