#if(DEBUG)

using FileFlows.ComicNodes.Comics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Comic.Tests;

[TestClass]
public class ComicTests : TestBase
{
    [TestMethod]
    public void Comic_Pdf_To_Cbz()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"D:\comics\testfiles\fp1.pdf", logger, false, string.Empty, null);
        args.TempPath = @"D:\comics\temp";

        var node = new ComicConverter();
        node.Format = "cbz";
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Comic_Cbz_To_Pdf()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"D:\comics\testfiles\mb.cbz", logger, false, string.Empty, null);
        args.TempPath = @"D:\comics\temp";

        var node = new ComicConverter();
        node.Format = "pdf";
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Comic_Cb7_To_Cbz()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"D:\comics\testfiles\cb7.cb7", logger, false, string.Empty, null);
        args.TempPath = @"D:\comics\temp";

        var node = new ComicConverter();
        node.Format = "cbz";
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Comic_Cbr_To_Cbz()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"D:\comics\testfiles\bm001.cbr", logger, false, string.Empty, null);
        args.TempPath = @"D:\comics\temp";

        var node = new ComicConverter();
        node.Format = "cbz";
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Comic_Resize()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(@"/home/john/Comics/unprocessed/ASMANN001.cbz", logger, false, string.Empty, new LocalFileService());
        args.TempPath = @"/home/john/Comics/temp";

        var ele = new ComicConverter();
        ele.Format = "cbz";
        ele.MaxHeight = 800;
        ele.Codec = "webp";
        ele.Quality = 20;
        int result = ele.Execute(args);

        string log = logger.ToString();
        TestContext.WriteLine(log);
        Assert.AreEqual(1, result);
    }
}

#endif