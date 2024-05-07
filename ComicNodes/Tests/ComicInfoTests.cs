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
            "/home/john/Comics/DC/Batman (1939)/Batman - #42 - Batman vs. Joker [old] [great] [amazing].cbr", 
            "/home/john/Comics",true);

        TestContext.WriteLine(Logger.ToString());

        Assert.IsFalse(result.IsFailed);
        var info = result.Value;
        Assert.IsNotNull(info);
        Assert.AreEqual("DC", info.Publisher);
        Assert.AreEqual("Batman (1939)", info.Series);
        Assert.AreEqual("1939", info.Volume);
        Assert.AreEqual("Batman vs. Joker", info.Title);
        Assert.AreEqual(3, info.Tags!.Length);
        Assert.AreEqual("old", info.Tags[0]);
        Assert.AreEqual("great", info.Tags[1]);
        Assert.AreEqual("amazing", info.Tags[2]);

        var xml = CreateComicInfo.SerializeToXml(info);
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
        TestContext.WriteLine(new string('-', 70));
        TestContext.WriteLine(xml);
        
        var name = CreateComicInfo.GetNewName(info, "cbz", 0);
        Assert.AreEqual("Batman (1939) - #42 - Batman vs. Joker (old) (great) (amazing).cbz", name.Value);
    }
    
    [TestMethod]
    public void VolumeYear()
    {
        var result = CreateComicInfo.GetInfo(Logger,
            "/home/john/Comics/actual/DC/He-Man and the Masters of the Universe/He-Man and the Masters of the Universe (2013)/He-Man and the Masters of the Universe - #123 - Desperate Times.cbr",
            "/home/john/Comics/actual",
            true);

        TestContext.WriteLine(Logger.ToString());

        Assert.IsFalse(result.IsFailed);
        var info = result.Value;
        Assert.IsNotNull(info);
        Assert.AreEqual("DC", info.Publisher);
        Assert.AreEqual("He-Man and the Masters of the Universe (2013)", info.Series);
        Assert.AreEqual("2013", info.Volume);
        Assert.AreEqual("Desperate Times", info.Title);

        var xml = CreateComicInfo.SerializeToXml(info);
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
        TestContext.WriteLine(new string('-', 70));
        TestContext.WriteLine(xml);
        
        var name = CreateComicInfo.GetNewName(info, "cbz", 2);
        Assert.AreEqual("He-Man and the Masters of the Universe (2013) - #123 - Desperate Times.cbz", name.Value);
    }

    [TestMethod]
    public void Volume()
    {
        var result = CreateComicInfo.GetInfo(Logger,
            "/home/john/Comics/Marvel/Ultimate Spider-Man (2000)/Ultimate Spider-Man - v05 - Public Scrutiny (junk) (remove0245).cbz", 
            "/home/john/Comics",
            true);

        TestContext.WriteLine(Logger.ToString());

        Assert.IsFalse(result.IsFailed);
        var info = result.Value;
        Assert.IsNotNull(info);
        Assert.AreEqual("Marvel", info.Publisher);
        Assert.AreEqual("Ultimate Spider-Man (2000)", info.Series);
        Assert.AreEqual("Volume 5", info.Volume);
        Assert.AreEqual("Public Scrutiny", info.Title);

        var xml = CreateComicInfo.SerializeToXml(info);
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
        TestContext.WriteLine(new string('-', 70));
        TestContext.WriteLine(xml);
        
        var name = CreateComicInfo.GetNewName(info, "cbz", 0);
        Assert.AreEqual("Ultimate Spider-Man (2000) - Volume 5 - Public Scrutiny.cbz", name.Value);
    }
    
    
    [TestMethod]
    public void NameAndNumber()
    {
        var result = CreateComicInfo.GetInfo(Logger,
            "/home/john/Comics/Marvel/X-Men/X-Man/X-Man 45.cbz", 
            "/home/john/Comics",
            true);

        TestContext.WriteLine(Logger.ToString());

        Assert.IsFalse(result.IsFailed);
        var info = result.Value;
        Assert.IsNotNull(info);
        Assert.AreEqual("Marvel", info.Publisher);
        Assert.AreEqual("X-Man", info.Series);
        Assert.AreEqual(45, info.Number);

        var xml = CreateComicInfo.SerializeToXml(info);
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
        TestContext.WriteLine(new string('-', 70));
        TestContext.WriteLine(xml);
        
        var name = CreateComicInfo.GetNewName(info, "cbz", 3);
        Assert.AreEqual("X-Man - #045.cbz", name.Value);
    }

    
    [TestMethod]
    public void PhysicalFileTest()
    {
        const string FILE =
            "/home/john/Comics/DC/Batman (1939)/Batman - #1 - Batman vs. Joker [old] [great] [amazing].cbz";
        var logger = new TestLogger();
        var args = new NodeParameters(FILE, logger, false, string.Empty, new LocalFileService())
        {
            LibraryFileName = FILE,
            LibraryPath = "/home/john/Comics"
        };
        args.TempPath = TempPath;

        var ele = new CreateComicInfo();
        var result = ele.Execute(args);

        TestContext.WriteLine(Logger.ToString());

        Assert.AreEqual(1, result);
    }
}


#endif