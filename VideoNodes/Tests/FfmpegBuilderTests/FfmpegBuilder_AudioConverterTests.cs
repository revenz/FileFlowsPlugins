#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;
using System.IO;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_AudioConverterTests: TestBase
{
    VideoInfo vii;
    NodeParameters args;
    private void Prepare()
    {
        string file = Path.Combine(TestPath, "basic.mkv");
        var logger = new TestLogger();
        var vi = new VideoInfoHelper(FfmpegPath, logger);
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
            },
            new AudioStream
            {
                Index = 6,
                TypeIndex = 4,
                IndexString = "0:a:4",
                Language = "en",
                Codec = "eac3",
                Channels = 5.1f
            },
            new AudioStream
            {
                Index = 7,
                TypeIndex = 5,
                IndexString = "0:a:5",
                Language = "en",
                Codec = "ac3",
                Channels = 5.1f
            }
        };
        args = new NodeParameters(file, logger, false, string.Empty, new LocalFileService());
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
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
        ffAudioConvert.Pattern = "fre";;
        ffAudioConvert.Field = FfmpegBuilderAudioConverter.FIELD_LANGUAGE;
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
        ffAudioConvert.Field = FfmpegBuilderAudioConverter.FIELD_LANGUAGE;
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
        var model = args.Variables[FfmpegBuilderNode.MODEL_KEY] as FfmpegModel;
        Assert.IsNotNull(model);
        var audio = model.AudioStreams[2];
        Assert.AreEqual("-map", audio.EncodingParameters[0]);
        Assert.AreEqual("0:a:{sourceTypeIndex}", audio.EncodingParameters[1]);
        Assert.AreEqual("-c:a:{index}", audio.EncodingParameters[2]);
        Assert.AreEqual("ac3", audio.EncodingParameters[3]);
        Assert.AreEqual("-ac:a:{index}", audio.EncodingParameters[4]);
        Assert.AreEqual("2.1", audio.EncodingParameters[5]);
        Assert.AreEqual("-b:a:{index}", audio.EncodingParameters[6]);
        Assert.AreEqual("384k", audio.EncodingParameters[7]);
    }
    
    [TestMethod]
    public void FfmpegBuilder_AudioConverter_Opus_All()
    {
        string file = Path.Combine(TestPath, "basic.mkv");
        var logger = new TestLogger();
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, new LocalFileService());
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderAudioConverter ffAudioConvert = new();
        ffAudioConvert.Codec = "opus";
        ffAudioConvert.PreExecute(args);
        int result = ffAudioConvert.Execute(args);
        Assert.AreEqual(1, result);
        
        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        ffExecutor.PreExecute(args);
        result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);

        var newInfo = vi.Read(args.WorkingFile).Value;
        Assert.AreEqual("opus", newInfo.AudioStreams[0].Codec);
    }
    
    
    [TestMethod]
    public void FfmpegBuilder_AudioConverter_Ac3ToOpus()
    {
        Prepare();

        FfmpegBuilderAudioConverter ffAudioConvert = new();
        ffAudioConvert.Codec = "opus";
        ffAudioConvert.Field = FfmpegBuilderAudioConverter.FIELD_CODEC;
        ffAudioConvert.Pattern = "ac3";
        ffAudioConvert.PreExecute(args);
        int result = ffAudioConvert.Execute(args);
        Assert.AreEqual(1, result);

        var model = args.Variables[FfmpegBuilderNode.MODEL_KEY] as FfmpegModel;
        Assert.IsNotNull(model);
        
        var firstAc3  = model.AudioStreams[0];
        var eac3 = model.AudioStreams[4];
        var secondAc3  = model.AudioStreams[5];
        Assert.AreEqual(firstAc3.ToString(), "0 / en / opus / 5.1 / Changed");
        Assert.AreEqual(secondAc3.ToString(), "5 / en / opus / 5.1 / Changed");
        Assert.AreEqual(eac3.ToString(), "4 / en / eac3 / 5.1");

    }
}

#endif