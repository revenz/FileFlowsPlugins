#if(DEBUG)

namespace MetaNodes.Tests.TheMovieDb
{
    using BasicNodes.Tests;
    using DM.MovieApi.MovieDb.Movies;
    using MetaNodes.TheMovieDb;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MovieLookupTests
    {
        [TestMethod]
        public void MovieLookupTests_File_Ghostbusters()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Ghostbusters.mkv");
            args.Logger = new TestLogger();

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Ghostbusters 2.mkv");
            args.Logger = new TestLogger();

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Back.To.The.Future.2.mkv");
            args.Logger = new TestLogger();

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Back.To.The.Future.1989.mkv");
            args.Logger = new TestLogger();

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Back To The Future (1989)\Jaws.mkv");
            args.Logger = new TestLogger();

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
    }
}

#endif