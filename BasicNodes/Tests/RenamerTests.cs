#if(DEBUG)

using System.IO;
using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class RenamerTests : TestBase
{
    [TestMethod]
    public void Renamer_Extension()
    {
        
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "movie.Year", 1984 },
            { "viResolution", "1080P" }
        };
        args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} [{viResolution}]{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $@"c:\temp\Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters [1080P].mkv";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }
    [TestMethod]
    public void Renamer_Extension_DoubleDot()
    {
        
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "movie.Year", 1984 },
            { "viResolution", "1080P" }
        };
        args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} [{viResolution}].{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $@"c:\temp\Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters [1080P].mkv";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }

    [TestMethod]
    public void Renamer_Empty_SquareBrackets()
    {
        
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "movie.Year", 1984 }
        };
        args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} [{viResolution}] {movie.Year}.{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $@"c:\temp\Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters 1984.mkv";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }

    [TestMethod]
    public void Renamer_Empty_RoundBrackets()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "viResolution", "1080p" }
        };
        args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} ({movie.Year}) {viResolution!}.{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $@"c:\temp\Ghostbusters{Path.DirectorySeparatorChar}Ghostbusters 1080P.mkv";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }
    [TestMethod]
    public void Renamer_Empty_SquareBrackets_Extension()
    {
        
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "movie.Year", 1984 }
        };
        args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} [{viResolution}].{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $@"c:\temp\Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters.mkv";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }


    [TestMethod]
    public void Renamer_Colon()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Batman Unlimited: Mech vs Mutants" },
            { "movie.Year", 2016 },
            { "ext", "mkv" }
        };
        args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title}.{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $@"c:\temp\Batman Unlimited - Mech vs Mutants (2016){Path.DirectorySeparatorChar}Batman Unlimited - Mech vs Mutants.mkv";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }
}


#endif