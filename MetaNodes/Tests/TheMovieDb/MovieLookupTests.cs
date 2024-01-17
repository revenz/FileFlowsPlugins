#if(DEBUG)

using DM.MovieApi;
using DM.MovieApi.MovieDb.Movies;
using MetaNodes.TheMovieDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetaNodes.Tests.TheMovieDb;

[TestClass]
public class MovieLookupTests
{
    [TestMethod]
    public void MovieLookupTests_File_Ghostbusters()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/Ghostbusters 1984.mkv", new TestLogger(), false, string.Empty, null);;

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);

        Assert.AreEqual("Ghostbusters", mi.Title);
        Assert.AreEqual(1984, mi.ReleaseDate.Year);
    }

    [TestMethod]
    public void MovieLookupTests_File_Ghostbusters2()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/Ghostbusters 2.mkv", new TestLogger(), false, string.Empty, null);;

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);

        Assert.AreEqual("Ghostbusters II", mi.Title);
        Assert.AreEqual(1989, mi.ReleaseDate.Year);
    }

    [TestMethod]
    public void MovieLookupTests_File_WithDots()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/Back.To.The.Future.2.mkv", new TestLogger(), false, string.Empty, null);;

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);

        Assert.AreEqual("Back to the Future Part II", mi.Title);
        Assert.AreEqual(1989, mi.ReleaseDate.Year);
    }

    [TestMethod]
    public void MovieLookupTests_File_WithYear()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/Back.To.The.Future.1989.mkv", new TestLogger(), false, string.Empty, null);;

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);

        Assert.AreEqual("Back to the Future Part II", mi.Title);
        Assert.AreEqual(1989, mi.ReleaseDate.Year);
    }

    [TestMethod]
    public void MovieLookupTests_Folder_WithYear()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/Back To The Future (1989)/Jaws.mkv", new TestLogger(), false, string.Empty, null);;

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = true;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);

        Assert.AreEqual("Back to the Future Part II", mi.Title);
        Assert.AreEqual(1989, mi.ReleaseDate.Year);
    }

    [TestMethod]
    public void MovieLookupTests_VariablesSet()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/Back To The Future (1989)/Jaws.mkv", new TestLogger(), false, string.Empty, null);;

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = true;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Variables.ContainsKey("movie.Title"));
        Assert.IsTrue(args.Variables.ContainsKey("movie.Year"));
        Assert.AreEqual("Back to the Future Part II", args.Variables["movie.Title"]);
        Assert.AreEqual(1989, args.Variables["movie.Year"]);
    }

    [TestMethod]
    public void MovieLookupTests_NoMatchNoVariables()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/sdfsdfdsvfdcxdsf.mkv", new TestLogger(), false, string.Empty, null);;

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(2, result);
        Assert.IsFalse(args.Variables.ContainsKey("movie.Title"));
        Assert.IsFalse(args.Variables.ContainsKey("movie.Year"));
    }


    [TestMethod]
    public void MovieLookupTests_ComplexFile()
    {
        var logger = new TestLogger(); 
        var args = new FileFlows.Plugin.NodeParameters(@"/test/Constantine.2005.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}/Constantine.2005.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}.mkv", logger, false, string.Empty, null);
        string log = logger.ToString();

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);

        Assert.AreEqual("Constantine", mi.Title);
        Assert.AreEqual(2005, mi.ReleaseDate.Year);
    }

    [TestMethod]
    public void MovieLookupTests_WonderWoman()
    {
        var args = new FileFlows.Plugin.NodeParameters(@"/test/Wonder.Woman.1984.2020.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}/Wonder.Woman.1984.2020.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}.mkv", new TestLogger(), false, string.Empty, null);;

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);

        Assert.AreEqual("Wonder Woman 1984", mi.Title);
        Assert.AreEqual(2020, mi.ReleaseDate.Year);
    }

    [TestMethod]
    public void MovieLookupTests_File_TheBatman_Metadata()
    {
        MovieDbFactory.RegisterSettings(MovieLookup.MovieDbBearerToken);
        var movieApi = MovieDbFactory.Create<IApiMovieRequest>().Value;
        var md = MovieLookup.GetVideoMetadata(movieApi, 414906, @"D:\videos\temp");
        Assert.IsNotNull(md);
        string json = System.Text.Json.JsonSerializer.Serialize(md, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(@"D:\videos\metadata.json", json);
    }
}

#endif