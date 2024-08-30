#if(DEBUG)

using System.Diagnostics.CodeAnalysis;
using DM.MovieApi;
using DM.MovieApi.MovieDb.Movies;
using MetaNodes.TheMovieDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace MetaNodes.Tests.TheMovieDb;

[TestClass]
[TestCategory("Slow")]
public class MovieLookupTests : TestBase
{
    [TestMethod]
    public void MovieLookupTests_File_Ghostbusters()
    {
        var args = GetNodeParameters("Ghostbusters 1984.mkv");

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
        var args = GetNodeParameters("Ghostbusters 2.mkv");

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
        var args = GetNodeParameters("Back.To.The.Future.2.mkv");

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
        var args = GetNodeParameters("Back.To.The.Future.1989.mkv");

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);

        Assert.AreEqual("Back to the Future Part II", mi.Title.Replace("(", "").Replace(")", ""));
        Assert.AreEqual(1989, mi.ReleaseDate.Year);
    }

    [TestMethod]
    public void MovieLookupTests_Folder_WithYear()
    {
        var args = GetNodeParameters("Back To The Future (1989)/Jaws.mkv");

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
        var args = GetNodeParameters("Back To The Future (1989)/Jaws.mkv");

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
        var args = GetNodeParameters("sdfsdfdsvfdcxdsf.mkv");

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
        var args = GetNodeParameters("Constantine.2005.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}/Constantine.2005.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}.mkv");

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
        var args = GetNodeParameters("Wonder.Woman.1984.2020.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}/Wonder.Woman.1984.2020.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}.mkv");

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
    [RequiresUnreferencedCode("")]
    public void MovieLookupTests_File_TheBatman_Metadata()
    {
        MovieDbFactory.RegisterSettings(Globals.MovieDbBearerToken);
        var movieApi = MovieDbFactory.Create<IApiMovieRequest>().Value;
        var args = GetNodeParameters("Ghostbusters 1984.mkv");
        
        var md = MovieLookup.GetVideoMetadata(args, movieApi, 414906, @"D:\videos\temp");
        Assert.IsNotNull(md);
        string json = System.Text.Json.JsonSerializer.Serialize(md, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(@"D:\videos\metadata.json", json);
    }
    [TestMethod]
    public void MovieLookupTests_WonderWoman_Nfo()
    {
        var args = GetNodeParameters(@"Wonder.Woman.1984.2020.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}/Wonder.Woman.1984.2020.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}.mkv");

        MovieLookup ml = new MovieLookup();
        ml.UseFolderName = false;

        var result = ml.Execute(args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(args.Parameters.ContainsKey(Globals.MOVIE_INFO));

        var mi = args.Parameters[Globals.MOVIE_INFO] as MovieInfo;
        Assert.IsNotNull(mi);
        Assert.AreEqual("Wonder Woman 1984", mi.Title);
        Assert.AreEqual(2020, mi.ReleaseDate.Year);

        var eleNfo = new NfoFileCreator();
        result = eleNfo.Execute(args);
        Assert.AreEqual(1, result);
        string nfo = (string)args.Variables["NFO"];
        Assert.IsFalse(string.IsNullOrWhiteSpace(nfo));
    }
}

#endif