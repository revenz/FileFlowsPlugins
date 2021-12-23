#if(DEBUG)

namespace MetaNodes.Tests.TheMovieDb
{
    using DM.MovieApi.MovieDb.Movies;
    using MetaNodes.TheMovieDb;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MovieLookupTests
    {
        [TestMethod]
        public void MovieLookupTests_File_Ghostbusters()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Ghostbusters 1984.mkv", new TestLogger(), false, string.Empty);

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Ghostbusters 2.mkv", new TestLogger(), false, string.Empty);

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Back.To.The.Future.2.mkv", new TestLogger(), false, string.Empty);

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Back.To.The.Future.1989.mkv", new TestLogger(), false, string.Empty);

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Back To The Future (1989)\Jaws.mkv", new TestLogger(), false, string.Empty);

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Back To The Future (1989)\Jaws.mkv", new TestLogger(), false, string.Empty);

            MovieLookup ml = new MovieLookup();
            ml.UseFolderName = true;

            var result = ml.Execute(args);
            Assert.AreEqual(1, result);
            Assert.IsTrue(args.Variables.ContainsKey("miTitle"));
            Assert.IsTrue(args.Variables.ContainsKey("miYear"));
            Assert.AreEqual("Back to the Future Part II", args.Variables["miTitle"]);
            Assert.AreEqual(1989, args.Variables["miYear"]);
        }

        [TestMethod]
        public void MovieLookupTests_NoMatchNoVariables()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", new TestLogger(), false, string.Empty);

            MovieLookup ml = new MovieLookup();
            ml.UseFolderName = false;

            var result = ml.Execute(args);
            Assert.AreEqual(2, result);
            Assert.IsFalse(args.Variables.ContainsKey("miTitle"));
            Assert.IsFalse(args.Variables.ContainsKey("miYear"));
        }


        [TestMethod]
        public void MovieLookupTests_ComplexFile()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Constantine.2005.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}\Constantine.2005.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}.mkv", new TestLogger(), false, string.Empty);

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Wonder.Woman.1984.2020.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}\Wonder.Woman.1984.2020.German.DL.AC3.1080p.BluRay.x265-Fun{{fdg$ERGESDG32fesdfgds}}.mkv", new TestLogger(), false, string.Empty);

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
    }
}

#endif