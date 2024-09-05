#if(DEBUG)

using System.IO;
using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class DeleteSourceDirectoryTests : TestBase
{
    
    [TestMethod]
    public void AtLibraryRoot()
    {
        var args = GetNodeParameters(TempFile);
        args.RelativeFile = TempFile[(TempPath.Length + 1)..];
        args.LibraryPath = TempPath;

        var element = new DeleteSourceDirectory();
        element.TopMostOnly = true;
        element.PreExecute(args);
        
        Assert.AreEqual(2, element.Execute(args));
        Assert.IsTrue(Logger.ToString().Contains("Cannot delete library root: "));
    }
    
    [TestMethod]
    public void DeleteSub()
    {
        string sub1 = Path.Combine(TempPath, "sub1");
        string sub2 = Path.Combine(sub1, "sub2");
        Directory.CreateDirectory(sub2);
        string newTemp = Path.Combine(sub2, "testfile.txt");
        System.IO.File.Move(TempFile, newTemp);
        
        var args = GetNodeParameters(newTemp);
        args.RelativeFile = newTemp[(TempPath.Length + 1)..];
        args.LibraryPath = TempPath;

        var element = new DeleteSourceDirectory();
        element.TopMostOnly = true;
        element.PreExecute(args);
        
        Assert.AreEqual(1, element.Execute(args));
        Assert.IsTrue(Directory.Exists(sub1));
        Assert.IsFalse(Directory.Exists(sub2));
    }
    
    [TestMethod]
    public void DeleteSubNotEmpty()
    {
        string sub1 = Path.Combine(TempPath, "sub1");
        string sub2 = Path.Combine(sub1, "sub2");
        Directory.CreateDirectory(sub2);
        string newTemp = Path.Combine(sub2, "testfile.txt");
        System.IO.File.Move(TempFile, newTemp);
        
        var args = GetNodeParameters(newTemp);
        args.RelativeFile = newTemp[(TempPath.Length + 1)..];
        args.LibraryPath = TempPath;

        var element = new DeleteSourceDirectory();
        element.TopMostOnly = true;
        element.IfEmpty = true;
        element.PreExecute(args);
        
        Assert.AreEqual(2, element.Execute(args));
        Assert.IsTrue(Directory.Exists(sub1));
        Assert.IsTrue(Directory.Exists(sub2));
    }
    
    [TestMethod]
    public void DeleteSubNotEmpty_Matching_1()
    {
        string sub1 = Path.Combine(TempPath, "sub1");
        string sub2 = Path.Combine(sub1, "sub2");
        Directory.CreateDirectory(sub2);
        string newTemp = Path.Combine(sub2, "testfile.txt");
        System.IO.File.Move(TempFile, newTemp);
        
        var args = GetNodeParameters(newTemp);
        args.RelativeFile = newTemp[(TempPath.Length + 1)..];
        args.LibraryPath = TempPath;

        var element = new DeleteSourceDirectory();
        element.TopMostOnly = true;
        element.IfEmpty = true;
        element.IncludePatterns = [@"\.txt$"];
        element.PreExecute(args);
        
        Assert.AreEqual(2, element.Execute(args));
        Assert.IsTrue(Directory.Exists(sub1));
        Assert.IsTrue(Directory.Exists(sub2));
    }
    
    [TestMethod]
    public void DeleteSubNotEmpty_Matching_2()
    {
        string sub1 = Path.Combine(TempPath, "sub1");
        string sub2 = Path.Combine(sub1, "sub2");
        Directory.CreateDirectory(sub2);
        string newTemp = Path.Combine(sub2, "testfile.txt");
        System.IO.File.Move(TempFile, newTemp);
        
        var args = GetNodeParameters(newTemp);
        args.RelativeFile = newTemp[(TempPath.Length + 1)..];
        args.LibraryPath = TempPath;

        var element = new DeleteSourceDirectory();
        element.TopMostOnly = true;
        element.IfEmpty = true;
        element.IncludePatterns = [@"\.csv"];
        element.PreExecute(args);
        
        Assert.AreEqual(1, element.Execute(args));
        Assert.IsTrue(Directory.Exists(sub1));
        Assert.IsFalse(Directory.Exists(sub2));
    }
}
#endif