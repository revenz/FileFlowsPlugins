#if(DEBUG)

using FileFlows.Emby.MediaManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Emby.Tests;

[TestClass]
public class EmbyTests
{
    [TestMethod]
    public void Emby_Basic()
    {
        var args = new NodeParameters(@"/media/movies/Citizen Kane (1941)/Citizen Kane (1941).mp4", new TestLogger(), false, string.Empty);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new EmbyUpdater();
        Assert.AreEqual(1, node.Execute(args));
    }

    [TestMethod]
    public void Emby_Fail()
    {
        var args = new NodeParameters(@"/media/unknownmovies/The Batman (2022)/The Batman.mkv", new TestLogger(), false, string.Empty);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.invalid.json");
        };

        var node = new EmbyUpdater();
        Assert.AreEqual(2, node.Execute(args));
    }

    [TestMethod]
    public void Emby_Mapped()
    {
        var args = new NodeParameters(@"/mnt/movies/Citizen Kane (1941)/Citizen Kane (1941).mp4", new TestLogger(), false, string.Empty);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new EmbyUpdater();
        Assert.AreEqual(1, node.Execute(args));
    }
}

#endif