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
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "miYear", 1984 },
                { "viResolution", "1080P" }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{miTitle} ({miYear})\{miTitle} [{viResolution}]{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $@"c:\temp\Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters [1080P].mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }
        [TestMethod]
        public void Renamer_Extension_DoubleDot()
        {
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "miYear", 1984 },
                { "viResolution", "1080P" }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{miTitle} ({miYear})\{miTitle} [{viResolution}].{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $@"c:\temp\Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters [1080P].mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }

        [TestMethod]
        public void Renamer_Empty_SquareBrackets()
        {
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "miYear", 1984 }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{miTitle} ({miYear})\{miTitle} [{viResolution}] {miYear}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $@"c:\temp\Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters 1984.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }

        [TestMethod]
        public void Renamer_Empty_RoundBrackets()
        {
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "viResolution", "1080p" }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{miTitle} ({miYear})\{miTitle} ({miYear}) {viResolution!}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $@"c:\temp\Ghostbusters{Path.DirectorySeparatorChar}Ghostbusters 1080P.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }
        [TestMethod]
        public void Renamer_Empty_SquareBrackets_Extension()
        {
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "miYear", 1984 }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{miTitle} ({miYear})\{miTitle} [{viResolution}].{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $@"c:\temp\Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }


        [TestMethod]
        public void Renamer_Colon()
        {
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Batman Unlimited: Mech vs Mutants" },
                { "miYear", 2016 }
            };
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid()}.mkv", dontDelete: true);


            Renamer node = new Renamer();
            node.Pattern = @"{miTitle} ({miYear})\{miTitle}.{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $@"c:\temp\Batman Unlimited - Mech vs Mutants (2016){Path.DirectorySeparatorChar}Batman Unlimited - Mech vs Mutants.mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }
    }
}

#endif