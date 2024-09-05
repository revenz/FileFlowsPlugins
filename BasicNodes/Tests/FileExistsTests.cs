#if(DEBUG)

using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class FileExistsTests
{
    [TestMethod]
    public void BasicTest()
    {            
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty, null);

        var element = new FileExists();
        element.FileName = "{folder.Orig.FullName}/{file.Orig.FileNameNoExtension}.en.srt";
        element.Execute(args);
    }
}

#endif