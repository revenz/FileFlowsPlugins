#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_AudioConverterTests
{
    VideoInfo vii;
    NodeParameters args;
    private void Prepare()
    {
        const string file = @"D:\videos\unprocessed\basic.mkv";
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        vii = vi.Read(file);
        vii.AudioStreams = new List<AudioStream>
        {
            new AudioStream
            {
                Index = 2,
                TypeIndex = 0,
                IndexString = "0:a:0",
                Language = "en",
                Codec = "AC3",
                Channels = 5.1f
            },
            new AudioStream
            {
                Index = 3,
                TypeIndex = 1,
                IndexString = "0:a:1",
                Language = "fre",
                Codec = "AAC",
                Channels = 2
            },
            new AudioStream
            {
                Index = 4,
                TypeIndex = 2,
                IndexString = "0:a:2",
                Language = "deu",
                Codec = "AAC",
                Channels = 2,
                Title = "third"
            },
            new AudioStream
            {
                Index = 5,
                TypeIndex = 3,
                IndexString = "0:a:3",
                Language = "en",
                Codec = "AAC",
                Channels = 5.1f
            }
        };
        args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));
    }

    [TestMethod]
    public void FfmpegBuilder_AudioConverter_Aac_French()
    {
        Prepare();

        FfmpegBuilderAudioConverter ffAudioConvert = new();
        ffAudioConvert.Codec = "aac";
        ffAudioConvert.Pattern = "fre";
        ffAudioConvert.UseLanguageCode = true;
        ffAudioConvert.PreExecute(args);
        int result = ffAudioConvert.Execute(args);
        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AudioConverter_Ac3_French()
    {
        Prepare();

        FfmpegBuilderAudioConverter ffAudioConvert = new();
        ffAudioConvert.Codec = "ac3";
        ffAudioConvert.Pattern = "fre";
        ffAudioConvert.UseLanguageCode = true;
        ffAudioConvert.PreExecute(args);
        int result = ffAudioConvert.Execute(args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AudioConverter_Aac_All()
    {
        Prepare();

        FfmpegBuilderAudioConverter ffAudioConvert = new();
        ffAudioConvert.Codec = "aac";
        ffAudioConvert.PreExecute(args);
        int result = ffAudioConvert.Execute(args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AudioConverter_Title_Ac3()
    {
        Prepare();

        FfmpegBuilderAudioConverter ffAudioConvert = new();
        ffAudioConvert.Codec = "ac3";
        ffAudioConvert.Pattern = "thir[a-z]"; 
        ffAudioConvert.PreExecute(args);
        int result = ffAudioConvert.Execute(args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_AudioConverter_Title_Ac3_Complete()
    {
        Prepare();

        FfmpegBuilderAudioConverter ffAudioConvert = new();
        ffAudioConvert.Codec = "ac3";
        ffAudioConvert.Pattern = "thir[a-z]";
        ffAudioConvert.Channels = 2.1f;
        ffAudioConvert.Bitrate = 384;
        ffAudioConvert.PreExecute(args);
        int result = ffAudioConvert.Execute(args);
        Assert.AreEqual(1, result);
        var model = args.Variables["FFMPEG_BUILDER_MODEL"] as FfmpegModel;
        Assert.IsNotNull(model);
        var audio = model.AudioStreams[2];
        Assert.AreEqual("-map", audio.EncodingParameters[0]);
        Assert.AreEqual("0:a:2", audio.EncodingParameters[1]);
        Assert.AreEqual("-c:a:{index}", audio.EncodingParameters[2]);
        Assert.AreEqual("ac3", audio.EncodingParameters[3]);
        Assert.AreEqual("-ac:a:{index}", audio.EncodingParameters[4]);
        Assert.AreEqual("2.1", audio.EncodingParameters[5]);
        Assert.AreEqual("-b:a:{index}", audio.EncodingParameters[6]);
        Assert.AreEqual("384k", audio.EncodingParameters[7]);
    }
    //[TestMethod]
    //public void FfmpegBuilder_AudioConverter_AacSameAsSource()
    //{
    //    Prepare();

    //    FfmpegBuilderAudioConverter ffAudioConvert = new();
    //    ffAudioConvert.Codec = "aac";
    //    ffAudioConvert.Channels = 0;
    //    var best = ffAudioConvert.GetBestAudioTrack(args, vii.AudioStreams);

    //    Assert.IsNotNull(best);

    //    Assert.AreEqual(5, best.Index);
    //    Assert.AreEqual("AAC", best.Codec);
    //    Assert.AreEqual(5.1f, best.Channels);
    //}

    //[TestMethod]
    //public void FfmpegBuilder_AudioConverter_Ac3SameAsSource()
    //{
    //    Prepare();

    //    FfmpegBuilderAudioConverter ffAudioConvert = new();
    //    ffAudioConvert.Codec = "ac3";
    //    ffAudioConvert.Channels = 0;
    //    ffAudioConvert.Index = 1;
    //    var best = ffAudioConvert.GetBestAudioTrack(args, vii.AudioStreams);

    //    Assert.IsNotNull(best);

    //    Assert.AreEqual(2, best.Index);
    //    Assert.AreEqual("AC3", best.Codec);
    //    Assert.AreEqual(5.1f, best.Channels);
    //}

    //[TestMethod]
    //public void FfmpegBuilder_AudioConverter_DtsSame()
    //{
    //    Prepare();

    //    FfmpegBuilderAudioConverter ffAudioConvert = new();
    //    ffAudioConvert.Codec = "dts";
    //    ffAudioConvert.Channels = 0;
    //    var best = ffAudioConvert.GetBestAudioTrack(args, vii.AudioStreams);

    //    Assert.IsNotNull(best);

    //    Assert.AreEqual(2, best.Index);
    //    Assert.AreEqual("AC3", best.Codec);
    //    Assert.AreEqual(5.1f, best.Channels);
    //}

    //[TestMethod]
    //public void FfmpegBuilder_AudioConverter_DtsStereo()
    //{
    //    Prepare();

    //    FfmpegBuilderAudioConverter ffAudioConvert = new();
    //    ffAudioConvert.Codec = "dts";
    //    ffAudioConvert.Channels = 2;
    //    var best = ffAudioConvert.GetBestAudioTrack(args, vii.AudioStreams);

    //    Assert.IsNotNull(best);

    //    Assert.AreEqual(3, best.Index);
    //    Assert.AreEqual("AAC", best.Codec);
    //    Assert.AreEqual(2f, best.Channels);
    //}

    //[TestMethod]
    //public void FfmpegBuilder_AudioConverter_DtsMono()
    //{
    //    Prepare();

    //    FfmpegBuilderAudioConverter ffAudioConvert = new();
    //    ffAudioConvert.Codec = "dts";
    //    ffAudioConvert.Channels = 1;
    //    var best = ffAudioConvert.GetBestAudioTrack(args, vii.AudioStreams);

    //    Assert.IsNotNull(best);

    //    Assert.AreEqual(3, best.Index);
    //    Assert.AreEqual("AAC", best.Codec);
    //    Assert.AreEqual(2f, best.Channels);
    //}
}

#endif