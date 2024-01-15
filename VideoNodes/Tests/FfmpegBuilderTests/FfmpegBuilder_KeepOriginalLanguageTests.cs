#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_KeepOriginalLanguageTests
{
    VideoInfo vii;
    NodeParameters args;
    TestLogger logger = new TestLogger();
    private void Prepare()
    {
        const string file = @"D:\videos\unprocessed\basic.mkv";
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        vii = vi.Read(file);
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
                Language = "fre",
                Codec = "AAC",
                Channels = 2
            },
            new AudioStream
            {
                Index = 5,
                IndexString = "0:a:4",
                Language = "deu",
                Codec = "AAC",
                Channels = 5.1f
            }
        };
        
        vii.SubtitleStreams = new List<SubtitleStream>
        {
            new()
            {
                Index = 2,
                IndexString = "0:s:0",
                Language = "en",
                Codec = "AC3"
            },
            new()
            {
                Index = 3,
                IndexString = "0:s:1",
                Language = "en",
                Codec = "AAC"
            },
            new()
            {
                Index = 4,
                IndexString = "0:s:3",
                Language = "fre",
                Codec = "AAC",
            },
            new()
            {
                Index = 5,
                IndexString = "0:s:4",
                Language = "deu",
                Codec = "AAC"
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

    private FfmpegModel GetFFmpegModel()
    {
        return args.Variables["FfmpegBuilderModel"] as FfmpegModel;
    }

    [TestMethod]
    public void FfmpegBuilder_Audio_German()
    {
        Prepare();

        FfmpegBuilderKeepOriginalLanguage ffElement = new();
        ffElement.StreamType = "Audio";
        args.Variables["OriginalLanguage"] = "German";
        ffElement.PreExecute(args);
        var result = ffElement.Execute(args);
        var log = logger.ToString();

        Assert.AreEqual(1, result);
        var model = GetFFmpegModel();
        var kept = model.AudioStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(1, kept.Count);
        Assert.AreEqual("deu", kept[0].Language);
    }
    
    [TestMethod]
    public void FfmpegBuilder_Audio_None()
    {
        Prepare();

        FfmpegBuilderKeepOriginalLanguage ffElement = new();
        ffElement.StreamType = "Audio";
        ffElement.FirstIfNone = true;
        args.Variables["OriginalLanguage"] = "Maori";
        ffElement.PreExecute(args);
        var result = ffElement.Execute(args);
        var log = logger.ToString();

        Assert.AreEqual(1, result);
        var model = GetFFmpegModel();
        var kept = model.AudioStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(1, kept.Count);
        Assert.AreEqual("en", kept[0].Language);
    }
    
    
    [TestMethod]
    public void FfmpegBuilder_Audio_OriginalAndEnglish()
    {
        Prepare();

        FfmpegBuilderKeepOriginalLanguage ffElement = new();
        ffElement.StreamType = "Audio";
        ffElement.AdditionalLanguages = new List<string>{ "English" };
        args.Variables["OriginalLanguage"] = "French";
        ffElement.PreExecute(args);
        var result = ffElement.Execute(args);
        var log = logger.ToString();

        Assert.AreEqual(1, result);
        var model = GetFFmpegModel();
        var kept = model.AudioStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(3, kept.Count);
        Assert.AreEqual("en", kept[0].Language);
        Assert.AreEqual("en", kept[1].Language);
        Assert.AreEqual("fre", kept[2].Language);
    }
    

    [TestMethod]
    public void FfmpegBuilder_Both_German()
    {
        Prepare();

        FfmpegBuilderKeepOriginalLanguage ffElement = new();
        ffElement.StreamType = "Both";
        args.Variables["OriginalLanguage"] = "German";
        ffElement.PreExecute(args);
        var result = ffElement.Execute(args);
        var log = logger.ToString();

        Assert.AreEqual(1, result);
        var model = GetFFmpegModel();
        var kept = model.AudioStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(1, kept.Count);
        Assert.AreEqual("deu", kept[0].Language);
        
        var subKept = model.SubtitleStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(1, subKept.Count);
        Assert.AreEqual("deu", subKept[0].Language);
    }
    
    [TestMethod]
    public void FfmpegBuilder_Both_None()
    {
        Prepare();

        FfmpegBuilderKeepOriginalLanguage ffElement = new();
        ffElement.StreamType = "Both";
        ffElement.FirstIfNone = true;
        args.Variables["OriginalLanguage"] = "Maori";
        ffElement.PreExecute(args);
        var result = ffElement.Execute(args);
        var log = logger.ToString();

        Assert.AreEqual(1, result);
        var model = GetFFmpegModel();
        var kept = model.AudioStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(1, kept.Count);
        Assert.AreEqual("en", kept[0].Language);
        
        var subKept = model.SubtitleStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(1, subKept.Count);
        Assert.AreEqual("en", subKept[0].Language);
    }
    
    [TestMethod]
    public void FfmpegBuilder_Both_OriginalAndEnglish()
    {
        Prepare();

        FfmpegBuilderKeepOriginalLanguage ffElement = new();
        ffElement.StreamType = "Both";
        ffElement.AdditionalLanguages = new List<string>{ "English" };
        args.Variables["OriginalLanguage"] = "French";
        ffElement.PreExecute(args);
        var result = ffElement.Execute(args);
        var log = logger.ToString();

        Assert.AreEqual(1, result);
        var model = GetFFmpegModel();
        var kept = model.AudioStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(3, kept.Count);
        Assert.AreEqual("en", kept[0].Language);
        Assert.AreEqual("en", kept[1].Language);
        Assert.AreEqual("fre", kept[2].Language);
        
        
        var subKept = model.SubtitleStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(3, subKept.Count);
        Assert.AreEqual("en", subKept[0].Language);
        Assert.AreEqual("en", subKept[1].Language);
        Assert.AreEqual("fre", subKept[2].Language);
    }
    
    [TestMethod]
    public void FfmpegBuilder_Both_OriginalAndEnglish_OnlyFirst()
    {
        Prepare();

        FfmpegBuilderKeepOriginalLanguage ffElement = new();
        ffElement.StreamType = "Both";
        ffElement.KeepOnlyFirst = true;
        ffElement.AdditionalLanguages = new List<string>{ "English" };
        args.Variables["OriginalLanguage"] = "French";
        ffElement.PreExecute(args);
        var result = ffElement.Execute(args);
        var log = logger.ToString();

        Assert.AreEqual(1, result);
        var model = GetFFmpegModel();
        var kept = model.AudioStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(2, kept.Count);
        Assert.AreEqual("en", kept[0].Language);
        Assert.AreEqual("0:a:0", kept[0].Stream.IndexString);
        Assert.AreEqual("fre", kept[1].Language);
        
        
        var subKept = model.SubtitleStreams.Where(x => x.Deleted == false).ToList();
        Assert.AreEqual(2, subKept.Count);
        Assert.AreEqual("en", subKept[0].Language);
        Assert.AreEqual("0:s:0", subKept[0].Stream.IndexString);
        Assert.AreEqual("fre", subKept[1].Language);
    }
}

#endif