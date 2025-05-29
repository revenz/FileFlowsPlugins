#if(DEBUG)

using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.TV;
using MetaNodes.Helpers;
using MetaNodes.TheMovieDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PluginTestLibrary;

namespace MetaNodes.Tests.TheMovieDb;

[TestClass]
[TestCategory("Slow")]
public class TVShowLookupTests : TestBase
{
    /// <summary>
    /// Tests JSON serialization
    /// </summary>
    [TestMethod]
    public void DeserializeTest()
    {
        string json =
            "{\"Id\":80748,\"Name\":\"FBI\",\"OriginalName\":\"FBI\",\"PosterPath\":\"/bircXcCsvP2DUximyQUOn5p2s4.jpg\",\"BackdropPath\":\"/adOjU80AEcr70IPTXe6ALLyXGxF.jpg\",\"Popularity\":238.873,\"VoteAverage\":7.9,\"VoteCount\":810,\"Overview\":\"The New York office of the FBI brings to bear all their talents, intellect and technical expertise on major cases in order to keep their city and the country safe.\",\"FirstAirDate\":\"2018-09-25T00:00:00\",\"OriginCountry\":[\"US\"],\"Genres\":[{\"Id\":80,\"Name\":\"Crime\"},{\"Id\":10759,\"Name\":\"Action \\u0026 Adventure\"},{\"Id\":18,\"Name\":\"Drama\"}],\"OriginalLanguage\":\"en\"}";
        var tvShow = System.Text.Json.JsonSerializer.Deserialize<TVShowInfo>(json);
        Assert.IsNotNull(tvShow);
        Assert.AreEqual("FBI", tvShow.Name);
    }
    
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
    public void TeenTitans()
    {
        var args = GetNodeParameters("/Internal/Downloads/TV/Teen Titans Go! S09E051.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = false;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("Teen Titans Go!", info.Name);
        Assert.AreEqual(2013, info.FirstAirDate.Year);
        Assert.AreEqual("en", info.OriginalLanguage);
        Assert.AreEqual("Teen Titans Go!", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2013, args.Variables["tvshow.Year"]);
    }
    
    [TestMethod]
    public void Severence()
    {
        var args = GetNodeParameters("/data/downloads/complete/Severence S01 COMPLETE DS4K 1080p ATVP WEBRip AV1 Opus 5.1 [RAV1NE]/Severance.S01E09.The.We.We.Are.1080p.DS4K.ATVP.WEBRip.AV1.Opus.5.1-RAV1NE.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = false;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("Severance", info.Name);
        Assert.AreEqual(2022, info.FirstAirDate.Year);
        Assert.AreEqual("en", info.OriginalLanguage);
        Assert.AreEqual("Severance", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2022, args.Variables["tvshow.Year"]);
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
    public void YearInFilenameDotBeforeE()
    {
        var args = GetNodeParameters("TestFolder/Eric.2024.S01.e01.mkv");

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
    public void TvdbID_Test_German()
    {
        var args = GetNodeParameters("The Walking Dead (2010) [tvdbid-153021]/Season 07/S07E03 - The Cell [HDTV-1080p][AAC 5.1][h265].mkv");

        var element = new TVShowLookup();
        element.UseFolderName = true;
        element.Language = "de";

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
            var helper = new TVShowHelper(null);
            var result = helper.GetTVShowInfo(item.Item2);
            Assert.AreEqual(item.Item1, result.ShowName);
        }
    }

    [TestMethod]
    public void TVShowNameLookupTest()
    {
        string filename =
            "/media/downloads/complete/tv/Orange.is.the.New.Black.S01E06.2013.Bluray.1080p.DTS-MA.x264.dvxa-JohnGalt-Obfuscated/fdfdg43tetgfdsfdsfdsf.mkv";
        
        var helper = new TVShowHelper(null);
        (string lookupName, string year) = helper.GetLookupName(filename, true);
        Assert.AreEqual("Orange is the New Black", lookupName);
    }
    
    
    [TestMethod]
    public void TvdbId_FolderName_False()
    {
        var args = GetNodeParameters("/anime/Reincarnated as a Sword (2022) {tvdb-410378}/Season 01/Reincarnated as a Sword (2022) - S01E05 - 005 - [Bluray-1080p][10bit][x265][FLAC 2.0][EN+JA]-YURASUKA.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = false;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("Reincarnated as a Sword", info.Name);
        Assert.AreEqual(2022, info.FirstAirDate.Year);
        Assert.AreEqual("ja", info.OriginalLanguage);
        Assert.AreEqual("Reincarnated as a Sword", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2022, args.Variables["tvshow.Year"]);
    }
    
    
    
    [TestMethod]
    public void TvdbId_FolderName_True()
    {
        var args = GetNodeParameters("/anime/Reincarnated as a Sword (2022) {tvdb-410378}/Season 01/Reincarnated as a Sword (2022) - S01E05 - 005 - [Bluray-1080p][10bit][x265][FLAC 2.0][EN+JA]-YURASUKA.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = true;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("Reincarnated as a Sword", info.Name);
        Assert.AreEqual(2022, info.FirstAirDate.Year);
        Assert.AreEqual("ja", info.OriginalLanguage);
        Assert.AreEqual("Reincarnated as a Sword", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2022, args.Variables["tvshow.Year"]);
    }
    
    
    [TestMethod]
    public void SeasonFolder()
    {
        var args = GetNodeParameters("/mnt/tv/Missions.S01.German.AC3.DL.1080p.BluRay.x265-FuN/agffdgfdgfdg.mkv");

        var element = new TVShowLookup();
        element.UseFolderName = true;

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.TV_SHOW_INFO));

        var info = args.Parameters[Globals.TV_SHOW_INFO] as TVShowInfo;
        Assert.IsNotNull(info);

        Assert.AreEqual("Missions", info.Name);
        Assert.AreEqual(2017, info.FirstAirDate.Year);
        Assert.AreEqual("fr", info.OriginalLanguage);
        Assert.AreEqual("Missions", args.Variables["tvshow.Title"]);
        Assert.AreEqual(2017, args.Variables["tvshow.Year"]);
    }
}


#endif