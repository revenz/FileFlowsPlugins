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
    public class SubtitleLanguageRemoverTests
    {
        [TestMethod]
        public void SubtitleLanguageRemover_Test_01()
        {
            const string file = @"D:\videos\unprocessed\Injustice.mkv";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);

            SubtitleLanguageRemover node = new();
            node.Pattern = "eng";
            node.NotMatching = true;
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";


            new VideoFile().Execute(args);

            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }
    }
}


#endif