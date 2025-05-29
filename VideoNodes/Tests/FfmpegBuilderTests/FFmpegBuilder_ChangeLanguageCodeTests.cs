#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FFmpegBuilder_ChangeLanguageCodeTests : VideoTestBase
{
    private List<KeyValuePair<string, string>> Replacements => new()
    {
        new("eng", "en"),
        new("deu", "de"),
        new("fre", "fr")
    };

    [TestMethod]
    public void ChangeFFmpegStream_VideoStream_ChangesLanguage()
    {
        var streams = new List<VideoStream>
        {
            new() { Language = "fre" },
            new() { Language = "deu" },
            new() { Language = "ita" } // should not change
        };

        var result = FfmpegBuilderChangeLanguageCode
            .ChangeVideoFileStream(streams, Logger, new StringHelper(Logger), Replacements);

        Assert.IsTrue(result);
        Assert.AreEqual("fr", streams[0].Language);
        Assert.AreEqual("de", streams[1].Language);
        Assert.AreEqual("ita", streams[2].Language); // unchanged
    }

    [TestMethod]
    public void ChangeFFmpegStream_AudioStream_ChangesLanguage()
    {
        var streams = new List<AudioStream>
        {
            new() { Language = "fre" },
            new() { Language = "eng" },
        };

        var result = FfmpegBuilderChangeLanguageCode
            .ChangeVideoFileStream(streams, Logger, new StringHelper(Logger), Replacements);

        Assert.IsTrue(result);
        Assert.AreEqual("fr", streams[0].Language);
        Assert.AreEqual("en", streams[1].Language);
    }
    
    
    [TestMethod]
    public void ChangeFFmpegStream_SubtitleStream_ChangesLanguage()
    {
        var streams = new List<SubtitleStream>
        {
            new() { Language = "deu" },
        };

        var result = FfmpegBuilderChangeLanguageCode
            .ChangeVideoFileStream(streams, Logger, new StringHelper(Logger), Replacements);

        Assert.IsTrue(result);
        Assert.AreEqual("de", streams[0].Language);
    }

    [TestMethod]
    public void ChangeFFmpegStream_FfmpegVideoStream_ChangesLanguage()
    {
        var streams = new List<FfmpegVideoStream>
        {
            new() { Language = "eng" },
        };

        var result = FfmpegBuilderChangeLanguageCode
            .ChangeFFmpegStream(streams, Logger, new StringHelper(Logger), Replacements);

        Assert.IsTrue(result);
        Assert.AreEqual("en", streams[0].Language);
    }

    [TestMethod]
    public void ChangeFFmpegStream_FfmpegAudioStream_ChangesLanguage()
    {
        var streams = new List<FfmpegAudioStream>
        {
            new() { Language = "fre" }
        };

        var result = FfmpegBuilderChangeLanguageCode
            .ChangeFFmpegStream(streams, Logger, new StringHelper(Logger), Replacements);

        Assert.IsTrue(result);
        Assert.AreEqual("fr", streams[0].Language);
    }

    [TestMethod]
    public void ChangeFFmpegStream_FfmpegSubtitleStream_NoMatch()
    {
        var streams = new List<FfmpegSubtitleStream>
        {
            new() { Language = "jpn" } // no match
        };

        var result = FfmpegBuilderChangeLanguageCode
            .ChangeFFmpegStream(streams, Logger, new StringHelper(Logger), Replacements);

        Assert.IsFalse(result);
        Assert.AreEqual("jpn", streams[0].Language);
    }
    
    [TestMethod]
    public void ChangeFFmpegStream_ChineseSimplifiedToTraditional()
    {
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("cn", "zht")
        };

        var streams = new List<FfmpegSubtitleStream>
        {
            new() { Language = "cn" },
            new() { Language = "en" } // should remain unchanged
        };

        var result = FfmpegBuilderChangeLanguageCode
            .ChangeFFmpegStream(streams, Logger, new StringHelper(Logger), replacements);

        Assert.IsTrue(result);
        Assert.AreEqual("zht", streams[0].Language);
        Assert.AreEqual("en", streams[1].Language);
    }
}

#endif