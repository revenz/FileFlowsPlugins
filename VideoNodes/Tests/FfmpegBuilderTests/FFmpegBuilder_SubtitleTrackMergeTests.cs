#if(DEBUG)

using VideoNodes.Tests;
using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

/// <summary>
/// Tests for FFmpeg Builder Subtitle Track Merge
/// </summary>
[TestClass]
public class FFmpegBuild_SubtitleTrackMergeTests : VideoTestBase
{
//     /// <summary>
//     /// Tests a subtitle using a pattern
//     /// </summary>
//     [TestMethod]
//     public void PatternTest()
//     {
//         var args = GetVideoNodeParameters();
//         var videoFile = new VideoFile();
//         videoFile.PreExecute(args);
//         videoFile.Execute(args);
//
//         var ffmpegBuilderStart = new FfmpegBuilderStart();
//         ffmpegBuilderStart.PreExecute(args);
//         Assert.AreEqual(1, ffmpegBuilderStart.Execute(args));
//
//         int currentSubs = ffmpegBuilderStart.GetModel().SubtitleStreams.Count;
//
//         var ele = new FfmpegBuilderSubtitleTrackMerge();
//         ele.Subtitles = ["srt", "sub", "sup", "ass"];
//         ele.Pattern = "^other";
//         ele.Title = "Other Subtitle";
//         ele.Default = true;
//         ele.Forced = true;
//         ele.PreExecute(args);
//         Assert.AreEqual(1, ele.Execute(args));
//
//         int newSubs = ffmpegBuilderStart.GetModel().SubtitleStreams.Count;
//         Assert.AreEqual(currentSubs + 1, newSubs);
//
//         var newSub = ffmpegBuilderStart.GetModel().SubtitleStreams.Last();
//         
//         Assert.AreEqual("Other Subtitle", newSub.Title);
//         Assert.IsTrue(newSub.IsDefault);
//         Assert.IsTrue(newSub.IsForced);
//     }
    
    /// <summary>
    /// Tests a subtitle using file matches
    /// </summary>
    [TestMethod]
    public void FileMatches()
    {
        var args = GetVideoNodeParameters();
        var videoFile = new VideoFile();
        videoFile.PreExecute(args);
        videoFile.Execute(args);

        var ffmpegBuilderStart = new FfmpegBuilderStart();
        ffmpegBuilderStart.PreExecute(args);
        Assert.AreEqual(1, ffmpegBuilderStart.Execute(args));

        int currentSubs = ffmpegBuilderStart.GetModel().SubtitleStreams.Count;

        var ele = new FfmpegBuilderSubtitleTrackMerge();
        ele.Subtitles = ["srt", "sub", "sup", "ass"];
        ele.MatchFilename = true;
        ele.PreExecute(args);
        Assert.AreEqual(1, ele.Execute(args));

        int newSubs = ffmpegBuilderStart.GetModel().SubtitleStreams.Count;
        Assert.AreEqual(currentSubs + 1, newSubs);

        var newSub = ffmpegBuilderStart.GetModel().SubtitleStreams.Last();
        
        Assert.AreEqual("English", newSub.Title);
        Assert.AreEqual("eng", newSub.Language);
        Assert.IsFalse(newSub.IsDefault);
        Assert.IsFalse(newSub.IsForced);
    }

    /// <summary>
    /// Test a file with a bad name that could match other languages
    /// </summary>
    [TestMethod]
    public void BadName()
    {
        string filename =
            "/volume1/syno/seedbox/anime/Solo.Leveling.S02E09.It.was.All.Worth.It.1080p.AMZN.WEB-DL.DDP2.0.H.264-ANiMEZ.mkv";
        
        string subfile =
            "/volume1/syno/seedbox/anime/Solo.Leveling.S02E09.It.was.All.Worth.It.1080p.AMZN.WEB-DL.DDP2.0.H.264-ANiMEZ.eng.srt";

        var ele = new FfmpegBuilderSubtitleTrackMerge();
        ele.FilenameMatches(filename, subfile, out var language, out var forced);
        
        Assert.AreEqual("eng", language);
        Assert.IsFalse(forced);
        
    }
}


#endif