﻿#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace FileFlows.AudioNodes.Tests;
[TestClass]
public class AudioBitrateMatchesTests
{
    const string file = @"/home/john/Music/test/test.mp3";
    readonly string ffmpegExe = (OperatingSystem.IsLinux() ? "/usr/bin/ffmpeg" :  @"C:\utils\ffmpeg\ffmpeg.exe");
    readonly string ffprobe = (OperatingSystem.IsLinux() ? "/usr/bin/ffprobe" :  @"C:\utils\ffmpeg\ffprobe.exe");
    
    [TestMethod]
    public void AudioInfo_SplitTrack()
    {
        var logger = new TestLogger();
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_GREATER_THAN, 90, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_GREATER_THAN, 100, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_GREATER_THAN_OR_EQUAL, 90, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_GREATER_THAN_OR_EQUAL, 99, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_LESS_THAN, 110, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_LESS_THAN, 100, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_LESS_THAN_OR_EQUAL, 110, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_LESS_THAN_OR_EQUAL, 101, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_NOT_EQUALS, 100, 100));
        Assert.IsFalse(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_EQUALS, 90, 100));
        
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_GREATER_THAN, 110, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_GREATER_THAN, 101, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_GREATER_THAN_OR_EQUAL, 110, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_GREATER_THAN_OR_EQUAL, 100, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_LESS_THAN, 90, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_LESS_THAN, 99, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_LESS_THAN_OR_EQUAL, 90, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_LESS_THAN_OR_EQUAL, 100, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_NOT_EQUALS, 99, 100));
        Assert.IsTrue(AudioBitrateMatches.DoMatch(logger, AudioBitrateMatches.MATCH_EQUALS, 100, 100));
    }
}

#endif