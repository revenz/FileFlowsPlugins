#if(DEBUG)

using BasicNodes.File;

namespace BasicNodes.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class WriteTextTests : TestBase
{
    [TestMethod]
    public void WorkingFile_Csv()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/file.mkv", new TestLogger(), false, string.Empty, new LocalFileService());

        var output = WriteText.GetText(args, "", "file.csv");
        Assert.AreEqual("\"/test/file.mkv\"", output);
    }
    
    [TestMethod]
    public void WorkingFile_Text()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/file.mkv", new TestLogger(), false, string.Empty, new LocalFileService());

        var output = WriteText.GetText(args, "", "file.txt");
        Assert.AreEqual("/test/file.mkv", output);
    }
    
    [TestMethod]
    public void CsvArgs()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/file.mkv", new TestLogger(), false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "file.mkv";
        args.Variables["ext"] = "mkv";

        var output = WriteText.GetText(args, "{file.Name};{ext}", "file.csv");
        Assert.AreEqual("\"file.mkv\",\"mkv\"", output);
    }
    
    [TestMethod]
    public void CsvArg()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/file.mkv", new TestLogger(), false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "file.mkv";

        var output = WriteText.GetText(args, "{file.Name}", "file.csv");
        Assert.AreEqual("\"file.mkv\"", output);
    }
}

#endif