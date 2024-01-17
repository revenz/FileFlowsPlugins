#if(DEBUG)

using FileFlows.ComicNodes.Comics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Comic.Tests;

[TestClass]
public class ExtractTests
{
    [TestMethod]
    public void Extract_Pdf()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"D:\comics\testfiles\fp1.pdf", logger, false, string.Empty, null);

        var node = new ComicExtractor();
        node.DestinationPath = @"D:\comics\converted\pdf";
        if (Directory.Exists(node.DestinationPath))
            Directory.Delete(node.DestinationPath, true);   
        Directory.CreateDirectory(node.DestinationPath);
        int result = node.Execute(args);
        
        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Extract_Cbr()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"D:\comics\testfiles\bm001.cbr", logger, false, string.Empty, null);

        var node = new ComicExtractor();
        node.DestinationPath = @"D:\comics\converted\cbr";
        if (Directory.Exists(node.DestinationPath))
            Directory.Delete(node.DestinationPath, true);
        Directory.CreateDirectory(node.DestinationPath);
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Extract_Cbz()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"D:\comics\testfiles\mb.cbz", logger, false, string.Empty, null);

        var node = new ComicExtractor();
        node.DestinationPath = @"D:\comics\converted\cbz";
        if (Directory.Exists(node.DestinationPath))
            Directory.Delete(node.DestinationPath, true);
        Directory.CreateDirectory(node.DestinationPath);
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Extract_Cb7()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"D:\comics\testfiles\cb7.cb7", logger, false, string.Empty, null);

        var node = new ComicExtractor();
        node.DestinationPath = @"D:\comics\converted\cb7";
        if(Directory.Exists(node.DestinationPath))
            Directory.Delete(node.DestinationPath, true);
        Directory.CreateDirectory(node.DestinationPath);
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }
}

#endif