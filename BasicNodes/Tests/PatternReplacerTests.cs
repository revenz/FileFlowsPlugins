#if(DEBUG)

namespace BasicNodes.Tests
{
    using FileFlows.BasicNodes.Functions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PatternReplacerTests
    {
        [TestMethod]    
        public void PatternReplacer_Basic()
        {
            PatternReplacer node = new PatternReplacer();
            node.Replacements = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Seinfeld", "Batman")
            };
            node.UnitTest = true;
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Seinfeld.mkv", new TestLogger(), false, string.Empty);

            var result = node.Execute(args);
            Assert.AreEqual(1, result);
            Assert.AreEqual(@"c:\test\Batman.mkv", args.WorkingFile);
        }

        [TestMethod]
        public void PatternReplacer_Regex()
        {
            PatternReplacer node = new PatternReplacer();
            node.Replacements = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(@"s([\d]+)e([\d]+)", "$1x$2"),
                new KeyValuePair<string, string>(@"0([1-9]+x[\d]+)", "$1"),
            };
            node.UnitTest = true;
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\Seinfeld S03E06.mkv", new TestLogger(), false, string.Empty);

            var result = node.Execute(args);
            Assert.AreEqual(1, result);
            Assert.AreEqual(@"c:\test\Seinfeld 3x06.mkv", args.WorkingFile);
        }
    }
}

#endif