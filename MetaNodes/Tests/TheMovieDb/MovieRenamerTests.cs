#if(DEBUG)

namespace MetaNodes.Tests.TheMovieDb
{
    using BasicNodes.Tests;
    using DM.MovieApi.MovieDb.Movies;
    using MetaNodes.TheMovieDb;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MovieMovieRenamerTests
    {
        [TestMethod]
        public void MovieRenamerTests_File_TitleYearExt()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Ghostbusters.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.SetParameter(Globals.MOVIE_INFO, new MovieInfo
            {
                Title = "Back to the Future Part II",
                ReleaseDate = new DateTime(1989, 5, 5)
            });

            MovieRenamer node = new MovieRenamer();
            node.Pattern = "{Title} ({Year}).{ext}";
            node.LogOnly = true;    

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            Assert.IsTrue(logger.Contains("Renaming file to: Back to the Future Part II (1989).mkv"));
        }

        [TestMethod]
        public void MovieRenamerTests_File_TitleExt()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Ghostbusters.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.SetParameter(Globals.MOVIE_INFO, new MovieInfo
            {
                Title = "Back to the Future Part II",
                ReleaseDate = new DateTime(1989, 5, 5)
            });

            MovieRenamer node = new MovieRenamer();
            node.Pattern = "{Title}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            Assert.IsTrue(logger.Contains("Renaming file to: Back to the Future Part II.mkv"));
        }

        [TestMethod]
        public void MovieRenamerTests_Folder_TitleYear_Windows()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Ghostbusters.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.SetParameter(Globals.MOVIE_INFO, new MovieInfo
            {
                Title = "Back to the Future Part II",
                ReleaseDate = new DateTime(1989, 5, 5)
            });

            MovieRenamer node = new MovieRenamer();
            node.Pattern = @"{Title} ({Year})\{Title}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            Assert.IsTrue(logger.Contains($"Renaming file to: Back to the Future Part II (1989){Path.DirectorySeparatorChar}Back to the Future Part II.mkv"));
        }

        [TestMethod]
        public void MovieRenamerTests_Folder_TitleYear_Linux()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Ghostbusters.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.SetParameter(Globals.MOVIE_INFO, new MovieInfo
            {
                Title = "Back to the Future Part II",
                ReleaseDate = new DateTime(1989, 5, 5)
            });

            MovieRenamer node = new MovieRenamer();
            node.Pattern = @"{Title} ({Year})/{Title}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            Assert.IsTrue(logger.Contains($"Renaming file to: Back to the Future Part II (1989){Path.DirectorySeparatorChar}Back to the Future Part II.mkv"));
        }

        [TestMethod]
        public void MovieRenamerTests_Folder_TitleYear_MoveActual()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mkv");
            string path = new FileInfo(tempFile).DirectoryName;
            File.WriteAllText(tempFile, "test");

            var args = new FileFlows.Plugin.NodeParameters(tempFile);
            var logger = new TestLogger();
            args.Logger = logger;
            args.SetParameter(Globals.MOVIE_INFO, new MovieInfo
            {
                Title = "Back to the Future Part II",
                ReleaseDate = new DateTime(1989, 5, 5)
            });

            MovieRenamer node = new MovieRenamer();
            node.Pattern = @"{Title} ({Year})/{Title}.{ext}";

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $"Back to the Future Part II (1989){Path.DirectorySeparatorChar}Back to the Future Part II.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));

            string expected = Path.Combine(path, expectedShort);
            Assert.IsTrue(File.Exists(expected));

            Directory.Delete(new FileInfo(expected).DirectoryName, true);
        }
    }
}

#endif