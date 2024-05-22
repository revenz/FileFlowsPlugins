#if(DEBUG)

using VideoNodes.Tests;
using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

/// <summary>
/// Tests for FFmpeg Builder Subtitle Track Merge
/// </summary>
[TestClass]
public class FFmpegBuild_SubtitleTrackMergeTests : TestBase
{
    /// <summary>
    /// Tests a subtitle using a pattern
    /// </summary>
    [TestMethod]
    public void PatternTest()
    {
        var args = new NodeParameters(TestFile_Subtitle, Logger, false, TestPath, new LocalFileService())
        {
            LibraryFileName = TestFile_Subtitle
        };
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
        
        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        var ffmpegBuilderStart = new FfmpegBuilderStart();
        ffmpegBuilderStart.PreExecute(args);
        Assert.AreEqual(1, ffmpegBuilderStart.Execute(args));

        int currentSubs = ffmpegBuilderStart.GetModel().SubtitleStreams.Count;

        var ele = new FfmpegBuilderSubtitleTrackMerge();
        ele.Subtitles = ["srt", "sub", "sup", "ass"];
        ele.Pattern = "^other";
        ele.Title = "Other Subtitle";
        ele.Language = "fre";
        ele.Default = true;
        ele.Forced = true;
        ele.PreExecute(args);
        Assert.AreEqual(1, ele.Execute(args));

        int newSubs = ffmpegBuilderStart.GetModel().SubtitleStreams.Count;
        Assert.AreEqual(currentSubs + 1, newSubs);

        var newSub = ffmpegBuilderStart.GetModel().SubtitleStreams.Last();
        
        Assert.AreEqual("Other Subtitle", newSub.Title);
        Assert.AreEqual("fre", newSub.Language);
        Assert.IsTrue(newSub.IsDefault);
        Assert.IsTrue(newSub.IsForced);
    }
    
    /// <summary>
    /// Tests a subtitle using file matches
    /// </summary>
    [TestMethod]
    public void FileMatches()
    {
        var args = new NodeParameters(TestFile_Subtitle, Logger, false, TestPath, new LocalFileService())
        {
            LibraryFileName = TestFile_Subtitle
        };
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
        
        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

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
}


#endif