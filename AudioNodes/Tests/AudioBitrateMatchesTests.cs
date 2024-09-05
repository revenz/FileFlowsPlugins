#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.AudioNodes.Tests;

[TestClass]
public class AudioBitrateMatchesTests
{
    
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