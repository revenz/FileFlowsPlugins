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

        [TestMethod]
        public void VideoInfoTest_DetectBlackBars()
        {
            const string file = @"D:\videos\unprocessed\The Witcher - S02E05 - Turn Your Back.mkv";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            vi.Read(@"D:\videos\unprocessed\Bourne.mkv");

            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger());
            args.GetToolPath = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";

            int result = new DetectBlackBars().Execute(args);

            Assert.IsTrue(result > 0);

        }
        [TestMethod]
        public void VideoInfoTest_NvidiaCard()
        {
            const string file = @"D:\videos\unprocessed\Bourne.mkv";
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger());
            //args.Process = new FileFlows.Plugin.ProcessHelper(args.Logger);

            var node = new VideoEncode();
            node.SetArgs(args);
            bool result = node.HasNvidiaCard(ffmpeg);

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void VideoInfoTest_CanEncodeNvidia()
        {
            const string file = @"D:\videos\unprocessed\Bourne.mkv";
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger());
            //args.Process = new FileFlows.Plugin.ProcessHelper(args.Logger);

            var node = new VideoEncode();
            node.SetArgs(args);
            bool result = node.CanProcessEncoder(ffmpeg, "hevc_nvenc -preset hq");

            Assert.IsTrue(result);
        }
    }
}

#endif