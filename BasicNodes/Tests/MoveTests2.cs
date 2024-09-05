
#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.Plugin;
using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

[TestClass]
public class MoveTests2 : TestBase
{
    
    /// <summary>
    /// Tests that confirms additional files are moved
    /// </summary>
    [TestMethod]
    public void MoveTests_AdditionalFiles()
    {
        var additionalFile = System.IO.Path.Combine(TempPath, Guid.NewGuid() + ".srt");
        System.IO.File.WriteAllText(additionalFile, Guid.NewGuid().ToString());
        var destPath = System.IO.Path.Combine(TempPath, Guid.NewGuid().ToString());
        
        var args = new NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        args.InitFile(TempFile);

        var ele = new MoveFile();
        ele.AdditionalFiles = ["*.srt"];
        ele.DestinationPath = destPath;
        var result = ele.Execute(args);
        Assert.AreEqual(1, result);
    }
}

#endif