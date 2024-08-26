#if(DEBUG)

using FileFlows.Plex.MediaManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Plex.Tests;

[TestClass]
public class PlexUpdaterTests : TestBase
{
    [TestMethod]
    public void Plex_Basic()
    {
        var args = new NodeParameters(@"/media/movies/The Batman (2022)/The Batman.mkv", 
            Logger, false, string.Empty, new LocalFileService());
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
        var args = new NodeParameters(@"/media/unknownmovies/The Batman (2022)/The Batman.mkv", 
            Logger, false, string.Empty, new LocalFileService());
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new PlexUpdater();
        Assert.AreEqual(2, node.Execute(args));
    }

    [TestMethod]
    public void Plex_Mapping()
    {
        var args = new NodeParameters(@"/mnt/movies/The Batman (2022)/The Batman.mkv", 
            Logger, false, string.Empty, new LocalFileService());
        var settings = new PluginSettings();
        settings.Mapping = new List<KeyValuePair<string, string>>();
        settings.Mapping.Add(new KeyValuePair<string, string>("/mnt/movies", "/media/movies"));

        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new PlexUpdater();
        Assert.AreEqual(1, node.Execute(args));
    }

}

#endif