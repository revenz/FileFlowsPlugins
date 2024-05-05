#if(DEBUG)

using FileFlows.ComicNodes.Comics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Comic.Tests;

[TestClass]
public class ComicInfoTests : TestBase
{
    [TestMethod]
    public void Basic()
    {
        var result = CreateComicInfo.GetInfo(Logger,
            "/home/john/comics/DC/Batman (1939)/Batman - #1 - Batman vs. Joker [old] [great] [amazing].cbr", true);

        TestContext.WriteLine(Logger.ToString());

        Assert.IsFalse(result.IsFailed);
        var info = result.Value;
        Assert.IsNotNull(info);
        Assert.AreEqual("DC", info.Publisher);
        Assert.AreEqual("Batman", info.Series);
        Assert.AreEqual("1939", info.Volume);
        Assert.AreEqual("Batman vs. Joker", info.Title);
        Assert.AreEqual(3, info.Tags.Length);
        Assert.AreEqual("old", info.Tags[0]);
        Assert.AreEqual("great", info.Tags[1]);
        Assert.AreEqual("amazing", info.Tags[2]);

        var xml = CreateComicInfo.SerializeToXml(info);
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
        TestContext.WriteLine(new string('-', 70));
        TestContext.WriteLine(xml);
    }

    [TestMethod]
    public void PhysicalFileTest()
    {
        const string FILE =
            "/home/john/Comics/DC/Batman (1939)/Batman - #1 - Batman vs. Joker [old] [great] [amazing].cbz";
        var logger = new TestLogger();
        var args = new NodeParameters(FILE, logger, false, string.Empty, new LocalFileService())
        {
            LibraryFileName = FILE
        };
        args.TempPath = TempPath;

        var ele = new CreateComicInfo();
        var result = ele.Execute(args);

        TestContext.WriteLine(Logger.ToString());

        Assert.AreEqual(1, result);
    }
}


#endif