#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_BasicTests : VideoTestBase
{
    VideoInfo vii;
    NodeParameters args;
    FfmpegModel Model;
    
    protected override void TestStarting()
    {
        args = GetVideoNodeParameters();
        VideoFile vf = new VideoFile();
        vf.PreExecute(args);
        vf.Execute(args);
        vii = (VideoInfo)args.Parameters["VideoInfo"];
        
        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));
        Model = ffStart.GetModel();
    }

    [TestMethod]
    public void FfmpegBuilder_Basic_h265()
    {
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

        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_Basic_Av1()
    {
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

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac()
    {
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

        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_AudioT064kbps()
    {
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

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_Normalize()
    {
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
        ffAudioNormalize.TwoPass = true;
        ffAudioNormalize.AllAudio = true;
        ffAudioNormalize.PreExecute(args);
        ffAudioNormalize.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_AdjustVolume()
    {
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

        FfmpegBuilderAudioAdjustVolume ffAudioAdjust= new();
        ffAudioAdjust.VolumePercent = 1000;
        ffAudioAdjust.Pattern = ">1";
        ffAudioAdjust.PreExecute(args);
        ffAudioAdjust.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3AacMp4NoSubs()
    {
        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderRemuxToMP4 ffMp4 = new();
        ffMp4.PreExecute(args);
        ffMp4.Execute(args);


        FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
        ffSubRemover.RemoveAll = true;
        ffSubRemover.PreExecute(args);
        ffSubRemover.Execute(args);


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

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3AacMp4NoSubs_BlackBars()
    {
        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h265";
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderRemuxToMP4 ffMp4 = new();
        ffMp4.PreExecute(args);
        ffMp4.Execute(args);

        FfmpegBuilderCropBlackBars ffCropBlackBars = new();
        ffCropBlackBars.CroppingThreshold = 10;
        ffCropBlackBars.PreExecute(args);
        ffCropBlackBars.Execute(args);

        FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
        ffSubRemover.RemoveAll = true;
        ffSubRemover.PreExecute(args);
        ffSubRemover.Execute(args);


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

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAc3AacMp4NoSubs_BlackBars_Scaled480p()
    {
        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h265";
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderRemuxToMP4 ffMp4 = new();
        ffMp4.PreExecute(args);
        ffMp4.Execute(args);

        FfmpegBuilderCropBlackBars ffCropBlackBars = new();
        ffCropBlackBars.CroppingThreshold = 10;
        ffCropBlackBars.PreExecute(args);
        ffCropBlackBars.Execute(args);

        FfmpegBuilderScaler ffScaler = new();
        ffScaler.Resolution = "640:-2";
        ffScaler.PreExecute(args);
        ffScaler.Execute(args);

        FfmpegBuilderSubtitleFormatRemover ffSubRemover = new();
        ffSubRemover.RemoveAll = true;
        ffSubRemover.PreExecute(args);
        ffSubRemover.Execute(args);


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

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }
    
    // Video isnt long enough for this
    // [TestMethod]
    // public void FfmpegBuilder_AddAc3Aac_AutoChapters()
    // {
    //     FfmpegBuilderVideoCodec ffEncode = new();
    //     ffEncode.VideoCodec = "h264";
    //     ffEncode.PreExecute(args);
    //     ffEncode.Execute(args);
    //
    //     FfmpegBuilderAudioAddTrack ffAddAudio = new();
    //     ffAddAudio.Codec = "ac3";
    //     ffAddAudio.Index = 1;
    //     ffAddAudio.PreExecute(args);
    //     ffAddAudio.Execute(args);
    //
    //     FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
    //     ffAddAudio2.Codec = "aac";
    //     ffAddAudio2.Index = 2;
    //     ffAddAudio2.PreExecute(args);
    //     ffAddAudio2.Execute(args);
    //
    //     FfmpegBuilderAutoChapters ffAutoChapters = new();
    //     ffAutoChapters.Percent = 45;
    //     ffAutoChapters.MinimumLength = 60;
    //     ffAutoChapters.PreExecute(args);
    //     Assert.AreEqual(1, ffAutoChapters.Execute(args));
    //
    //     FfmpegBuilderExecutor ffExecutor = new();
    //     ffExecutor.PreExecute(args);
    //     int result = ffExecutor.Execute(args);
    //
    //     Assert.AreEqual(1, result);
    // }

    // [TestMethod]
    // public void FfmpegBuilder_AddAc3Aac_ComskipChapters()
    // {
    //     FfmpegBuilderVideoCodec ffEncode = new();
    //     ffEncode.VideoCodec = "h264";
    //     ffEncode.PreExecute(args);
    //     ffEncode.Execute(args);
    //
    //     FfmpegBuilderAudioAddTrack ffAddAudio = new();
    //     ffAddAudio.Codec = "ac3";
    //     ffAddAudio.Index = 1;
    //     ffAddAudio.PreExecute(args);
    //     ffAddAudio.Execute(args);
    //
    //     FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
    //     ffAddAudio2.Codec = "aac";
    //     ffAddAudio2.Index = 2;
    //     ffAddAudio2.PreExecute(args);
    //     ffAddAudio2.Execute(args);
    //
    //     FfmpegBuilderAudioSetLanguage ffSetLanguage = new();
    //     ffSetLanguage.Language = "deu";
    //     ffSetLanguage.PreExecute(args);
    //     ffSetLanguage.Execute(args);
    //
    //     FfmpegBuilderComskipChapters ffComskipChapters = new();
    //     ffComskipChapters.PreExecute(args);
    //     Assert.AreEqual(1, ffComskipChapters.Execute(args));
    //
    //     FfmpegBuilderExecutor ffExecutor = new();
    //     ffExecutor.PreExecute(args);
    //     int result = ffExecutor.Execute(args);
    //
    //     Assert.AreEqual(1, result);
    // }

    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_AudioTrackReorder()
    {
        FfmpegBuilderAudioTrackReorder ffAudioReorder= new();
        ffAudioReorder.Channels = new List<string> { "1.0", "5.1", "2.0" };
        ffAudioReorder.Languages = new List<string> { "fre", "deu" };
        ffAudioReorder.PreExecute(args);
        ffAudioReorder.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_SetLanguage()
    {
        FfmpegBuilderAudioSetLanguage ffSetLanguage = new();
        ffSetLanguage.Language = "deu";
        ffSetLanguage.PreExecute(args);
        ffSetLanguage.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AudioMinusOne()
    {
        FfmpegBuilderAudioTrackRemover ffAudioRemover = new();
        ffAudioRemover.RemoveAll = true;
        ffAudioRemover.PreExecute(args);
        ffAudioRemover.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Index = 0;
        ffAddAudio.PreExecute(args);
        ffAddAudio.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_VideoBitrate()
    {
        FfmpegBuilderVideoBitrate ffBitrate = new();
        ffBitrate.Bitrate = 1_000;
        ffBitrate.PreExecute(args);
        ffBitrate.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_VideoCodecAndBitrate()
    {
        FfmpegBuilderVideoCodec ffEncode = new();
        ffEncode.VideoCodec = "h264";
        ffEncode.Force = true;
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderVideoBitrate ffBitrate = new();
        ffBitrate.Bitrate = 50;
        ffBitrate.Percent = true;
        ffBitrate.PreExecute(args);
        ffBitrate.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }


    [TestMethod]
    public void FfmpegBuilder_AddAc3Aac_AV1()
    {
        FfmpegBuilderAudioTrackRemover ffAudioRemove = new();
        ffAudioRemove.RemoveAll = true;
        ffAudioRemove.PreExecute(args);
        ffAudioRemove.Execute(args);

        FfmpegBuilderAudioAddTrack ffAddAudio2 = new();
        ffAddAudio2.Codec = "aac";
        ffAddAudio2.Language = "deu";
        ffAddAudio2.Index = 1;
        ffAddAudio2.PreExecute(args);
        ffAddAudio2.Execute(args);

        FfmpegBuilderSubtitleFormatRemover ffSubtitle= new();
        ffSubtitle.RemoveAll = true;
        ffSubtitle.PreExecute(args);
        ffSubtitle.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_VideoTag()
    {
        FfmpegBuilderVideoTag ffTag= new();
        ffTag.Tag = "hvc1";
        ffTag.PreExecute(args); 
        ffTag.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_CustomParameters()
    {
        FfmpegBuilderCustomParameters ffCustom = new();
        ffCustom.Parameters = "this is a \"testing bobby drake\" blah";
        ffCustom.ForceEncode = true;
        ffCustom.PreExecute(args);
        ffCustom.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.IsTrue(Logger.ToString().Contains("this is a \"testing bobby drake\" blah"));
    }

    [TestMethod]
    public void FfmpegBuilder_SubtitleTrackMerge()
    {
        FfmpegBuilderSubtitleTrackMerge ffSubMerge = new();
        ffSubMerge.Subtitles = new List<string> { "srt" };
        ffSubMerge.MatchFilename = true;
        ffSubMerge.PreExecute(args);
        Assert.AreEqual(1, ffSubMerge.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_SubtitleTrackMerge_FileMatchesTests()
    {
        FfmpegBuilderSubtitleTrackMerge ffSubMerge = new();
        
        foreach (var item in new[] { 

                     (File: "The Big Bang Theory_S01E01_Pilot.en.closedcaptions.srt", Language: "English (CC)", IsMatch: true, Forced: false),
                     (File: "The Big Bang Theory_S01E01_Pilot.it.closedcaptions.srt", Language: "Italian (CC)", IsMatch: true, Forced: false),
                     (File: "The Big Bang Theory_S01E01_Pilot.it.forced.srt", Language: "Italian", IsMatch: true, Forced: true),
                 })
        {
            bool isMatch = ffSubMerge.FilenameMatches("The Big Bang Theory_S01E01_Pilot.mp4", item.File, out string lang, out bool forced);
            Assert.AreEqual(item.IsMatch, isMatch, "Not match: " + item.File);
            Assert.AreEqual(item.Forced, forced);
            Assert.AreEqual(item.Language, lang, "Language not matching in: " + item.Language);
        }
        
        foreach (var item in new[] { 
("test.en.cc.srt", "English (CC)", true),
("test.srt", "", true),
("test.en.srt", "English", true),
("test(en).srt", "English", true),
("test (en).srt", "English", true),
("test.en.hi.srt", "English (HI)", true),
("test.en.sdh.srt", "English (SDH)", true),
("test.de.srt", "German", true),
("test(de).srt", "German", true),
("test (de).srt", "German", true),

("nomatch.srt", "", false),
("nomatch.en.srt", "English", false),
("nomatch(en).srt", "English", false),
("nomatch (en).srt", "English", false)
        })
        {
            TestContext.WriteLine("File: " + item.Item1);
            bool isMatch = ffSubMerge.FilenameMatches("Test.mkv", item.Item1, out string lang, out bool forced);
            Assert.AreEqual(item.Item3, isMatch);
            Assert.AreEqual(item.Item2, lang, "Language not matching in: " + item.Item1);
        }
    }

    [TestMethod]
    public void FfmpegBuilder_Scale()
    {
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
        Assert.AreEqual(1, result);
    }
    
    [TestMethod]
    public void FfmpegBuilder_SubtitleClearDefault()
    {
        FfmpegBuilderSubtitleClearDefault ffClearDefault = new();
        ffClearDefault.LeaveForced = true;
        ffClearDefault.PreExecute(args);
        int result1 = ffClearDefault.Execute(args);
        Assert.AreEqual(1, result1);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result1);
        Assert.AreEqual(1, result);
    }

}

#endif