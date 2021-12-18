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
    public class VideoInfoHelperTests
    {
        [TestMethod]
        public void VideoInfoTest_JudgeDreed()
        {
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            vi.Read(@"D:\videos\unprocessed\Hellboy 2019 Bluray-1080p.mp4");

        }
        [TestMethod]
        public void VideoInfoTest_SubtitleRemover()
        {
            const string file = @"D:\videos\unprocessed\Bourne.mkv";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            vi.Read(@"D:\videos\unprocessed\Bourne.mkv");

            SubtitleRemover remover = new SubtitleRemover();
            remover.SubtitlesToRemove = new List<string>
            {
                "subrip", "srt"
            };
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger());
            args.GetToolPath = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";

            new VideoFile().Execute(args);

            int output = remover.Execute(args);

            Assert.AreEqual(1, output);

        }
    }
}

#endif