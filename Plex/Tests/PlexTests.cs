#if(DEBUG)

using FileFlows.Plex.MediaManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Plex.Tests;

[TestClass]
public class PlexTests
{
    [TestMethod]
    public void Plex_Basic()
    {
        var args = new NodeParameters(@"/media/movies/The Batman (2022)/The Batman.mkv", new TestLogger(), false, string.Empty);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new PlexUpdater();
        Assert.AreEqual(1, node.Execute(args));
    }

    [TestMethod]
    public void Plex_Fail()
    {
        var args = new NodeParameters(@"/media/unknownmovies/The Batman (2022)/The Batman.mkv", new TestLogger(), false, string.Empty);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new PlexUpdater();
        Assert.AreEqual(2, node.Execute(args));
    }
}

#endif