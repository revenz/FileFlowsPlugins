#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_LanguageRemoverTests : VideoTestBase
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
    
    /// <summary>
    /// Test German is matched and removed
    /// </summary>
    [TestMethod]
    public void KeepEngAndOriginalSubtitles()
    {
        int originaluNonDeletedSubtitles = Model.SubtitleStreams.Count(x => x.Deleted == false);
        FFmpegBuilderLanguageRemover ffLangRemover = new();
        args.Variables["OriginalLanguage"] = "fre";
        ffLangRemover.Languages = [ "orig", "eng" ];
        ffLangRemover.NotMatching = true;
        ffLangRemover.StreamType = "Subtitle";
        ffLangRemover.PreExecute(args);
        Assert.AreEqual(1, ffLangRemover.Execute(args));
        List<FfmpegSubtitleStream> nonDeletedSubtitles = new();

        Logger.ILog(new string('-', 100));
        foreach (var sub in Model.SubtitleStreams.Where(x => x.Deleted))
        {
            Logger.ILog("Deleted subtitle: " + sub);
        }
        Logger.ILog(new string('-', 100));
        foreach (var sub in Model.SubtitleStreams.Where(x => x.Deleted == false))
        {
            Logger.ILog("Non deleted subtitle: " + sub);
            nonDeletedSubtitles.Add(sub);
        }
        Assert.AreNotEqual(originaluNonDeletedSubtitles, nonDeletedSubtitles.Count);
        Assert.AreEqual(nonDeletedSubtitles.Count, 2);
        Assert.AreEqual("eng", nonDeletedSubtitles[0].Language);
        Assert.AreEqual("fre", nonDeletedSubtitles[1].Language);
    }
    
    /// <summary>
    /// Test German is matched and removed
    /// </summary>
    [TestMethod]
    public void KeepOriginalSubtitles()
    {
        int originaluNonDeletedSubtitles = Model.SubtitleStreams.Count(x => x.Deleted == false);
        FFmpegBuilderLanguageRemover ffLangRemover = new();
        args.Variables["OriginalLanguage"] = "fre";
        ffLangRemover.Languages = ["orig"];
        ffLangRemover.StreamType = "Subtitle";
        ffLangRemover.NotMatching = true;
        ffLangRemover.PreExecute(args);
        Assert.AreEqual(1, ffLangRemover.Execute(args));
        List<FfmpegSubtitleStream> nonDeletedSubtitles = new();

        Logger.ILog(new string('-', 100));
        foreach (var sub in Model.SubtitleStreams.Where(x => x.Deleted))
        {
            Logger.ILog("Deleted subtitle: " + sub);
        }
        Logger.ILog(new string('-', 100));
        foreach (var sub in Model.SubtitleStreams.Where(x => x.Deleted == false))
        {
            Logger.ILog("Non deleted subtitle: " + sub);
            nonDeletedSubtitles.Add(sub);
        }
        Assert.AreNotEqual(originaluNonDeletedSubtitles, nonDeletedSubtitles.Count);
        Assert.AreEqual(nonDeletedSubtitles.Count, 1);
        Assert.AreEqual("fre", nonDeletedSubtitles[0].Language);
    }
    
    /// <summary>
    /// Tests english subtitles are removed
    /// </summary>
    [TestMethod]
    public void RemoveEnglishSubtitles()
    {
        int originaluNonDeletedSubtitles = Model.SubtitleStreams.Count(x => x.Deleted == false);
        FFmpegBuilderLanguageRemover ffLangRemover = new();
        args.Variables["OriginalLanguage"] = "fre";
        ffLangRemover.Languages = ["eng"];
        ffLangRemover.StreamType = "Subtitle";
        ffLangRemover.PreExecute(args);
        Assert.AreEqual(1, ffLangRemover.Execute(args));
        List<FfmpegSubtitleStream> nonDeletedSubtitles = new();

        Logger.ILog(new string('-', 100));
        foreach (var sub in Model.SubtitleStreams.Where(x => x.Deleted))
        {
            Logger.ILog("Deleted subtitle: " + sub);
        }
        Logger.ILog(new string('-', 100));
        foreach (var sub in Model.SubtitleStreams.Where(x => x.Deleted == false))
        {
            Logger.ILog("Non deleted subtitle: " + sub);
            nonDeletedSubtitles.Add(sub);
        }
        Assert.AreNotEqual(originaluNonDeletedSubtitles, nonDeletedSubtitles.Count);
        Assert.AreEqual(nonDeletedSubtitles.Count, originaluNonDeletedSubtitles - 1);
        Assert.IsFalse(nonDeletedSubtitles.Any(x =>
            x.Language.Contains("en", StringComparison.InvariantCultureIgnoreCase)));
    }
    /// <summary>
    /// Tests english subtitles are removed
    /// </summary>
    [TestMethod]
    public void RemoveEnglishAudio()
    {
        int originaalNondDeleted = Model.AudioStreams.Count(x => x.Deleted == false);
        FFmpegBuilderLanguageRemover ffLangRemover = new();
        args.Variables["OriginalLanguage"] = "fre";
        ffLangRemover.Languages = ["eng"];
        ffLangRemover.StreamType = "Audio";
        ffLangRemover.PreExecute(args);
        Assert.AreEqual(1, ffLangRemover.Execute(args));
        List<FfmpegAudioStream> nonDeleted = new();

        Logger.ILog(new string('-', 100));
        foreach (var stream in Model.AudioStreams.Where(x => x.Deleted))
        {
            Logger.ILog("Deleted audio: " + stream);
        }
        Logger.ILog(new string('-', 100));
        foreach (var stream in Model.AudioStreams.Where(x => x.Deleted == false))
        {
            Logger.ILog("Non deleted audio: " + stream);
            nonDeleted.Add(stream);
        }
        Assert.AreNotEqual(originaalNondDeleted, nonDeleted.Count);
        Assert.AreEqual(nonDeleted.Count, originaalNondDeleted - 1);
        Assert.IsFalse(nonDeleted.Any(x =>
            x.Language.Contains("en", StringComparison.InvariantCultureIgnoreCase)));
    }
}

#endif