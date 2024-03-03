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

}


#endif