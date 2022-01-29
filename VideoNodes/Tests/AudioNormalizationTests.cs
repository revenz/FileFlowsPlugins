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
    public class AudioNormalizationTests
    {
        [TestMethod]
        public void AudioNormalization_Test_DoTwoPassMethod()
        {
            const string file = @"D:\videos\unprocessed\The IT Crowd - 2x04 - The Dinner Party.avi";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);

            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";

            AudioNormalization node = new();
            //node.OutputFile = file + ".sup";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";

            new VideoFile().Execute(args);

            string output = AudioNormalization.DoTwoPass(args, ffmpeg, 1);
            Assert.IsFalse(string.IsNullOrWhiteSpace(output));
        }

        [TestMethod]
        public void AudioNormalization_Test_TwoPass()
        {
            const string file = @"D:\videos\unprocessed\The IT Crowd - 2x04 - The Dinner Party.avi";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);

            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";

            AudioNormalization node = new();
            node.TwoPass = true;
            //node.OutputFile = file + ".sup";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";

            new VideoFile().Execute(args);

            int output = node.Execute(args);
            Assert.AreEqual(1, output);
        }
    }
}


#endif