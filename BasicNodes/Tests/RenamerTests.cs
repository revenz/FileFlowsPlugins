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
        var ext = new FileInfo(TempFile).Extension;
        var args = new FileFlows.Plugin.NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "movie.Year", 1984 },
            { "viResolution", "1080P" }
        };
        args.InitFile(TempFile);


        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} [{viResolution}].{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $"{TempPath}/Ghostbusters (1984)/Ghostbusters [1080P]{ext}";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }
    
    [TestMethod]
    public void Renamer_Extension_DoubleDot()
    {
        var ext = new FileInfo(TempFile).Extension;
        var args = new FileFlows.Plugin.NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "movie.Year", 1984 },
            { "viResolution", "1080P" }
        };
        args.InitFile(TempFile);

        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} [{viResolution}].{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $"{TempPath}/Ghostbusters (1984)/Ghostbusters [1080P]{ext}";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }

    [TestMethod]
    public void Renamer_Empty_SquareBrackets()
    {
        var ext = new FileInfo(TempFile).Extension;
        var args = new FileFlows.Plugin.NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "movie.Year", 1984 }
        };
        args.InitFile(TempFile);

        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} [{viResolution}] {movie.Year}.{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $"{TempPath}/Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters 1984{ext}";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }

    [TestMethod]
    public void Renamer_Empty_RoundBrackets()
    {
        var ext = new FileInfo(TempFile).Extension;
        var args = new FileFlows.Plugin.NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "viResolution", "1080p" }
        };
        args.InitFile(TempFile);

        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} ({movie.Year}) {viResolution!}.{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $"{TempPath}/Ghostbusters{Path.DirectorySeparatorChar}Ghostbusters 1080P{ext}";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }
    
    [TestMethod]
    public void Renamer_Empty_SquareBrackets_Extension()
    {
        var ext = new FileInfo(TempFile).Extension;
        var args = new FileFlows.Plugin.NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Ghostbusters" },
            { "movie.Year", 1984 }
        };
        args.InitFile(TempFile);

        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title} [{viResolution}].{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $"{TempPath}/Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters{ext}";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }


    [TestMethod]
    public void Renamer_Colon()
    {
        var ext = new FileInfo(TempFile).Extension;
        var args = new FileFlows.Plugin.NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        args.Variables = new Dictionary<string, object>
        {
            { "movie.Title", "Batman Unlimited: Mech vs Mutants" },
            { "movie.Year", 2016 },
            { "ext", "mkv" }
        };
        args.InitFile(TempFile);

        Renamer node = new Renamer();
        node.Pattern = @"{movie.Title} ({movie.Year})\{movie.Title}.{ext}";
        node.LogOnly = true;

        var result = node.Execute(args);
        Assert.AreEqual(1, result);

        string expectedShort = $"{TempPath}/Batman Unlimited - Mech vs Mutants (2016){Path.DirectorySeparatorChar}Batman Unlimited - Mech vs Mutants{ext}";
        Assert.IsTrue(Logger.Contains($"Renaming file to: " + expectedShort));
    }
}


#endif