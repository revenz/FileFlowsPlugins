#if(DEBUG)

using System.IO;
using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class FileSizeCompareTests : TestBase
{
    private string CreateFile(int size)
    {
        string tempFile = Path.Combine(TempPath, Guid.NewGuid() + ".tmp");  
        System.IO.File.WriteAllText(tempFile, new string('a', size * 1000));
        return tempFile;
    }
    
    [TestMethod]
    public void FileSize_Shrunk()
    {
        string tempFile = CreateFile(2);
        var args = new FileFlows.Plugin.NodeParameters(tempFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);

        string wfFile = CreateFile(1);
        args.SetWorkingFile(wfFile);

        FileSizeCompare node = new();
        int result = node.Execute(args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FileSize_Grew()
    {
        string tempFile = CreateFile(2);
        var args = new FileFlows.Plugin.NodeParameters(tempFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);

        string wfFile = CreateFile(20);
        args.SetWorkingFile(wfFile);

        FileSizeCompare node = new ();
        int result = node.Execute(args);
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void FileSize_SameSize()
    {
        string tempFile = CreateFile(2);
        var args = new FileFlows.Plugin.NodeParameters(tempFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);

        string wfFile = CreateFile(2);
        args.SetWorkingFile(wfFile);

        FileSizeCompare node = new();
        int result = node.Execute(args);
        Assert.AreEqual(2, result);
    }


    [TestMethod]
    public void FileSize_Shrunk_OriginalDeleted()
    {
        string tempFile = CreateFile(2);
        var args = new FileFlows.Plugin.NodeParameters(tempFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);
        Assert.IsTrue(args.WorkingFileSize > 0);
        System.IO.File.Delete(tempFile);

        string wfFile = CreateFile(1);
        args.SetWorkingFile(wfFile);

        FileSizeCompare node = new();
        int result = node.Execute(args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FileSize_Grew_OriginalDeleted()
    {
        string tempFile = CreateFile(2);
        var args = new FileFlows.Plugin.NodeParameters(tempFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);
        System.IO.File.Delete(tempFile);

        string wfFile = CreateFile(20);
        args.SetWorkingFile(wfFile);

        FileSizeCompare node = new();
        int result = node.Execute(args);
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void FileSize_SameSize_OriginalDeleted()
    {
        string tempFile = CreateFile(2);
        var args = new FileFlows.Plugin.NodeParameters(tempFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(tempFile);
        System.IO.File.Delete(tempFile);

        string wfFile = CreateFile(2);
        args.SetWorkingFile(wfFile);

        FileSizeCompare node = new();
        int result = node.Execute(args);
        Assert.AreEqual(2, result);
    }
}

#endif