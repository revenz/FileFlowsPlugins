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
            node.Pattern = @"{mi.Title} ({mi.Year})\{mi.Title} [{vi.Resolution}].{ext}";
            node.LogOnly = true;

            var result = node.Execute(args);
            Assert.AreEqual(1, result);

            string expectedShort = $"Ghostbusters (1984){Path.DirectorySeparatorChar}Ghostbusters [1080P].mkv";
            Assert.IsTrue(logger.Contains($"Renaming file to: " + expectedShort));
        }

    }
}

#endif