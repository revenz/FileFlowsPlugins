#if(DEBUG)


using FileFlows.BasicNodes.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class PatternReplacerTests : TestBase
{
    [TestMethod]    
    public void PatternReplacer_Basic()
    {
        var testFile = System.IO.Path.Combine(TempPath, "Seinfeld.mkv");
        System.IO.File.Move(TempFile, testFile);
        PatternReplacer node = new PatternReplacer();
        node.Replacements = new List<KeyValuePair<string, string>>
        {
            new ("Seinfeld", "Batman")
        };
        node.UnitTest = true;
        var args = new FileFlows.Plugin.NodeParameters(testFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(testFile);

        var result = node.Execute(args);
        Assert.AreEqual(1, result);
        Assert.AreEqual(System.IO.Path.Combine(TempPath, "Batman.mkv"), args.WorkingFile);
    }

    [TestMethod]
    public void PatternReplacer_Regex()
    {
        var testFile = System.IO.Path.Combine(TempPath, "Seinfeld S03E06.mkv");
        System.IO.File.Move(TempFile, testFile);
        PatternReplacer node = new PatternReplacer();
        node.Replacements = new List<KeyValuePair<string, string>>
        {
            new (@"s([\d]+)e([\d]+)", "$1x$2"),
            new (@"0([1-9]+x[\d]+)", "$1"),
        };
        node.UnitTest = true;
        var args = new FileFlows.Plugin.NodeParameters(testFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(testFile);

        var result = node.Execute(args);
        Assert.AreEqual(1, result);
        Assert.AreEqual(System.IO.Path.Combine(TempPath, "Seinfeld 3x06.mkv"), args.WorkingFile);
    }
    
    [TestMethod]    
    public void PatternReplacer_Empty()
    {
        var testFile = System.IO.Path.Combine(TempPath, "Seinfeld.h265.mkv");
        System.IO.File.Move(TempFile, testFile);
        PatternReplacer node = new PatternReplacer();
        node.Replacements = new List<KeyValuePair<string, string>>
        {
            new (@"\.h265", "EMPTY")
        };
        node.UnitTest = true;
        var args = new FileFlows.Plugin.NodeParameters(testFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(testFile);
        
        var result = node.RunReplacements(args, args.WorkingFile);
        Assert.AreEqual(System.IO.Path.Combine(TempPath, "Seinfeld.mkv"), result);
    }
}


#endif