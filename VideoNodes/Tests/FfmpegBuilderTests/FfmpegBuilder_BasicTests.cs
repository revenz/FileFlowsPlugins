#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_BasicTests : TestBase
{
    [TestMethod]
    public void FfmpegBuilder_Basic_h265()
    {
        const string file = @"D:\videos\unprocessed\basic.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Codec = "h265 10BIT";
        ffEncode.Quality = 28;
        //ffEncode.HardwareEncoding = true;
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_Basic_Av1()
    {
        const string file = @"D:\videos\unprocessed\basic.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Codec = "av1 10BIT";
        ffEncode.Quality = 28;
        //ffEncode.HardwareEncoding = false;
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac()
    {
        const string file = @"D:\videos\unprocessed\multi_audio.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new ();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new ();
        ffEncode.VideoCodec = "h264";
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);


        FfmpegBuilderAudioAddTrack  ffAddAudio = new ();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.NewTitle = "new ac3";
        ffAddAudio.Channels = 2;
        ffAddAudio.Index = 0;
        ffAddAudio.PreExecute(args);
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.RemoveTitle = true;
        ffAddAudio2.Index = 1;
        ffAddAudio.Channels = 2;
        ffAddAudio2.PreExecute(args);
        ffAddAudio2.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_AudioT064kbps()
    {
        const string file = @"D:\videos\testfiles\basic.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderAudioTrackRemover ffRemover = new();
        ffRemover.RemoveAll = true;
        ffRemover.StreamType = "Audio";
        ffRemover.PreExecute(args);
        ffRemover.Execute(args);    


        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "aac";
        ffAddAudio.Bitrate = 640;
        ffAddAudio.Channels = 0;
        ffAddAudio.Index = 0;
        ffAddAudio.PreExecute(args);
        ffAddAudio.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudioTracks()
    {
        const string file = @"D:\videos\unprocessed\bigbuckbunny_480p_30s.mp4";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));
        var model = ffStart.GetModel();
        if (model.AudioStreams[0].Stream.Channels < 5.1f)
            Assert.Fail();

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.Execute(args);

        int index = 0;
        FfmpegBuilderAudioAddTrack ffAddAudioMono = new();
        ffAddAudioMono.Codec = "mp3";
        ffAddAudioMono.Index = index;
        ffAddAudioMono.Channels = 1;
        ffAddAudioMono.Execute(args);
        model.AudioStreams[index].Title = "MP3 Mono";
        ++index;

        FfmpegBuilderAudioAddTrack ffAddAudioStereoAac = new();
        ffAddAudioStereoAac.Codec = "aac";
        ffAddAudioStereoAac.Index = index;
        ffAddAudioStereoAac.Channels = 2;
        ffAddAudioStereoAac.Execute(args);
        model.AudioStreams[index].Title = "AAC Stereo";
        ++index;

        FfmpegBuilderAudioAddTrack ffAddAudioStereoMp3French = new();
        ffAddAudioStereoMp3French.Codec = "mp3";
        ffAddAudioStereoMp3French.Index = index;
        ffAddAudioStereoMp3French.Channels = 2;
        ffAddAudioStereoMp3French.Execute(args);
        model.AudioStreams[index].Language = "fre";
        model.AudioStreams[index].Title = "MP3 Stereo";
        ++index;

        FfmpegBuilderAudioAddTrack ffAddAudioStereoMp3 = new();
        ffAddAudioStereoMp3.Codec = "mp3";
        ffAddAudioStereoMp3.Index = index;
        ffAddAudioStereoMp3.Channels = 2;
        ffAddAudioStereoMp3.Execute(args);
        model.AudioStreams[index].Title = "MP3 Stereo";
        ++index;

        FfmpegBuilderAudioAddTrack ffAddAudioAc3German = new();
        ffAddAudioAc3German.Codec = "ac3";
        ffAddAudioAc3German.Index = index;
        ffAddAudioAc3German.Execute(args);
        model.AudioStreams[index].Title = "AC3 5.1";
        model.AudioStreams[index].Language = "deu";
        ++index;

        FfmpegBuilderAudioAddTrack ffAddAudioAc3 = new();
        ffAddAudioAc3.Codec = "ac3";
        ffAddAudioAc3.Index = index;
        ffAddAudioAc3.Execute(args);
        model.AudioStreams[index].Title = "AC3 5.1";
        ++index;

        FfmpegBuilderAudioAddTrack ffAddAudioAac = new();
        ffAddAudioAac.Codec = "aac";
        ffAddAudioAac.Index = index;
        ffAddAudioAac.Execute(args);
        model.AudioStreams[index].Title = "AAC 5.1";
        ++index;

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_Normalize()
    {
        const string file = @"D:\videos\unprocessed\dummy.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderAudioTrackRemover ffAudioRemover = new();
        ffAudioRemover.RemoveAll = true;
        ffAudioRemover.PreExecute(args);
        ffAudioRemover.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.PreExecute(args);
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Index = 2;
        ffAddAudio2.PreExecute(args);
        ffAddAudio2.Execute(args);

        FfmpegBuilderAudioNormalization ffAudioNormalize = new();
        ffAudioNormalize.TwoPass = false;
        ffAudioNormalize.AllAudio = true;
        ffAudioNormalize.PreExecute(args);
        ffAudioNormalize.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_AdjustVolume()
    {
        const string file = @"D:\videos\unprocessed\dummy.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.Execute(args);

        FfmpegBuilderAudioTrackRemover ffAudioRemover = new();
        ffAudioRemover.RemoveAll = true;
        ffAudioRemover.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Index = 2;
        ffAddAudio2.Execute(args);

        FfmpegBuilderAudioAdjustVolume ffAudioAdjust= new();
        ffAudioAdjust.VolumePercent = 1000;
        ffAudioAdjust.Pattern = ">1";
        ffAudioAdjust.Execute(args);

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
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.Execute(args);

        FfmpegBuilderRemuxToMP4 ffMp4 = new();
        ffMp4.Execute(args);


        FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
        ffSubRemover.RemoveAll = true;
        ffSubRemover.Execute(args);


        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
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
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
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


        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
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
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
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


        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
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
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
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


        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
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
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
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


        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Index = 2;
        ffAddAudio2.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_AutoChapters()
    {
        const string file = @"D:\videos\unprocessed\sitcom.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Index = 2;
        ffAddAudio2.Execute(args);

        FfmpegBuilderAutoChapters ffAutoChapters = new();
        ffAutoChapters.Percent = 45;
        ffAutoChapters.MinimumLength = 60;
        Assert.AreEqual(1, ffAutoChapters.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_ComskipChapters()
    {
        const string file = @"D:\videos\recordings\Rescue My Renovation (2001).ts";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 1;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Index = 2;
        ffAddAudio2.Execute(args);

        FfmpegBuilderAudioSetLanguage ffSetLanguage = new();
        ffSetLanguage.Language = "deu";
        ffSetLanguage.Execute(args);

        FfmpegBuilderComskipChapters  ffComskipChapters = new();
        Assert.AreEqual(1, ffComskipChapters.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_AudioTrackReorder()
    {
        const string file = @"D:\videos\unprocessed\multi_audio.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderAudioTrackReorder ffAudioReorder= new();
        ffAudioReorder.Channels = new List<string> { "1.0", "5.1", "2.0" };
        ffAudioReorder.Languages = new List<string> { "fre", "deu" };
        ffAudioReorder.PreExecute(args);
        ffAudioReorder.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);


        var vi2 = new VideoInfoHelper(ffmpeg, logger);
        var vii2 = vi.Read(args.WorkingFile);
    }


    [TestMethod]
    public void FfmpegBuilder_SubtitleTrackReorder()
    {
        const string file =  @"D:\videos\testfiles\movtext.mp4";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));
        var model = (FfmpegModel)args.Variables["FFMPEG_BUILDER_MODEL"];

        Assert.AreNotEqual("eng", model.SubtitleStreams[0].Language);

        FfmpegBuilderAudioTrackReorder ffAudioReorder = new();
        ffAudioReorder.StreamType = "Subtitle";
        ffAudioReorder.Languages = new List<string> { "eng", "deu" };
        ffAudioReorder.PreExecute(args);
        ffAudioReorder.Execute(args);

        Assert.AreEqual("eng", model.SubtitleStreams[0].Language);

    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3AacMp4NoSubs_BlackBars_Normalize_AutoChapters_Upscale4k()
    {
        const string file = @"D:\videos\unprocessed\blackbars.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));
        var model = ffStart.GetModel();

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h265";
        ffEncode.Execute(args);

        FfmpegBuilderScaler ffScaler = new();
        ffScaler.Resolution = "3840:-2";
        ffScaler.Execute(args);

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
        ffAddAudio.Index = 0;
        ffAddAudio.Execute(args);
        model.AudioStreams[0].Language = "mao";
        model.AudioStreams[0].Title = "AC3";

        FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Index = 1;
        ffAddAudio2.Execute(args);
        model.AudioStreams[1].Language = "fre";
        model.AudioStreams[1].Title = "AAC";

        FfmpegBuilderAudioNormalization ffAudioNormalize = new();
        ffAudioNormalize.TwoPass = false;
        ffAudioNormalize.AllAudio = true;
        ffAudioNormalize.Execute(args);


        FfmpegBuilderAutoChapters ffaAutoChapters = new();
        ffaAutoChapters.MinimumLength = 30;
        ffaAutoChapters.Percent = 45;
        ffaAutoChapters.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_SetLanguage()
    {
        const string file = @"D:\videos\unprocessed\sitcom.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderAudioSetLanguage ffSetLanguage = new();
        ffSetLanguage.Language = "deu";
        ffSetLanguage.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_HdrToSdr()
    {
        const string file = @"D:\videos\unprocessed\hdr.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderHdrToSdr ffHdrToSdr= new();
        Assert.AreEqual(1, ffHdrToSdr.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_AudioMinusOne()
    {
        const string file = @"D:\videos\unprocessed\minus1.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        //FfmpegBuilderVideoCodec ffEncode = new();
        //ffEncode.VideoCodec = "h264";
        //ffEncode.Execute(args);

        FfmpegBuilderAudioTrackRemover ffAudioRemover = new();
        ffAudioRemover.RemoveAll = true;
        ffAudioRemover.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 0;
        ffAddAudio.Execute(args);

        //FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
        //ffAddAudio2.Codec = "aac";
        //ffAddAudio2.Index = 2;
        //ffAddAudio2.Execute(args);

        //FfmpegBuilderAudioNormalization ffAudioNormalize = new();
        //ffAudioNormalize.TwoPass = false;
        //ffAudioNormalize.AllAudio = true;
        //ffAudioNormalize.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_HardwareDecoding()
    {
        const string file = @"D:\videos\unprocessed\basic.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.Execute(args);


        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 0;
        ffAddAudio.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_VideoBitrate()
    {
        const string file = @"D:\videos\unprocessed\basic.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoBitrate ffBitrate = new();
        ffBitrate.Bitrate = 1_000;
        ffBitrate.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_VideoCodecAndBitrate()
    {
        const string file = @"D:\videos\unprocessed\basic.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.Force = true;
        ffEncode.Execute(args);

        FfmpegBuilderVideoBitrate ffBitrate = new();
        ffBitrate.Bitrate = 50;
        ffBitrate.Percent = true;
        ffBitrate.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_FF43()
    {
        const string file = @"D:\videos\testfiles\ff-43.ts";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderAudioTrackRemover ffRemoveAudio = new();
        ffRemoveAudio.RemoveAll = true;
        ffRemoveAudio.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 0;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Index = 1;
        ffAddAudio2.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_AV1()
    {
        const string file = @"D:\videos\testfiles\av1.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg5\ffmpeg.exe";
        VideoInfoHelper.ProbeSize = 1000;
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderAudioTrackRemover ffAudioRemove = new();
        ffAudioRemove.RemoveAll = true;
        ffAudioRemove.Execute(args);

        //FfmpegBuilderAudioAddTrack ffAddAudio = new();
        //ffAddAudio.Codec = "ac3";
        //ffAddAudio.Language = "eng";
        //ffAddAudio.Index = 0;
        //ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Language = "deu";
        ffAddAudio2.Index = 1;
        ffAddAudio2.Execute(args);

        FfmpegBuilderSubtitleFormatRemover ffSubtitle= new();
        ffSubtitle.RemoveAll = true;
        ffSubtitle.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }




    [TestMethod]
    public void FfmpegBuilder_RemoveSubtitleFormat_MovText()
    {
        const string file = @"D:\videos\testfiles\movtext.mp4";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderSubtitleFormatRemover ffSubRemover= new();
        ffSubRemover.SubtitlesToRemove = new List<string> { "mov_text" };
        ffSubRemover.PreExecute(args);
        Assert.AreEqual(1, ffSubRemover.Execute(args));
    }



    [TestMethod]
    public void FfmpegBuilder_VideoTag()
    {
        const string file = @"D:\videos\testfiles\120-mbps-4k-uhd-hevc-10bit.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoTag ffTag= new();
        ffTag.Tag = "hvc1";
        ffTag.PreExecute(args); 
        ffTag.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }



    [TestMethod]
    public void FfmpegBuilder_CustomParameters()
    {
        const string file = @"D:\videos\testfiles\basic.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderCustomParameters ffCustom = new();
        ffCustom.Parameters = "this is a \"testing bobby drake\" blah";
        ffCustom.ForceEncode = true;
        ffCustom.PreExecute(args);
        ffCustom.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.IsTrue(log.Contains("this is a \"testing bobby drake\" blah"));
    }


    [TestMethod]
    public void FfmpegBuilder_ImageStream()
    {
        const string file = @"D:\videos\testfiles\img_stream.mp4";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderRemuxToMkv ffMkv = new();
        ffMkv.PreExecute(args);
        Assert.AreEqual(1, ffMkv.Execute(args));

        FfmpegBuilderAudioTrackRemover ffRemover = new();
        ffRemover.StreamType = "Video";
        ffRemover.RemoveIndex = 1;
        ffRemover.PreExecute(args);
        Assert.AreEqual(1, ffRemover.Execute(args));


        FfmpegBuilderAudioTrackRemover ffRemoverSubtitles = new();
        ffRemoverSubtitles.StreamType = "Subtitle";
        ffRemoverSubtitles.RemoveAll = true;
        ffRemoverSubtitles.PreExecute(args);
        Assert.AreEqual(1, ffRemoverSubtitles.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }



    [TestMethod]
    public void FfmpegBuilder_SubtitleTrackMerge()
    {
        const string file = @"D:\videos\testfiles\dummy.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));


        FfmpegBuilderSubtitleTrackMerge ffSubMerge = new();
        ffSubMerge.Subtitles = new List<string> { "srt" };
        ffSubMerge.MatchFilename = true;
        ffSubMerge.PreExecute(args);
        Assert.AreEqual(1, ffSubMerge.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_SubtitleTrackMerge_FileMatchesTests()
    {
        FfmpegBuilderSubtitleTrackMerge ffSubMerge = new();
        foreach (var item in new[] { 
("test.srt", "", true),
("test.en.srt", "English", true),
("test(en).srt", "English", true),
("test (en).srt", "English", true),
("test.en.hi.srt", "English (HI)", true),
("test.en.sdh.srt", "English (SDH)", true),
("test.en.cc.srt", "English (CC)", true),
("test.de.srt", "German", true),
("test(de).srt", "German", true),
("test (de).srt", "German", true),

("nomatch.srt", "", false),
("nomatch.en.srt", "English", false),
("nomatch(en).srt", "English", false),
("nomatch (en).srt", "English", false)
        })
        {
            string lang;
            bool isMatch = ffSubMerge.FilenameMatches("Test.mkv", item.Item1, out lang);
            Assert.AreEqual(item.Item3, isMatch);
            Assert.AreEqual(item.Item2, lang, "Language not matching in: " + item.Item1);
        }


    }

    [TestMethod]
    public void FfmpegBuilder_BlackBars_Short()
    {
        const string file = @"D:\videos\testfiles\tag.mp4";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderCropBlackBars ffCropBlackBars = new();
        ffCropBlackBars.CroppingThreshold = 10;
        ffCropBlackBars.PreExecute(args);
        ffCropBlackBars.Execute(args);

        string log = logger.ToString();
        Assert.IsTrue(log.Contains($"-ss 2 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 4 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 6 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 8 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
    }

    [TestMethod]
    public void FfmpegBuilder_BlackBars_30sec()
    {
        const string file = @"D:\videos\testfiles\50-mbps-hd-h264.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderCropBlackBars ffCropBlackBars = new();
        ffCropBlackBars.CroppingThreshold = 10;
        ffCropBlackBars.PreExecute(args);
        ffCropBlackBars.Execute(args);

        string log = logger.ToString();
        Assert.IsTrue(log.Contains($"-ss 6 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 12 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 18 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 24 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
    }


    [TestMethod]
    public void FfmpegBuilder_BlackBars_LongVideo()
    {
        const string file = @"D:\videos\testfiles\pgs.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderCropBlackBars ffCropBlackBars = new();
        ffCropBlackBars.CroppingThreshold = 10;
        ffCropBlackBars.PreExecute(args);
        ffCropBlackBars.Execute(args);

        string log = logger.ToString();
        Assert.IsTrue(log.Contains($"-ss 60 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 120 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 240 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
        Assert.IsTrue(log.Contains($"-ss 360 -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -"));
    }



    [TestMethod]
    public void FfmpegBuilder_Scale()
    {
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(TestFile_120_mbps_4k_uhd_hevc_10bit);
        var args = new NodeParameters(TestFile_50_mbps_hd_h264, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffCodec = new();
        ffCodec.VideoCodec = "h265";
        ffCodec.VideoCodecParameters = "h265";
        ffCodec.PreExecute(args);
        ffCodec.Execute(args);

        FfmpegBuilderScaler ffScaler = new();
        ffScaler.Resolution = "640:-2";
        ffScaler.Force = true;
        ffScaler.PreExecute(args);
        ffScaler.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);
        string log = logger.ToString();
        Assert.AreEqual(1, result);

    }



    [TestMethod]
    public void FfmpegBuilder_SubtitleFormatRemover_Ppgs()
    {
        const string file = @"D:\videos\testfiles\pgs.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));


        FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
        ffSubRemover.SubtitlesToRemove = new List<string> { "hdmv_pgs_subtitle" };
        ffSubRemover.PreExecute(args);
        int result1 = ffSubRemover.Execute(args);


        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result1);
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_SubtitleFormatRemover_Unknown()
    {
        const string file = @"D:\videos\testfiles\pgs.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));


        FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
        ffSubRemover.SubtitlesToRemove = new List<string> { "UNKNOWN" };
        ffSubRemover.PreExecute(args);
        int result1 = ffSubRemover.Execute(args);


        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(2, result1);
        Assert.AreEqual(2, result);
    }
    
    
    [TestMethod]
    public void FfmpegBuilder_SubtitleClearDefault()
    {
        var logger = new TestLogger();
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(TestFile_DefaultIsForcedSub);
        var args = new NodeParameters(TestFile_DefaultIsForcedSub, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));
        
        
        FfmpegBuilderSubtitleClearDefault ffClearDefault = new();
        ffClearDefault.LeaveForced = true;
        ffClearDefault.PreExecute(args);
        int result1 = ffClearDefault.Execute(args);
        Assert.AreEqual(1, result1);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result1);
        Assert.AreEqual(1, result);
    }

}

#endif