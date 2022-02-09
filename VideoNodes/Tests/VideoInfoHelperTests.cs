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
            var info = vi.Read(@"D:\videos\unprocessed\Injustice.mkv");
            Assert.IsNotNull(info);

        }

        [TestMethod]
        public void VideoInfoTest_SubtitleRemover()
        {
            const string file = @"D:\videos\unprocessed\Bourne.mkv";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(@"D:\videos\unprocessed\Masters of the Universe (1987) Bluray-1080p.mkv.skip");

            SubtitleRemover remover = new SubtitleRemover();
            remover.SubtitlesToRemove = new List<string>
            {
                "subrip", "srt"
            };
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";

            new VideoFile().Execute(args);

            int output = remover.Execute(args);

            Assert.AreEqual(1, output);

        }

        [TestMethod]
        public void VideoInfoTest_DetectBlackBars()
        {
            //const string file = @"D:\videos\unprocessed\The Witcher - S02E05 - Turn Your Back.mkv";
            //const string file = @"D:\videos\unprocessed\Hawkeye (2021) - S01E05 - Ronin.mkv";
            const string file = @"\\ORACLE\tv\Dexter - New Blood\Season 1\Dexter - New Blood - S01E07 - Skin of Her Teeth.mkv";
            //var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger(), false, string.Empty);
            //vi.Read(@"D:\videos\unprocessed\Bourne.mkv");

            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";

            int result = new DetectBlackBars().Execute(args);

            Assert.IsTrue(result > 0);
        }


        [TestMethod]
        public void VideoInfoTest_NvidiaCard()
        {
            const string file = @"D:\videos\unprocessed\Bourne.mkv";
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
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
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            //args.Process = new FileFlows.Plugin.ProcessHelper(args.Logger);

            var node = new VideoEncode();
            node.SetArgs(args);
            bool result = node.CanProcessEncoder(ffmpeg, "hevc_nvenc -preset hq");

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void VideoInfoTest_CanEncodeIntel()
        {
            const string file = @"D:\videos\unprocessed\Bourne.mkv";
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            //args.Process = new FileFlows.Plugin.ProcessHelper(args.Logger);

            var node = new VideoEncode();
            node.SetArgs(args);
            bool result = node.CanProcessEncoder(ffmpeg, "h264_qsv");

            Assert.IsTrue(result);
        }


        [TestMethod]
        public void VideoInfoTest_AudioTrackReorder()
        {
            var node = new AudioTrackReorder();
            var original = new List<AudioStream>
            {
                new AudioStream{ Codec = "aac", Language = "fre"},
                new AudioStream{ Codec = "dts", Language = "fre"},
                new AudioStream{ Codec = "aac", Language = "eng"},
                new AudioStream{ Codec = "aac", Language = "mao"},
                new AudioStream{ Codec = "dts", Language = "mao"},
                new AudioStream{ Codec = "ac3", Language = "mao"},
                new AudioStream{ Codec = "ac3", Language = "eng"},
                new AudioStream{ Codec = "ac3", Language = "fre"},
            };
            node.Languages = new List<string> { "eng" };
            node.OrderedTracks = new List<string> { "ac3", "aac" };
            var reordered = node.Reorder(original);

            Assert.AreEqual("ac3", reordered[0].Codec);
            Assert.AreEqual("eng", reordered[0].Language);

            Assert.AreEqual("aac", reordered[1].Codec);
            Assert.AreEqual("eng", reordered[1].Language);
            
            Assert.AreEqual("ac3", reordered[2].Codec);
            Assert.AreEqual("mao", reordered[2].Language);

            Assert.AreEqual("ac3", reordered[3].Codec);
            Assert.AreEqual("fre", reordered[3].Language);

            Assert.AreEqual("aac", reordered[4].Codec);
            Assert.AreEqual("fre", reordered[4].Language);

            Assert.AreEqual("aac", reordered[5].Codec);
            Assert.AreEqual("mao", reordered[5].Language);

            Assert.AreEqual("dts", reordered[6].Codec);
            Assert.AreEqual("fre", reordered[6].Language);

            Assert.AreEqual("dts", reordered[7].Codec);
            Assert.AreEqual("mao", reordered[7].Language);
        }



        [TestMethod]
        public void VideoInfoTest_AudioTrackReorder_Channels()
        {
            var node = new AudioTrackReorder();
            var original = new List<AudioStream>
            {
                new AudioStream{ Codec = "aac", Language = "fre", Channels = 5.1f},
                new AudioStream{ Codec = "dts", Language = "fre", Channels = 2},
                new AudioStream{ Codec = "aac", Language = "eng", Channels = 2.1f},
                new AudioStream{ Codec = "aac", Language = "mao", Channels = 8},
                new AudioStream{ Codec = "dts", Language = "mao" , Channels=7.1f} ,
                new AudioStream{ Codec = "ac3", Language = "mao", Channels = 6.2f},
                new AudioStream{ Codec = "ac3", Language = "eng", Channels = 5.1f},
                new AudioStream{ Codec = "ac3", Language = "fre", Channels = 8},
            };


            node.Channels = new List<string> { "8", "5.1", "7.1",  "6.2" };
            var reordered = node.Reorder(original);

            int count = 0;
            foreach (var chan in new[] { ("aac", "mao", 8f), ("ac3", "fre", 8), ("aac", "fre", 5.1f), ("ac3", "eng", 5.1f), ("dts", "mao", 7.1f),
                ("ac3", "mao", 6.2f), ("dts", "fre", 2), ("aac", "eng", 2.1f) })
            {
                Assert.AreEqual(chan.Item1, reordered[count].Codec);
                Assert.AreEqual(chan.Item2, reordered[count].Language);
                Assert.AreEqual(chan.Item3, reordered[count].Channels);
                ++count;
            }
        }

        [TestMethod]
        public void VideoInfoTest_AudioTrackReorder_NothingConfigured()
        {
            var node = new AudioTrackReorder();
            var original = new List<AudioStream>
            {
                new AudioStream{ Codec = "aac", Language = "fre"},
                new AudioStream{ Codec = "dts", Language = "fre"},
                new AudioStream{ Codec = "aac", Language = "eng"},
                new AudioStream{ Codec = "aac", Language = "mao"},
                new AudioStream{ Codec = "dts", Language = "mao"},
                new AudioStream{ Codec = "ac3", Language = "mao"},
                new AudioStream{ Codec = "ac3", Language = "eng"},
                new AudioStream{ Codec = "ac3", Language = "fre"},
            };
            node.Languages = null;
            node.OrderedTracks = new List<string>();
            var reordered = node.Reorder(original);

            for(int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].Codec, reordered[i].Codec);
                Assert.AreEqual(original[i].Language, reordered[i].Language);
            }
        }

        [TestMethod]
        public void VideoInfoTest_AudioTrackReorder_NoLanguage()
        {
            var node = new AudioTrackReorder();
            var original = new List<AudioStream>
            {
                new AudioStream{ Codec = "aac", Language = "fre"},
                new AudioStream{ Codec = "dts", Language = "fre"},
                new AudioStream{ Codec = "aac", Language = "eng"},
                new AudioStream{ Codec = "aac", Language = "mao"},
                new AudioStream{ Codec = "dts", Language = "mao"},
                new AudioStream{ Codec = "ac3", Language = "mao"},
                new AudioStream{ Codec = "ac3", Language = "eng"},
                new AudioStream{ Codec = "ac3", Language = "fre"},
            };
            node.OrderedTracks = new List<string> { "ac3", "aac" };
            var reordered = node.Reorder(original);

            Assert.AreEqual("ac3", reordered[0].Codec);
            Assert.AreEqual("mao", reordered[0].Language);

            Assert.AreEqual("ac3", reordered[1].Codec);
            Assert.AreEqual("eng", reordered[1].Language);

            Assert.AreEqual("ac3", reordered[2].Codec);
            Assert.AreEqual("fre", reordered[2].Language);

            Assert.AreEqual("aac", reordered[3].Codec);
            Assert.AreEqual("fre", reordered[3].Language);

            Assert.AreEqual("aac", reordered[4].Codec);
            Assert.AreEqual("eng", reordered[4].Language);

            Assert.AreEqual("aac", reordered[5].Codec);
            Assert.AreEqual("mao", reordered[5].Language);

            Assert.AreEqual("dts", reordered[6].Codec);
            Assert.AreEqual("fre", reordered[6].Language);

            Assert.AreEqual("dts", reordered[7].Codec);
            Assert.AreEqual("mao", reordered[7].Language);
        }



        [TestMethod]
        public void VideoInfoTest_AudioTrackReorder_NoCodec()
        {
            var node = new AudioTrackReorder();
            var original = new List<AudioStream>
            {
                new AudioStream{ Codec = "aac", Language = "fre"},
                new AudioStream{ Codec = "dts", Language = "fre"},
                new AudioStream{ Codec = "aac", Language = "eng"},
                new AudioStream{ Codec = "aac", Language = "mao"},
                new AudioStream{ Codec = "dts", Language = "mao"},
                new AudioStream{ Codec = "ac3", Language = "mao"},
                new AudioStream{ Codec = "ac3", Language = "eng"},
                new AudioStream{ Codec = "ac3", Language = "fre"},
            };
            node.Languages = new List<string> { "eng" };
            var reordered = node.Reorder(original);

            Assert.AreEqual("aac", reordered[0].Codec);
            Assert.AreEqual("eng", reordered[0].Language);

            Assert.AreEqual("ac3", reordered[1].Codec);
            Assert.AreEqual("eng", reordered[1].Language);

            Assert.AreEqual("aac", reordered[2].Codec);
            Assert.AreEqual("fre", reordered[2].Language);

            Assert.AreEqual("dts", reordered[3].Codec);
            Assert.AreEqual("fre", reordered[3].Language);

            Assert.AreEqual("aac", reordered[4].Codec);
            Assert.AreEqual("mao", reordered[4].Language);

            Assert.AreEqual("dts", reordered[5].Codec);
            Assert.AreEqual("mao", reordered[5].Language);

            Assert.AreEqual("ac3", reordered[6].Codec);
            Assert.AreEqual("mao", reordered[6].Language);

            Assert.AreEqual("ac3", reordered[7].Codec);
            Assert.AreEqual("fre", reordered[7].Language);
        }


        [TestMethod]
        public void ComskipTest()
        {
            const string file = @"D:\videos\unprocessed\The IT Crowd - 2x04 - The Dinner Party - No English.mkv";
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);

            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";


            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);
            args.SetParameter("VideoInfo", vii);
            //args.Process = new FileFlows.Plugin.ProcessHelper(args.Logger);

            var node = new ComskipRemoveAds();
            int output = node.Execute(args);
            Assert.AreEqual(1, output);
        }

        [TestMethod]
        public void Comskip_Chapters()
        {
            const string file = @"D:\videos\recordings\Rescue My Renovation (2001).ts";
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty);

            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";


            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);
            args.SetParameter("VideoInfo", vii);
            //args.Process = new FileFlows.Plugin.ProcessHelper(args.Logger);

            var node = new ComskipChapters();
            int output = node.Execute(args);
            Assert.AreEqual(1, output);
        }



        [TestMethod]
        public void VideoInfoTest_Subtitle_Extractor()
        {
            const string file = @"D:\videos\Injustice.mkv";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);

            SubtitleExtractor node = new ();
            //node.OutputFile = file + ".sup";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";

            new VideoFile().Execute(args);

            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }

        [TestMethod]
        public void VideoInfoTest_AC1()
        {
            string ffmpegOutput = @"Input #0, mov,mp4,m4a,3gp,3g2,mj2, from '/media/Videos/#-Test Tdarr/Input3/input file.mp4':
  Metadata:
    major_brand     : mp42
    minor_version   : 512
    compatible_brands: mp42iso6
    creation_time   : 2022-01-07T06:30:47.000000Z
    title           : Episode title
    comment         : Episode description
  Duration: 00:42:23.75, start: 0.000000, bitrate: 3174 kb/s
    Stream #0:0(und): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 1920x1080 [SAR 1:1 DAR 16:9], 2528 kb/s, 23.98 fps, 23.98 tbr, 24k tbn, 47.95 tbc (default)
    Metadata:
      creation_time   : 2022-01-07T06:30:47.000000Z
      handler_name    : VideoHandler
    Stream #0:1(deu): Audio: eac3 (ec-3 / 0x332D6365), 48000 Hz, 5.1(side), fltp, 640 kb/s (default)
    Metadata:
      creation_time   : 2022-01-07T06:30:47.000000Z
      handler_name    : SoundHandler
    Side data:
      audio service type: main";
            var vi = VideoInfoHelper.ParseOutput(null, ffmpegOutput);
            Assert.AreEqual(1920, vi.VideoStreams[0].Width);
            Assert.AreEqual(1080, vi.VideoStreams[0].Height);
        }
    }
}

#endif