#if(DEBUG)

namespace VideoNodes.Tests
{
    using FileFlows.VideoNodes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class SubtitleExtractorTests: TestBase
    {
        [TestMethod]
        public void SubtitleExtractor_Extension_Test()
        {
            string file = TestFile_BasicMkv;
            var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
            var vii = vi.Read(file);

            foreach (string ext in new[] { String.Empty, ".srt", ".sup" })
            {
                SubtitleExtractor node = new();
                node.OutputFile = Path.Combine(TempPath, "subtitle.en" + ext);
                node.Language = "eng";

                var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
                args.GetToolPathActual = (string tool) => FfmpegPath;
                args.TempPath = TempPath;

                Assert.AreEqual(1, new VideoFile().Execute(args));

                int output = node.Execute(args);

                Assert.AreEqual(1, output);
            }
        }

        [TestMethod]
        public void SubtitleExtractor_Pgs_Test()
        {
            string file = TestFile_Pgs;
            var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
            var vii = vi.Read(file);

            foreach (string ext in new[] { String.Empty, ".srt", ".sup" })
            {
                SubtitleExtractor node = new();
                node.OutputFile = Path.Combine(TempPath, "subtitle.en" + ext);
                node.Language = "eng";

                var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
                args.GetToolPathActual = (string tool) => FfmpegPath;
                args.TempPath = TempPath;

                Assert.AreEqual(1, new VideoFile().Execute(args));

                int output = node.Execute(args);

                Assert.AreEqual(1, output);
            }
        }
    }
}


#endif