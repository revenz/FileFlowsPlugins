#if(DEBUG)

namespace BasicNodes.Tests
{
    using FileFlows.BasicNodes.File;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RenamerTests
    {
        [TestMethod]
        public void Renamer_Extension()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            var logger = new TestLogger();
            args.Logger = logger;   
            args.Variables = new Dictionary<string, object>
            {
                { "mi.Title", "Ghostbusters" },
                { "mi.Year", 1984 },
                { "vi.Resolution", "1080P" }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{mi.Title} ({mi.Year})\{mi.Title} [{vi.Resolution}]{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $"Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters [1080P].mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }
        [TestMethod]
        public void Renamer_Extension_DoubleDot()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.Variables = new Dictionary<string, object>
            {
                { "mi.Title", "Ghostbusters" },
                { "mi.Year", 1984 },
                { "vi.Resolution", "1080P" }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{mi.Title} ({mi.Year})\{mi.Title} [{vi.Resolution}].{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $"Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters [1080P].mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }

        [TestMethod]
        public void Renamer_Empty_SquareBrackets()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.Variables = new Dictionary<string, object>
            {
                { "mi.Title", "Ghostbusters" },
                { "mi.Year", 1984 }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{mi.Title} ({mi.Year})\{mi.Title} [{vi.Resolution}] {mi.Year}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $"Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters 1984.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }

        [TestMethod]
        public void Renamer_Empty_RoundBrackets()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.Variables = new Dictionary<string, object>
            {
                { "mi.Title", "Ghostbusters" },
                { "vi.Resolution", "1080p" }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{mi.Title} ({mi.Year})\{mi.Title} ({mi.Year}) {vi.Resolution!}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $"Ghostbusters{Path.DirectorySeparatorChar}Ghostbusters 1080P.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }
        [TestMethod]
        public void Renamer_Empty_SquareBrackets_Extension()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.Variables = new Dictionary<string, object>
            {
                { "mi.Title", "Ghostbusters" },
                { "mi.Year", 1984 }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{mi.Title} ({mi.Year})\{mi.Title} [{vi.Resolution}].{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $"Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }


        [TestMethod]
        public void Renamer_Colon()
        {
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            var logger = new TestLogger();
            args.Logger = logger;
            args.Variables = new Dictionary<string, object>
            {
                { "mi.Title", "Batman Unlimited: Mech vs Mutants" },
                { "mi.Year", 2016 }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{mi.Title} ({mi.Year})\{mi.Title}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $"Batman Unlimited - Mech vs Mutants (2016){Path.DirectorySeparatorChar}Batman Unlimited - Mech vs Mutants.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }
    }
}

#endif