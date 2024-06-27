#if(DEBUG)

using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.TV;
using MetaNodes.TheMovieDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetaNodes.Tests.TheMovieDb;

[TestClass]
public class TVShowLookupTests : TestBase
{
    [TestMethod]
    public void TheBatman_Filename()
    {
        var args = GetNodeParameters("The Batman/Season 2/The Batman.s02e01.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = false;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("The Batman", info.Name);
        Assert.AreEqual(2004, info.FirstAirDate.Year);
        Assert.AreEqual("en", info.OriginalLanguage);
        Assert.AreEqual("The Batman", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2004, args.Variables["tvshow.Year"]);
    }
    
    [TestMethod]
    public void YearInFilename()
    {
        var args = GetNodeParameters("TestFolder/Eric.2024.S01.01.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = false;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("Eric", info.Name);
        Assert.AreEqual(2024, info.FirstAirDate.Year);
        Assert.AreEqual("en", info.OriginalLanguage);
        Assert.AreEqual("Eric", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2024, args.Variables["tvshow.Year"]);
    }
        
    [TestMethod]
    public void TvdbID_Test()
    {
        var args = GetNodeParameters("The Walking Dead (2010) [tvdbid-153021]/Season 07/S07E03 - The Cell [HDTV-1080p][AAC 5.1][h265].mkv");

        var element = new TVShowLookup();
        element.UseFolderName = true;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("The Walking Dead", info.Name);
        Assert.AreEqual(2010, info.FirstAirDate.Year);
        Assert.AreEqual("en", info.OriginalLanguage);
        Assert.AreEqual("The Walking Dead", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2010, args.Variables["tvshow.Year"]);
    }

    [TestMethod]
    public void TheBatman_Folder()
    {
        var args = GetNodeParameters("The Batman/Season 2/The Batman.s02e01.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = true;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("The Batman", info.Name);
        Assert.AreEqual(2004, info.FirstAirDate.Year);
        Assert.AreEqual("en", info.OriginalLanguage);
        Assert.AreEqual("The Batman", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2004, args.Variables["tvshow.Year"]);
    }
    
    [TestMethod]
    public void SquidGame_Filename()
    {
        var args = GetNodeParameters("Squid Game/Season 1/Squid.Game.1x01-02.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = false;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("Squid Game", info.Name);
        Assert.AreEqual(2021, info.FirstAirDate.Year);
        Assert.AreEqual("ko", info.OriginalLanguage);
        Assert.AreEqual("Squid Game", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2021, args.Variables["tvshow.Year"]);
    }
    
    [TestMethod]
    public void SquidGame_Folder()
    {
        var args = GetNodeParameters("Squid Game/Season 1/Squid.Game.1x01-02.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = true;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("Squid Game", info.Name);
        Assert.AreEqual(2021, info.FirstAirDate.Year);
        Assert.AreEqual("ko", info.OriginalLanguage);
        Assert.AreEqual("Squid Game", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2021, args.Variables["tvshow.Year"]);
    }

    [TestMethod]
    public void TVShowNameRegexTests()
    {
        foreach (var item in new[]
                 {
                     ("The Batman", "The.Batman.s2e01"),
                     ("Squid Game", "Squid.Game.1x01-02"),
                     ("Friends", "Friends - 10x15 - The One Where Estelle Dies"),
                     ("Another Show", "Another.Show.S01e03"),
                     ("Test Show", "Test.Show.5x09-12"),
                 })
        {
            var result = TVShowLookup.GetTVShowInfo(item.Item2);
            Assert.AreEqual(item.Item1, result.ShowName);
        }
    }

    [TestMethod]
    public void TVShowNameLookupTest()
    {
        string filename =
            "/media/downloads/complete/tv/Orange.is.the.New.Black.S01E06.2013.Bluray.1080p.DTS-MA.x264.dvxa-JohnGalt-Obfuscated/fdfdg43tetgfdsfdsfdsf.mkv";
        
        (string lookupName, string year) = TVShowLookup.GetLookupName(filename, true);
        Assert.AreEqual("Orange is the New Black", lookupName);
    }
}


#endif