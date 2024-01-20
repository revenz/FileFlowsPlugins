
#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.Plugin;
using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

[TestClass]
public class MoveTests2
{
    
    /// <summary>
    /// Tests that confirms additional files are moved
    /// </summary>
    [TestMethod]
    public void MoveTests_AdditionalFiles()
    {
        var logger = new TestLogger();
        var fileService = new LocalFileService();
        var args = new NodeParameters(@"/home/john/Videos/move-me/basic.mkv", logger, false, string.Empty, fileService);

        var ele = new MoveFile();
        ele.AdditionalFiles = new[] { "*.srt" };
        ele.DestinationPath = "/home/john/Videos/converted";
        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(1, result);
    }
}

#endif