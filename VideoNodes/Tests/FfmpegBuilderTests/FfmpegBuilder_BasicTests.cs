#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests
{
    [TestClass]
    public class FfmpegBuilder_BasicTests
    {
        [TestMethod]
        public void FfmpegBuilder_AddAc3Aac()
        {
            const string file = @"D:\videos\unprocessed\basic.mkv";
            var logger = new TestLogger();
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var vi = new VideoInfoHelper(ffmpeg, logger);
            var vii = vi.Read(file);
            var args = new NodeParameters(file, logger, false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";
            args.Parameters.Add("VideoInfo", vii);


            FfmpegBuilderStart ffStart = new ();
            Assert.AreEqual(1, ffStart.Execute(args));

            FfmpegBuilderVideoEncode ffEncode = new ();
            ffEncode.VideoCodec = "h264";
            ffEncode.Execute(args);


            FfmpegBuilderAudioAddTrack ffAddAudio = new ();
            ffAddAudio.Codec = "ac3";
            ffAddAudio.Index = 1;
            ffAddAudio.Execute(args);

            FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
            ffAddAudio2.Codec = "aac";
            ffAddAudio2.Index = 2;
            ffAddAudio2.Execute(args);

            FfmpegBuilderExecutor ffExecutor = new();
            int result = ffExecutor.Execute(args);

            string log = logger.ToString();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void FfmpegBuilder_AddAc3AacMp4NoSubs()
        {
            const string file = @"D:\videos\unprocessed\basic.mkv";
            var logger = new TestLogger();
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var vi = new VideoInfoHelper(ffmpeg, logger);
            var vii = vi.Read(file);
            var args = new NodeParameters(file, logger, false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";
            args.Parameters.Add("VideoInfo", vii);


            FfmpegBuilderStart ffStart = new();
            Assert.AreEqual(1, ffStart.Execute(args));

            FfmpegBuilderVideoEncode ffEncode = new();
            ffEncode.VideoCodec = "h264";
            ffEncode.Execute(args);

            FfmpegBuilderRemuxToMP4 ffMp4 = new();
            ffMp4.Execute(args);


            FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
            ffSubRemover.RemoveAll = true;
            ffSubRemover.Execute(args);


            FfmpegBuilderAudioAddTrack ffAddAudio = new();
            ffAddAudio.Codec = "ac3";
            ffAddAudio.Index = 1;
            ffAddAudio.Execute(args);

            FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
            ffAddAudio2.Codec = "aac";
            ffAddAudio2.Index = 2;
            ffAddAudio2.Execute(args);

            FfmpegBuilderExecutor ffExecutor = new();
            int result = ffExecutor.Execute(args);

            string log = logger.ToString();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void FfmpegBuilder_AddAc3AacMp4NoSubs_BlackBars()
        {
            const string file = @"D:\videos\unprocessed\blackbars.mkv";
            var logger = new TestLogger();
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var vi = new VideoInfoHelper(ffmpeg, logger);
            var vii = vi.Read(file);
            var args = new NodeParameters(file, logger, false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";
            args.Parameters.Add("VideoInfo", vii);


            FfmpegBuilderStart ffStart = new();
            Assert.AreEqual(1, ffStart.Execute(args));

            FfmpegBuilderVideoEncode ffEncode = new();
            ffEncode.VideoCodec = "h265";
            ffEncode.Execute(args);

            FfmpegBuilderRemuxToMP4 ffMp4 = new();
            ffMp4.Execute(args);

            FfmpegBuilderCropBlackBars ffCropBlackBars = new();
            ffCropBlackBars.CroppingThreshold = 10;
            ffCropBlackBars.Execute(args);

            FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
            ffSubRemover.RemoveAll = true;
            ffSubRemover.Execute(args);


            FfmpegBuilderAudioAddTrack ffAddAudio = new();
            ffAddAudio.Codec = "ac3";
            ffAddAudio.Index = 1;
            ffAddAudio.Execute(args);

            FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
            ffAddAudio2.Codec = "aac";
            ffAddAudio2.Index = 2;
            ffAddAudio2.Execute(args);

            FfmpegBuilderExecutor ffExecutor = new();
            int result = ffExecutor.Execute(args);

            string log = logger.ToString();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void FfmpegBuilder_AddAc3AacMp4NoSubs_BlackBars_Scaled480p()
        {
            const string file = @"D:\videos\unprocessed\blackbars.mkv";
            var logger = new TestLogger();
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var vi = new VideoInfoHelper(ffmpeg, logger);
            var vii = vi.Read(file);
            var args = new NodeParameters(file, logger, false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";
            args.Parameters.Add("VideoInfo", vii);


            FfmpegBuilderStart ffStart = new();
            Assert.AreEqual(1, ffStart.Execute(args));

            FfmpegBuilderVideoEncode ffEncode = new();
            ffEncode.VideoCodec = "h265";
            ffEncode.Execute(args);

            FfmpegBuilderRemuxToMP4 ffMp4 = new();
            ffMp4.Execute(args);

            FfmpegBuilderCropBlackBars ffCropBlackBars = new();
            ffCropBlackBars.CroppingThreshold = 10;
            ffCropBlackBars.Execute(args);

            FfmpegBuilderScaler ffScaler = new();
            ffScaler.Resolution = "640:-2";
            ffScaler.Execute(args);

            FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
            ffSubRemover.RemoveAll = true;
            ffSubRemover.Execute(args);


            FfmpegBuilderAudioAddTrack ffAddAudio = new();
            ffAddAudio.Codec = "ac3";
            ffAddAudio.Index = 1;
            ffAddAudio.Execute(args);

            FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
            ffAddAudio2.Codec = "aac";
            ffAddAudio2.Index = 2;
            ffAddAudio2.Execute(args);

            FfmpegBuilderExecutor ffExecutor = new();
            int result = ffExecutor.Execute(args);

            string log = logger.ToString();
            Assert.AreEqual(1, result);
        }


        [TestMethod]
        public void FfmpegBuilder_AddAc3AacMp4NoSubs_BlackBars_Scaled4k()
        {
            const string file = @"D:\videos\unprocessed\blackbars.mkv";
            var logger = new TestLogger();
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var vi = new VideoInfoHelper(ffmpeg, logger);
            var vii = vi.Read(file);
            var args = new NodeParameters(file, logger, false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";
            args.Parameters.Add("VideoInfo", vii);


            FfmpegBuilderStart ffStart = new();
            Assert.AreEqual(1, ffStart.Execute(args));

            FfmpegBuilderVideoEncode ffEncode = new();
            ffEncode.VideoCodec = "h265";
            ffEncode.Execute(args);

            FfmpegBuilderRemuxToMP4 ffMp4 = new();
            ffMp4.Execute(args);

            FfmpegBuilderCropBlackBars ffCropBlackBars = new();
            ffCropBlackBars.CroppingThreshold = 10;
            ffCropBlackBars.Execute(args);

            FfmpegBuilderScaler ffScaler = new();
            ffScaler.Resolution = "3840:-2";
            ffScaler.Execute(args);

            FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
            ffSubRemover.RemoveAll = true;
            ffSubRemover.Execute(args);


            FfmpegBuilderAudioAddTrack ffAddAudio = new();
            ffAddAudio.Codec = "ac3";
            ffAddAudio.Index = 1;
            ffAddAudio.Execute(args);

            FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
            ffAddAudio2.Codec = "aac";
            ffAddAudio2.Index = 2;
            ffAddAudio2.Execute(args);

            FfmpegBuilderExecutor ffExecutor = new();
            int result = ffExecutor.Execute(args);

            string log = logger.ToString();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void FfmpegBuilder_AddAc3AacMp4NoSubs_BlackBars_Scaled480p2()
        {
            const string file = @"D:\videos\unprocessed\basic.mkv";
            var logger = new TestLogger();
            const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
            var vi = new VideoInfoHelper(ffmpeg, logger);
            var vii = vi.Read(file);
            var args = new NodeParameters(file, logger, false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpeg;
            args.TempPath = @"D:\videos\temp";
            args.Parameters.Add("VideoInfo", vii);


            FfmpegBuilderStart ffStart = new();
            Assert.AreEqual(1, ffStart.Execute(args));

            FfmpegBuilderVideoEncode ffEncode = new();
            ffEncode.VideoCodec = "h265";
            ffEncode.Execute(args);

            FfmpegBuilderRemuxToMP4 ffMp4 = new();
            ffMp4.Execute(args);

            FfmpegBuilderCropBlackBars ffCropBlackBars = new();
            ffCropBlackBars.CroppingThreshold = 10;
            ffCropBlackBars.Execute(args);

            FfmpegBuilderScaler ffScaler = new();
            ffScaler.Resolution = "640:-2";
            ffScaler.Execute(args);

            FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
            ffSubRemover.RemoveAll = true;
            ffSubRemover.Execute(args);


            FfmpegBuilderAudioAddTrack ffAddAudio = new();
            ffAddAudio.Codec = "ac3";
            ffAddAudio.Index = 1;
            ffAddAudio.Execute(args);

            FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
            ffAddAudio2.Codec = "aac";
            ffAddAudio2.Index = 2;
            ffAddAudio2.Execute(args);

            FfmpegBuilderExecutor ffExecutor = new();
            int result = ffExecutor.Execute(args);

            string log = logger.ToString();
            Assert.AreEqual(1, result);
        }
    }
}

#endif