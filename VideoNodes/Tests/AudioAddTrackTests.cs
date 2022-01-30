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
    public class AudioAddTrackTests
    {
        [TestMethod]
        public void AudioAddTrackTests_Mono_First()
        {
            const string file = @"D:\videos\unprocessed\The Witcher - S02E05 - Turn Your Back.mkv";
            var logger = new TestLogger();
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", logger);
            var vii = vi.Read(file);

            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";

            AudioAddTrack node = new();
            var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";

            new VideoFile().Execute(args);
            node.Bitrate = 128;
            node.Channels = 0;
            node.Index = 2;
            node.Codec = "aac";

            int output = node.Execute(args);
            string log = logger.ToString();
            Assert.AreEqual(1, output);
        }

    }
}


#endif