#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_AddAudioTests : TestBase
{
    VideoInfo vii;
    NodeParameters args;
    private void Prepare()
    {
        var vi = new VideoInfoHelper(FfmpegPath, Logger);
        vii = vi.Read(TestFile_BasicMkv);
        vii.AudioStreams = new List<AudioStream>
        {
            new AudioStream
            {
                Index = 2,
                IndexString = "0:a:0",
                Language = "en",
                Codec = "AC3",
                Channels = 5.1f
            },
            new AudioStream
            {
                Index = 3,
                IndexString = "0:a:1",
                Language = "en",
                Codec = "AAC",
                Channels = 2
            },
            new AudioStream
            {
                Index = 4,
                IndexString = "0:a:3",
                Language = "en",
                Codec = "AAC",
                Channels = 2
            },
            new AudioStream
            {
                Index = 5,
                IndexString = "0:a:4",
                Language = "en",
                Codec = "AAC",
                Channels = 5.1f
            }
        };
        args = new NodeParameters(TestFile_BasicMkv, Logger, false, string.Empty, new LocalFileService());
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_GetSource_1()
    {
        Prepare();
        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.CustomTrackSelection = true;
        ffAddAudio.TrackSelectionOptions = new()
        {
            new("Codec", "AC*"),
            new("Language", "English"),
            new("Codec", "!aac"),
        };
        ffAddAudio.PreExecute(args);
        var source = ffAddAudio.GetSourceTrack<AudioStream>();
        Assert.IsNotNull(source);
        Logger.ILog("Source Track: " + source);
        Assert.AreEqual("en", source.Language);
        Assert.AreEqual("AC3", source.Codec);

    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_AacStereo()
    {
        Prepare();

        FfmpegBuilderAudioAddTrack  ffAddAudio = new();
        ffAddAudio.Codec = "aac";
        ffAddAudio.Channels = 2;
        var best = ffAddAudio.GetBestAudioTrack(args, vii.AudioStreams);

        Assert.IsNotNull(best);

        Assert.AreEqual(3, best.Index);
        Assert.AreEqual("AAC", best.Codec);
        Assert.AreEqual(2f, best.Channels);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_AacSameAsSource()
    {
        Prepare();

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "aac";
        ffAddAudio.Channels = 0;
        var best = ffAddAudio.GetBestAudioTrack(args, vii.AudioStreams);

        Assert.IsNotNull(best);

        Assert.AreEqual(5, best.Index);
        Assert.AreEqual("AAC", best.Codec);
        Assert.AreEqual(5.1f, best.Channels);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_Ac3SameAsSource()
    {
        Prepare();

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "ac3";
        ffAddAudio.Channels = 0;
        ffAddAudio.Index = 1;
        var best = ffAddAudio.GetBestAudioTrack(args, vii.AudioStreams);

        Assert.IsNotNull(best);

        Assert.AreEqual(2, best.Index);
        Assert.AreEqual("AC3", best.Codec);
        Assert.AreEqual(5.1f, best.Channels);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_DtsSame()
    {
        Prepare();

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "dts";
        ffAddAudio.Channels = 0;
        var best = ffAddAudio.GetBestAudioTrack(args, vii.AudioStreams);

        Assert.IsNotNull(best);

        Assert.AreEqual(2, best.Index);
        Assert.AreEqual("AC3", best.Codec);
        Assert.AreEqual(5.1f, best.Channels);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_DtsStereo()
    {
        Prepare();

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "dts";
        ffAddAudio.Channels = 2;
        var best = ffAddAudio.GetBestAudioTrack(args, vii.AudioStreams);

        Assert.IsNotNull(best);

        Assert.AreEqual(3, best.Index);
        Assert.AreEqual("AAC", best.Codec);
        Assert.AreEqual(2f, best.Channels);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_DtsMono()
    {
        Prepare();

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "dts";
        ffAddAudio.Channels = 1;
        var best = ffAddAudio.GetBestAudioTrack(args, vii.AudioStreams);

        Assert.IsNotNull(best);

        Assert.AreEqual(3, best.Index);
        Assert.AreEqual("AAC", best.Codec);
        Assert.AreEqual(2f, best.Channels);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_Basic()
    {
        Prepare();

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "dts";
        ffAddAudio.Channels = 1;
        ffAddAudio.Index = 1000;
        ffAddAudio.PreExecute(args);
        var output = ffAddAudio.Execute(args);
        Assert.AreEqual(1, output);

        FfmpegModel model = (FfmpegModel)args.Variables["FfmpegBuilderModel"];
        var last = model.AudioStreams.Last();
        Assert.AreEqual(1, last.Channels);
    }

    [TestMethod]
    public void FfmpegBuilder_AddAudio_Basic_2()
    {
        Prepare();

        FfmpegBuilderAudioAddTrack ffAddAudio = new();
        ffAddAudio.Codec = "dts";
        ffAddAudio.Channels = 2;
        ffAddAudio.Index = 1000;
        ffAddAudio.PreExecute(args);
        var output = ffAddAudio.Execute(args);
        Assert.AreEqual(1, output);

        FfmpegModel model = (FfmpegModel)args.Variables["FfmpegBuilderModel"];
        var last = model.AudioStreams.Last();
        Assert.AreEqual(2, last.Channels);
    }
}

#endif