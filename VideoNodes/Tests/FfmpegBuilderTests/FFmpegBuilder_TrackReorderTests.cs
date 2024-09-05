#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

/// <summary>
/// Tests for track reorders
/// </summary>
[TestClass]
public class FFmpegBuilder_TrackReorderTests
{
    /// <summary>
    /// Basic test
    /// </summary>
    [TestMethod]
    public void Basic()
    {
        var element = new FfmpegBuilderAudioTrackReorder();
        List<FfmpegAudioStream> original = new()
        {
            new() { Index = 1, Channels = 2, Stream = new() { Language = "en", Codec = "ac3", Channels = 5.1f } },
            new() { Index = 2, Channels = 0, Stream = new() { Language = "en", Codec = "ac3", Channels = 5.1f } },
            new() { Index = 3, Channels = 5.1f, Stream = new() { Language = "fr", Codec = "ac3", Channels = 5.1f } },
            new() { Index = 4, Channels = 5.1f, Stream = new() { Language = "en", Codec = "aac", Channels = 2 } },
        };
        element.Channels = new() { "5.1" };
        var reordered = element.Reorder(original);
        Assert.IsFalse(element.AreSame(original, reordered));
        Assert.AreEqual(2, reordered[0].Index);
        Assert.AreEqual(3, reordered[1].Index);
        Assert.AreEqual(4, reordered[2].Index);
        Assert.AreEqual(1, reordered[3].Index);
    }
    
    /// <summary>
    /// Basic test 2
    /// </summary>
    [TestMethod]
    public void Basic_2()
    {
        var element = new FfmpegBuilderAudioTrackReorder();
        List<FfmpegAudioStream> original = new()
        {
            new() { Index = 1, Channels = 2, Stream = new() { Language = "en", Codec = "ac3", Channels = 5.1f } },
            new() { Index = 2, Channels = 0, Stream = new() { Language = "en", Codec = "ac3", Channels = 5.1f } },
            new() { Index = 3, Channels = 5.1f, Stream = new() { Language = "fr", Codec = "ac3", Channels = 7.1f } },
            new() { Index = 4, Channels = 5.1f, Stream = new() { Language = "en", Codec = "aac", Channels = 2 } },
        };
        element.Channels = new() { "5.1" };
        var reordered = element.Reorder(original);
        Assert.IsFalse(element.AreSame(original, reordered));
        Assert.AreEqual(2, reordered[0].Index);
        Assert.AreEqual(3, reordered[1].Index);
        Assert.AreEqual(4, reordered[2].Index);
        Assert.AreEqual(1, reordered[3].Index);
    }
    
    /// <summary>
    /// Basic test 3
    /// </summary>
    [TestMethod]
    public void Basic_3()
    {
        var element = new FfmpegBuilderAudioTrackReorder();
        List<FfmpegAudioStream> original = new()
        {
            new() { Index = 1, Channels = 2, Stream = new() { Language = "en", Codec = "ac3", Channels = 5.1f } },
            new() { Index = 2, Channels = 0, Stream = new() { Language = "en", Codec = "ac3", Channels = 5.1f } },
            new() { Index = 3, Channels = 5.1f, Stream = new() { Language = "fr", Codec = "ac3", Channels = 7.1f } },
            new() { Index = 4, Channels = 5.1f, Stream = new() { Language = "en", Codec = "aac", Channels = 2 } },
        };
        element.Channels = new() { "5.1" };
        element.OrderedTracks = new() { "aac", "ac3" };
        var reordered = element.Reorder(original);
        Assert.IsFalse(element.AreSame(original, reordered));
        Assert.AreEqual(4, reordered[0].Index);
        Assert.AreEqual(2, reordered[1].Index);
        Assert.AreEqual(3, reordered[2].Index);
        Assert.AreEqual(1, reordered[3].Index);
    }
}

#endif