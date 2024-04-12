#if(DEBUG)

using System.Diagnostics.CodeAnalysis;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.TV;
using MetaNodes.TheMovieDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetaNodes.Tests.TheMovieDb;

[TestClass]
public class TVEpisodeLookupTests
{
    /// <summary>
    /// The test context instance
    /// </summary>
    private TestContext testContextInstance;

    /// <summary>
    /// Gets or sets the test context
    /// </summary>
    public TestContext TestContext
    {
        get { return testContextInstance; }
        set { testContextInstance = value; }
    }
    
    [TestMethod]
    public void TheBatman_s02e01()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters("/test/tv/The Batman/Season 2/The Batman.s02e01.mkv", logger, false, string.Empty, null);

        var element = new TVEpisodeLookup();

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        
        Assert.AreEqual("The Batman", args.Variables["tvepisode.Title"]);
        Assert.AreEqual(2, args.Variables["tvepisode.Season"]);
        Assert.AreEqual(1, args.Variables["tvepisode.Episode"]);
        Assert.AreEqual("The Cat, the Bat and the Very Ugly", args.Variables["tvepisode.Subtitle"]);
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["tvepisode.Overview"] as string));
    }
    
    [TestMethod]
    public void TheBatman_2x03()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters("/test/tv/The Batman/Season 2/The Batman - 2x03.mkv", logger, false, string.Empty, null);

        var element = new TVEpisodeLookup();

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        
        Assert.AreEqual("The Batman", args.Variables["tvepisode.Title"]);
        Assert.AreEqual(2, args.Variables["tvepisode.Season"]);
        Assert.AreEqual(3, args.Variables["tvepisode.Episode"]);
        Assert.AreEqual("Fire & Ice", args.Variables["tvepisode.Subtitle"]);
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["tvepisode.Overview"] as string));
    }
    
    [TestMethod]
    public void WithYear()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters("/test/tv/Paradise PD (2018) - S04E04 - Good Jeans (1080p NF WEB-DL x265 t3nzin).mkv", logger, false, string.Empty, null);

        var element = new TVEpisodeLookup();

        var result = element.Execute(args);
        TestContext.WriteLine(logger.ToString());
        
        Assert.AreEqual(1, result);
        
        
        Assert.AreEqual("Paradise PD", args.Variables["tvepisode.Title"]);
        Assert.AreEqual(4, args.Variables["tvepisode.Season"]);
        Assert.AreEqual(4, args.Variables["tvepisode.Episode"]);
        Assert.AreEqual("Good Jeans", args.Variables["tvepisode.Subtitle"]);
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["tvepisode.Overview"] as string));
        
    }
    
    [TestMethod]
    public void TheBatman_3x01_2()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters("/test/tv/The Batman/Season 3/The Batman - 3x01-2.mkv", logger, false, string.Empty, null);

        var element = new TVEpisodeLookup();

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        
        Assert.AreEqual("The Batman", args.Variables["tvepisode.Title"]);
        Assert.AreEqual(3, args.Variables["tvepisode.Season"]);
        Assert.AreEqual(1, args.Variables["tvepisode.Episode"]);
        Assert.AreEqual(2, args.Variables["tvepisode.LastEpisode"]);
        Assert.AreEqual("Batgirl Begins (1)", args.Variables["tvepisode.Subtitle"]);
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["tvepisode.Overview"] as string));
    }
    
    [TestMethod]
    public void TheBatman_s4e12_13()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters("/test/tv/The Batman/Season 4/The Batman - s4e12-13.mkv", logger, false, string.Empty, null);

        var element = new TVEpisodeLookup();

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        
        Assert.AreEqual("The Batman", args.Variables["tvepisode.Title"]);
        Assert.AreEqual(4, args.Variables["tvepisode.Season"]);
        Assert.AreEqual(12, args.Variables["tvepisode.Episode"]);
        Assert.AreEqual(13, args.Variables["tvepisode.LastEpisode"]);
        Assert.AreEqual("The Joining (1)", args.Variables["tvepisode.Subtitle"]);
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["tvepisode.Overview"] as string));
    }
    
    [TestMethod]
    public void TheBatman_s5e1_2_3()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters("/test/tv/The Batman/Season 5/The Batman - s5e1-3.mkv", logger, false, string.Empty, null);

        var element = new TVEpisodeLookup();

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        
        Assert.AreEqual("The Batman", args.Variables["tvepisode.Title"]);
        Assert.AreEqual(5, args.Variables["tvepisode.Season"]);
        Assert.AreEqual(1, args.Variables["tvepisode.Episode"]);
        Assert.AreEqual(3, args.Variables["tvepisode.LastEpisode"]);
        Assert.AreEqual("The Batman/Superman Story (1)", args.Variables["tvepisode.Subtitle"]);
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["tvepisode.Overview"] as string));
    }
    
    
    
    [TestMethod]
    public void TheBatman_2x03_nfo()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters("/test/tv/The Batman/Season 2/The Batman - 2x03.mkv", logger, false, string.Empty, null);

        var element = new TVEpisodeLookup();

        var result = element.Execute(args);
        Assert.AreEqual(1, result);

        var eleNfo = new NfoFileCreator();
        result = eleNfo.Execute(args);
        Assert.AreEqual(1, result);

        TVShowInfo tvShowInfo = (TVShowInfo)args.Variables[Globals.TV_SHOW_INFO];
        Episode epInfo = (Episode)args.Variables[Globals.TV_EPISODE_INFO];
        string nfo = eleNfo.CreateTvShowNfo(args, tvShowInfo, epInfo);
        Assert.IsNotNull(nfo);
    }
    
    [TestMethod]
    public void TheBatman_s5e1_2_3_Nfo()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters("/test/tv/The Batman/Season 5/The Batman - s5e1-3.mkv", logger, false, string.Empty, null);

        var element = new TVEpisodeLookup();

        var result = element.Execute(args);
        Assert.AreEqual(1, result);
        
        var eleNfo = new NfoFileCreator();
        result = eleNfo.Execute(args);
        Assert.AreEqual(1, result);
        string nfo = (string)args.Variables["NFO"];
    }

    
    [TestMethod]
    public void VariousTests()
    {
        var logger = new TestLogger();
        foreach (var test in new TVLookupTestData[]
                 {
                     new("See",
                         "/media/TV/See (2019) [tvdbid-361565]/Season 02/S02E06 - The Truth About Unicorns ATVP WEBDL-2160pDV]EAC3 Atmos 5.1h265]-MP4.mp4"),
                     new("The Walking Dead",
                         "/media/TV/The Walking Dead (2010) [tvdbid-153021]/Season 07/S07E04 - Service HDTV-1080pAAC 5.1]h265mkv"),
                     new("Teenage Mutant Ninja Turtles",
                         "/media/Anime/Teenage Mutant Ninja Turtles (1987) [tvdbid-74582]/Season 04/S04E11 - 089 - Menace Maestro Please SDTV10bit]x264AC3 2.0].mkv"),
                     new("Yowamushi Pedal",
                         "/media/Anime/Yowamushi Pedal (2013) [tvdbid-272309]/Season 03/S03E12 - 074 - Trouble! HDTV-1080p8bit]x264Opus 2.0]JAmkv"),
                     new("Law & Order - Special Victims Unit",
                         "/media/TV/Law & Order - Special Victims Unit (1999) [tvdbid-75692]/Season 07/S07E04 - Ripped AMZN WEBRip-1080p ProperEAC3 5.1]x264NTb.mkv"),
                     new("X-Men '97", "/media/tv/X-Men '97 (2024) [tvdb-3423432]/xmen97.3x03.mkv"),
                     new("Phantom 2040", "/media/tv/Phantom 2040 (1992)/Phantom 2040.1x03.mkv"),
                 })
        {
            (string lookupName, string year) = TVShowLookup.GetLookupName(test.Path, true);
            Assert.AreEqual(test.Show, lookupName);
            //
            // var args = new FileFlows.Plugin.NodeParameters(test.Path, logger, false, string.Empty, null);
            //
            // var element = new TVEpisodeLookup();
            //
            // var result = element.Execute(args);
            // Assert.AreEqual(1, result);
            //
            // Assert.AreEqual(test.Show, args.Variables["tvepisode.Title"]);
            
        }
    }

    private record TVLookupTestData(string Show, string Path);
}

#endif