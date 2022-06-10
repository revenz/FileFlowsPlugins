#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

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

        FfmpegBuilderVideoCodec ffEncode = new ();
        ffEncode.VideoCodec = "h264";
        ffEncode.Execute(args);


        FfmpegBuilderAudioAddTrack  ffAddAudio = new ();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 0;
        ffAddAudio.Execute(args);

        FfmpegBuilderAudioAddTrack  ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Index = 1;
        ffAddAudio2.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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

        FfmpegBuilderAudioNormalization ffAudioNormalize = new();
        ffAudioNormalize.TwoPass = false;
        ffAudioNormalize.AllAudio = true;
        ffAudioNormalize.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderAudioTrackReorder ffAudioReorder= new();
        ffAudioReorder.Channels = new List<string> { "1.0", "5.1", "2.0" };
        ffAudioReorder.Languages = new List<string> { "fre", "deu" };
        ffAudioReorder.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_AddAc3AacMp4NoSubs_BlackBars_Normalize_AutoChapters_Upscale4k()
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
        var args = new NodeParameters(file, logger, false, string.Empty);
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
}

#endif