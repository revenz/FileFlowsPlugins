#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

/// <summary>
/// Tests for track reorders
/// </summary>
[TestClass]
public class FFmpegBuilder_SetTrackTtitlesTests
{
    [TestMethod]
    public void FormatTitle_DefaultCase_Success()
    {
        // Arrange
        string formatter = "Track: lang / codec / channels / default / bitrate / samplerate / cc / sdh / hi";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 44100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English / AAC / Stereo / Default / 128Kbps / 44.1kHz", result);
    }

    [TestMethod]
    public void FormatTitle_DefaultCase_Success_Exclaim()
    {
        // Arrange
        string formatter = "Track: lang / !codec / channels / default / bitrate / samplerate / cc / sdh / hi";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 44100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English / aac / Stereo / Default / 128Kbps / 44.1kHz", result);
    }
    
    [TestMethod]
    public void FormatTitle_SDH()
    {
        // Arrange
        string formatter = "Track: lang / codec / channels / default / bitrate / samplerate / cc / sdh / hi";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 44100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, 
            channels, sampleRate, isForced, sdh: true, cc: true);

        // Assert
        Assert.AreEqual("Track: English / AAC / Stereo / Default / 128Kbps / 44.1kHz / CC / SDH", result);
    }
    

    [TestMethod]
    public void FormatTitle_Codec_CommericalName()
    {
        // Arrange
        string formatter = "Track: lang / codec-cc / channels / default / bitrate / samplerate / cc / sdh / hi";
        string separator = " / ";
        string language = "English";
        string codec = "DTS";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 44100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, 
            channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English / Digital Theater Systems / Stereo / Default / 128Kbps / 44.1kHz", result);
    }
    
    [TestMethod]
    public void FormatTitle_EmptyFormatter_ReturnsEmptyString()
    {
        // Arrange
        string formatter = "";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 44100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void FormatTitle_NullFormatter_ReturnsEmptyString()
    {
        // Arrange
        string formatter = null;
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 44100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void FormatTitle_MonoChannel_Success()
    {
        // Arrange
        string formatter = "Track: lang / channels / codec";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 1.0f; // Mono
        int sampleRate = 44_100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English / Mono / AAC", result);
    }

    [TestMethod]
    public void FormatTitle_StereoChannel_Success()
    {
        // Arrange
        string formatter = "Track: lang / channels / codec";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f; // Stereo
        int sampleRate = 44_100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English / Stereo / AAC", result);
    }

    [TestMethod]
    public void FormatTitle_ZeroBitrateAndSampleRate_Success()
    {
        // Arrange
        string formatter = "Track: lang / codec / bitrate / samplerate";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 0;
        float channels = 2.0f;
        int sampleRate = 0;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English / AAC", result);
    }

    [TestMethod]
    public void FormatTitle_EmptyValues_Success()
    {
        // Arrange
        string formatter = "Track: lang / codec / default / forced / bitrate / samplerate";
        string separator = " / ";
        string language = string.Empty;
        string codec = string.Empty;
        bool isDefault = false;
        float bitrate = 0;
        float channels = 2.0f;
        int sampleRate = 0;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track:", result);
    }

    [TestMethod]
    public void FormatTitle_MissingSampleRateAndForced_Success()
    {
        // Arrange
        string formatter = "Track: lang / codec / default / bitrate";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 0; // Missing sample rate
        bool isForced = false; // Missing forced

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English / AAC / Default / 128Kbps", result);
    }

    [TestMethod]
    public void FormatTitle_AllValuesSet_Success()
    {
        // Arrange
        string formatter = "Track: lang / codec / default / forced / bitrate / channels / sample-rate";
        string separator = " / ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 44_100;
        bool isForced = true;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English / AAC / Default / Forced / 128Kbps / Stereo / 44.1kHz", result);
    }
    [TestMethod]
    public void FormatTitle_OnlyForcedSet_Success()
    {
        // Arrange
        string formatter = "lang / codec / default / forced / bitrate / channels / sample-rate";
        string separator = " / ";
        string language = string.Empty; // Default value
        string codec = string.Empty; // Default value
        bool isDefault = false; // Default value
        float bitrate = 0; // Default value
        float channels = 0; // Default value
        int sampleRate = 0; // Default value
        bool isForced = true; // Only Forced is set to true

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Forced", result);
    }
    
    [TestMethod]
    public void FormatTitle_SeparatorDash_Success()
    {
        // Arrange
        string formatter = "Track: lang-codec-default-forced-bitrate-channels-sample-rate";
        string separator = "-";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 2.0f;
        int sampleRate = 44_100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English-AAC-Default-128Kbps-Stereo-44.1kHz", result);
    }

    [TestMethod]
    public void FormatTitle_SeparatorBackslash_Success()
    {
        // Arrange
        string formatter = "Track: lang\\codec\\default\\forced\\bitrate\\channels\\samplerate";
        string separator = "\\";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 1.000445f;
        int sampleRate = 44_100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English\\AAC\\Default\\128Kbps\\Mono\\44.1kHz", result);
    }

    [TestMethod]
    public void FormatTitle_SeparatorSpace_Success()
    {
        // Arrange
        string formatter = "Track: lang codec default forced bitrate channels samplerate";
        string separator = " ";
        string language = "English";
        string codec = "AAC";
        bool isDefault = true;
        float bitrate = 128_000;
        float channels = 5.0999996f;
        int sampleRate = 44_100;
        bool isForced = false;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, isDefault, bitrate, channels, sampleRate, isForced);

        // Assert
        Assert.AreEqual("Track: English AAC Default 128Kbps 5.1 44.1kHz", result);
    }


    
    [TestMethod]
    public void FormatTitle_Video_Resolution()
    {
        // Arrange
        string formatter = "Track: lang / codec / fps / resolution / dimensions / pixelformat";
        string separator = " / ";
        string language = "English";
        string codec = "HEVC";

        ResolutionHelper.Resolution resolution = ResolutionHelper.Resolution.r720p;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, 
            resolution: resolution);

        // Assert
        Assert.AreEqual("Track: English / HEVC / 720P", result);
        
        // Act
        formatter = "Track: lang / codec / fps / !resolution / dimensions / pixelformat";
        result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, 
            resolution: resolution);

        // Assert
        Assert.AreEqual("Track: English / HEVC / 720p", result);
    }
    
    [TestMethod]
    public void FormatTitle_Video_PixelFormat()
    {
        // Arrange
        string formatter = "Track: lang / codec / fps / resolution / dimensions / pixelformat";
        string separator = " / ";
        string language = "English";
        string codec = "HEVC";

        string pixelFormat = "Nv12";

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, 
            pixelFormat: pixelFormat);

        // Assert
        Assert.AreEqual("Track: English / HEVC / NV12", result);
        
        
        // Act
        formatter = "Track: lang / codec / fps / resolution / dimensions / !pixelformat";
        result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, 
            pixelFormat: pixelFormat);

        // Assert
        Assert.AreEqual("Track: English / HEVC / nv12", result);
        
        // Act
        formatter = "Track: lang / codec / fps / resolution / dimensions / !pixelformat!";
        result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, 
            pixelFormat: pixelFormat);

        // Assert
        Assert.AreEqual("Track: English / HEVC / Nv12", result);
    }
    
    [TestMethod]
    public void FormatTitle_Video_Dimensions()
    {
        // Arrange
        string formatter = "Track: lang / codec / fps / resolution / dimensions / pixelformat";
        string separator = " / ";
        string language = "English";
        string codec = "HEVC";

        string dimensions = "1920x1080";

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, 
            dimensions: dimensions);

        // Assert
        Assert.AreEqual("Track: English / HEVC / 1920x1080", result);
    }
    
    [TestMethod]
    public void FormatTitle_Video_Fps()
    {
        // Arrange
        string formatter = "Track: lang / codec / fps / resolution / dimensions / pixelformat";
        string separator = " / ";
        string language = "English";
        string codec = "HEVC";

        float fps = 23.9999997f;

        // Act
        string result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, 
            fps: fps);

        // Assert
        Assert.AreEqual("Track: English / HEVC / 24FPS", result);

        // Act
        fps = 23.9999997f;
        formatter = "Track: lang / codec / !fps / resolution / dimensions / pixelformat";
        result = FfmpegBuilderSetTrackTitles.FormatTitle(formatter, separator, language, codec, 
            fps: fps);

        // Assert
        Assert.AreEqual("Track: English / HEVC / 24fps", result);
    }
}

#endif