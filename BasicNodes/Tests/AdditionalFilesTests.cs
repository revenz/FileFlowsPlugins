#if(DEBUG)

using System.IO;
using FileFlows.BasicNodes.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace BasicNodes.Tests;

/// <summary>
/// Tests fot he additional files method
/// </summary>
[TestClass]
public class AdditionalFilesTests: TestBase
{
    [TestMethod]
    public void Basic()
    {
        var fileService = new LocalFileService();
        var dir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        CreateFile(dir, "my movie.mkv");
        CreateFile(dir, "my movie.srt");
        CreateFile(dir, "my movie.en.sub");
        CreateFile(dir, "my movie.it.srt");
        CreateFile(dir, "not my movie.en.sub");
        CreateFile(dir, "not my movie.sub");
        CreateFile(dir, "not my movie.srt");
        var results = FolderHelper.GetAdditionalFiles(Logger, fileService, 
            (s, b, b2) => s, 
            "my movie", dir, [".srt", ".sub"]);
        Assert.AreEqual(3, results.Count);
        Assert.IsTrue(results.Contains(Path.Combine(dir, "my movie.srt")));
        Assert.IsTrue(results.Contains(Path.Combine(dir, "my movie.en.sub")));
        Assert.IsTrue(results.Contains(Path.Combine(dir, "my movie.it.srt")));

    }
    
    /// <summary>
    /// Creates a file
    /// </summary>
    /// <param name="directory">the directory to create the file in</param>
    /// <param name="name">the name of the file</param>
    private void CreateFile(string directory, string name)
    {
        System.IO.File.WriteAllText(Path.Combine(directory, name), "");
    }
}

#endif