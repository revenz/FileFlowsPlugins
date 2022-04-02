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
    public class VideoEncodeTests
    {
        [TestMethod]
        public void VideoEncode_EAC3_Test()
        {
            const string file = @"D:\videos\problemfile\sample fileflows.mkv";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);

            VideoEncode node = new();
            //node.OutputFile = file + ".sup";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";


            node.VideoCodec = "h265";
            node.AudioCodec = "aac";
            node.Language = "DE";


            new VideoFile().Execute(args);

            TestVideoInfo(args, "h264", "eac3");

            int output = node.Execute(args);

            Assert.AreEqual(1, output);
            TestVideoInfo(args, "hevc", "aac");
        }

        private void TestVideoInfo(FileFlows.Plugin.NodeParameters parameters, string videoCodec, string audioCodec)
        {
            Assert.AreEqual(videoCodec, parameters.Variables["vi.Video.Codec"]);
            Assert.AreEqual(audioCodec, parameters.Variables["vi.Audio.Codec"]);
            var videoInfo = parameters.Variables["vi.VideoInfo"] as VideoInfo;
            Assert.AreEqual(videoCodec, videoInfo.VideoStreams[0].Codec);
            Assert.AreEqual(audioCodec, videoInfo.AudioStreams[0].Codec);
        }

    }
}


#endif