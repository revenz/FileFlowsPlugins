#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_TrackSorterTests
{

    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnSorters()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac" },
            new() { Index = 2, Channels = 2, Language = "fr", Codec = "aac" },
            new() { Index = 3, Channels = 5.1f, Language = "en", Codec = "ac3" },
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new("Language", "en"),
            new("Channels", ">=5.1"),
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(3, sorted[0].Index);
        Assert.AreEqual(1, sorted[1].Index);
        Assert.AreEqual(2, sorted[2].Index);

        // Additional assertions for logging
        Assert.AreEqual("3 / en / ac3 / 5.1", sorted[0].ToString());
        Assert.AreEqual("1 / en / aac / 2.0", sorted[1].ToString());
        Assert.AreEqual("2 / fr / aac / 2.0", sorted[2].ToString());
    }


    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnChannels()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac" },
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "aac" },
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" },
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new("Channels", ">=5.1"),
        };

        // Act
        var result = trackSorter.ProcessStreams(args, streams);

        // Assert
        Assert.AreEqual(2, streams[0].Index);
        Assert.AreEqual(3, streams[1].Index);
        Assert.AreEqual(1, streams[2].Index);

        // Additional assertions for logging
        Assert.AreEqual("2 / fr / aac / 5.1", streams[0].ToString());
        Assert.AreEqual("3 / en / ac3 / 7.1", streams[1].ToString());
        Assert.AreEqual("1 / en / aac / 2.0", streams[2].ToString());
    }

    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnLanguageThenCodec()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac" },
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "aac" },
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" },
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new("Language", "en"),
            new("Codec", "ac3"),
        };

        // Act
        var result = trackSorter.ProcessStreams(args, streams);

        // Assert
        Assert.AreEqual(3, streams[0].Index);
        Assert.AreEqual(1, streams[1].Index);
        Assert.AreEqual(2, streams[2].Index);

        // Additional assertions for logging
        Assert.AreEqual("3 / en / ac3 / 7.1", streams[0].ToString());
        Assert.AreEqual("1 / en / aac / 2.0", streams[1].ToString());
        Assert.AreEqual("2 / fr / aac / 5.1", streams[2].ToString());
    }

    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnCustomMathOperation()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac" },
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "aac" },
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" },
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new("Channels", ">4.0"),
        };

        // Act
        var result = trackSorter.ProcessStreams(args, streams);

        // Assert
        Assert.AreEqual(2, streams[0].Index);
        Assert.AreEqual(3, streams[1].Index);
        Assert.AreEqual(1, streams[2].Index);

        // Additional assertions for logging
        Assert.AreEqual("2 / fr / aac / 5.1", streams[0].ToString());
        Assert.AreEqual("3 / en / ac3 / 7.1", streams[1].ToString());
        Assert.AreEqual("1 / en / aac / 2.0", streams[2].ToString());
    }
    
    
    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnRegex()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "ac3" },
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "eac3" },
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" },
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new ("Codec", "^ac3$"),
        };

        // Act
        var result = trackSorter.ProcessStreams(args, streams);

        // Assert
        Assert.AreEqual(1, streams[0].Index);
        Assert.AreEqual(3, streams[1].Index);
        Assert.AreEqual(2, streams[2].Index);

        // Additional assertions for logging
        Assert.AreEqual("1 / en / ac3 / 2.0", streams[0].ToString());
        Assert.AreEqual("3 / en / ac3 / 7.1", streams[1].ToString());
        Assert.AreEqual("2 / fr / eac3 / 5.1", streams[2].ToString());
    }
    
    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnMultipleSorters()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac" },
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "aac" },
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" },
            new() { Index = 4, Channels = 2, Language = "fr", Codec = "ac3" },
            new() { Index = 5, Channels = 5.1f, Language = "en", Codec = "ac3" },
            new() { Index = 6, Channels = 7.1f, Language = "fr", Codec = "aac" },
            new() { Index = 7, Channels = 2, Language = "en", Codec = "ac3" },
            new() { Index = 8, Channels = 5.1f, Language = "fr", Codec = "ac3" },
            new() { Index = 9, Channels = 7.1f, Language = "en", Codec = "aac" },
            new() { Index = 10, Channels = 2, Language = "fr", Codec = "aac" },
        };

        // Mock sorters by different properties and custom math operations
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new("Language", "en"),
            new("Channels", ">4.0"),
            new("Codec", "ac3")
        };

        // Act
        var result = trackSorter.ProcessStreams(args, streams);

        // Assert
        
        // en
        Assert.AreEqual(3, streams[0].Index);
        Assert.AreEqual(5, streams[1].Index);
        Assert.AreEqual(9, streams[2].Index);
        Assert.AreEqual(7, streams[3].Index);
        Assert.AreEqual(1, streams[4].Index);
        
        // >5 non en, 2, 6, 8 , 8 first cause ac3
        Assert.AreEqual(8, streams[5].Index);
        Assert.AreEqual(2, streams[6].Index);
        Assert.AreEqual(6, streams[7].Index);
        
        
        Assert.AreEqual(4, streams[8].Index);
        Assert.AreEqual(10, streams[9].Index);


        // Additional assertions for logging
        // Add assertions for the log messages if needed
    }
   
    
    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnBitrate()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac", Stream = new AudioStream() { Bitrate = 140_000_000 }},
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "dts" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 20_000_000 }},
            new() { Index = 4, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new ("Bitrate", "")
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(3, sorted[0].Index);
        Assert.AreEqual(1, sorted[1].Index);
        Assert.AreEqual(2, sorted[2].Index);
        Assert.AreEqual(4, sorted[3].Index);

        // Additional assertions for logging
        Assert.AreEqual(20_000_000, sorted[0].Stream.Bitrate);
        Assert.AreEqual(140_000_000, sorted[1].Stream.Bitrate);
        Assert.AreEqual(200_000_000, sorted[2].Stream.Bitrate);
        Assert.AreEqual(200_000_000, sorted[3].Stream.Bitrate);
    }

    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnChannelsAsc()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac", Stream = new AudioStream() { Bitrate = 140_000_000 }},
            new() { Index = 2, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 20_000_000 }},
            new() { Index = 3, Channels = 5.1f, Language = "fr", Codec = "dts" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
            new() { Index = 4, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new ("Channels", "")
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(1, sorted[0].Index);
        Assert.AreEqual(3, sorted[1].Index);
        Assert.AreEqual(2, sorted[2].Index);
        Assert.AreEqual(4, sorted[3].Index);

        // Additional assertions for logging
        Assert.AreEqual(2.0f, sorted[0].Channels);
        Assert.AreEqual(5.1f, sorted[1].Channels);
        Assert.AreEqual(7.1f, sorted[2].Channels);
        Assert.AreEqual(7.1f, sorted[3].Channels);
    }
    
    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnChannelsDesc()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac", Stream = new AudioStream() { Bitrate = 140_000_000 }},
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "dts" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
            new() { Index = 3, Channels = 7.0999999996f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 20_000_000 }},
            new() { Index = 4, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new ("ChannelsDesc", "")
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(3, sorted[0].Index);
        Assert.AreEqual(4, sorted[1].Index);
        Assert.AreEqual(2, sorted[2].Index);
        Assert.AreEqual(1, sorted[3].Index);

        // Additional assertions for logging
        Assert.AreEqual(7.1f, sorted[0].Channels);
        Assert.AreEqual(7.1f, sorted[1].Channels);
        Assert.AreEqual(5.1f, sorted[2].Channels);
        Assert.AreEqual(2.0f, sorted[3].Channels);
    }

    
    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnBitrateInvert()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac", Stream = new AudioStream() { Bitrate = 140_000_000 }},
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "dts" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 20_000_000 }},
            new() { Index = 4, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new ("BitrateDesc", "")
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(2, sorted[0].Index);
        Assert.AreEqual(4, sorted[1].Index);
        Assert.AreEqual(1, sorted[2].Index);
        Assert.AreEqual(3, sorted[3].Index);

        // Additional assertions for logging
        Assert.AreEqual(200_000_000, sorted[0].Stream.Bitrate);
        Assert.AreEqual(200_000_000, sorted[1].Stream.Bitrate);
        Assert.AreEqual(140_000_000, sorted[2].Stream.Bitrate);
        Assert.AreEqual(20_000_000, sorted[3].Stream.Bitrate);
    }

    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnBitrateAndCodec()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac", Stream = new AudioStream() { Bitrate = 140_000_000 }},
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "dts" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 20_000_000 }},
            new() { Index = 4, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new ("BitrateDesc", ""),
            new ("Codec", "ac3"),
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(4, sorted[0].Index);
        Assert.AreEqual(2, sorted[1].Index);
        Assert.AreEqual(1, sorted[2].Index);
        Assert.AreEqual(3, sorted[3].Index);

        // Additional assertions for logging
        Assert.AreEqual(200_000_000, sorted[0].Stream.Bitrate);
        Assert.AreEqual(200_000_000, sorted[1].Stream.Bitrate);
        Assert.AreEqual(140_000_000, sorted[2].Stream.Bitrate);
        Assert.AreEqual(20_000_000, sorted[3].Stream.Bitrate);
        
        Assert.AreEqual("ac3", sorted[0].Codec);
        Assert.AreEqual("dts", sorted[1].Codec);
        Assert.AreEqual("aac", sorted[2].Codec);
        Assert.AreEqual("ac3", sorted[3].Codec);
    }

    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnBitrateUnit()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "en", Codec = "aac", Stream = new AudioStream() { Bitrate = 140_000_000 }},
            new() { Index = 2, Channels = 5.1f, Language = "fr", Codec = "dts" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
            new() { Index = 3, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 20_000_000 }},
            new() { Index = 4, Channels = 7.1f, Language = "en", Codec = "ac3" , Stream = new AudioStream() { Bitrate = 200_000_000 }},
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new ("Bitrate", ">=150Mbps")
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(2, sorted[0].Index);
        Assert.AreEqual(4, sorted[1].Index);
        Assert.AreEqual(1, sorted[2].Index);
        Assert.AreEqual(3, sorted[3].Index);

        // Additional assertions for logging
        Assert.AreEqual(200_000_000, sorted[0].Stream.Bitrate);
        Assert.AreEqual(200_000_000, sorted[1].Stream.Bitrate);
        Assert.AreEqual(140_000_000, sorted[2].Stream.Bitrate);
        Assert.AreEqual(20_000_000, sorted[3].Stream.Bitrate);
    }
    
    
    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnLanguage()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "eng", Codec = "aac" },
            new() { Index = 2, Channels = 2, Language = "fr", Codec = "aac" },
            new() { Index = 3, Channels = 5.1f, Language = "en", Codec = "eac3" },
            new() { Index = 4, Channels = 5.1f, Language = "ger", Codec = "ac3" },
            new() { Index = 5, Channels = 5.1f, Language = "German", Codec = "dts" },
            new() { Index = 6, Channels = 5.1f, Language = "English", Codec = "aac" },
            new() { Index = 7, Channels = 5.1f, Language = "eng", Codec = "ac3" },
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new("Language", "en")
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(1, sorted[0].Index);
        Assert.AreEqual(3, sorted[1].Index);
        Assert.AreEqual(6, sorted[2].Index);
        Assert.AreEqual(7, sorted[3].Index);
        
        // non english
        Assert.AreEqual(2, sorted[4].Index);
        Assert.AreEqual(4, sorted[5].Index);
        Assert.AreEqual(5, sorted[6].Index);

        // Additional assertions for logging
        Assert.AreEqual("eng", sorted[0].Language);
        Assert.AreEqual("en", sorted[1].Language);
        Assert.AreEqual("English", sorted[2].Language);
        Assert.AreEqual("eng", sorted[3].Language);
        
        Assert.AreEqual("fr", sorted[4].Language);
        Assert.AreEqual("ger", sorted[5].Language);
        Assert.AreEqual("German", sorted[6].Language);
    }
    
    
    
    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnLanguageRegex()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "eng", Codec = "aac" },
            new() { Index = 2, Channels = 2, Language = "fr", Codec = "aac" },
            new() { Index = 3, Channels = 5.1f, Language = "en", Codec = "eac3" },
            new() { Index = 4, Channels = 5.1f, Language = "ger", Codec = "ac3" },
            new() { Index = 5, Channels = 5.1f, Language = "German", Codec = "dts" },
            new() { Index = 6, Channels = 5.1f, Language = "chi", Codec = "aac" },
            new() { Index = 7, Channels = 5.1f, Language = "mri", Codec = "ac3" },
        };

        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new("Language", "en|deu")
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(1, sorted[0].Index);
        Assert.AreEqual(3, sorted[1].Index);
        Assert.AreEqual(4, sorted[2].Index);
        Assert.AreEqual(5, sorted[3].Index);
        
        // non english
        Assert.AreEqual(2, sorted[4].Index);
        Assert.AreEqual(6, sorted[5].Index);
        Assert.AreEqual(7, sorted[6].Index);

        // Additional assertions for logging
        Assert.AreEqual("eng", sorted[0].Language);
        Assert.AreEqual("en", sorted[1].Language);
        Assert.AreEqual("ger", sorted[2].Language);
        Assert.AreEqual("German", sorted[3].Language);
        
        Assert.AreEqual("fr", sorted[4].Language);
        Assert.AreEqual("chi", sorted[5].Language);
        Assert.AreEqual("mri", sorted[6].Language);
    }
    
    
    [TestMethod]
    public void ProcessStreams_SortsStreamsBasedOnLanguageRegexOriginal()
    {
        // Arrange
        var args = new NodeParameters(new TestLogger());
        var trackSorter = new FfmpegBuilderTrackSorter();
        List<FfmpegAudioStream> streams = new List<FfmpegAudioStream>
        {
            new() { Index = 1, Channels = 2, Language = "eng", Codec = "aac" },
            new() { Index = 2, Channels = 2, Language = "fr", Codec = "aac" },
            new() { Index = 3, Channels = 5.1f, Language = "en", Codec = "eac3" },
            new() { Index = 4, Channels = 5.1f, Language = "ger", Codec = "ac3" },
            new() { Index = 5, Channels = 5.1f, Language = "mri", Codec = "ac3" },
            new() { Index = 6, Channels = 5.1f, Language = "chi", Codec = "aac" },
            new() { Index = 7, Channels = 5.1f, Language = "German", Codec = "dts" },
        };

        args.Variables["OriginalLanguage"] = "Maori";
        
        // Mock sorters by different properties
        trackSorter.Sorters = new List<KeyValuePair<string, string>>
        {
            new("Language", "orig|deu")
        };

        // Act
        var sorted = trackSorter.SortStreams(args, streams);

        // Assert
        Assert.AreEqual(4, sorted[0].Index);
        Assert.AreEqual(5, sorted[1].Index);
        Assert.AreEqual(7, sorted[2].Index);
        
        // non english
        Assert.AreEqual(1, sorted[3].Index);
        Assert.AreEqual(2, sorted[4].Index);
        Assert.AreEqual(3, sorted[5].Index);
        Assert.AreEqual(6, sorted[6].Index);

        // Additional assertions for logging
        Assert.AreEqual("ger", sorted[0].Language);
        Assert.AreEqual("mri", sorted[1].Language);
        Assert.AreEqual("German", sorted[2].Language);
        
        Assert.AreEqual("eng", sorted[3].Language);
        Assert.AreEqual("fr", sorted[4].Language);
        Assert.AreEqual("en", sorted[5].Language);
        Assert.AreEqual("chi", sorted[6].Language);
    }
}

#endif