#if(DEBUG)

namespace VideoNodes.Tests;

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
[TestCategory("Slow")]
public class AudioExtractorTests : VideoTestBase
{
    [TestMethod]
    public void AudioExtractor_Mp3_Basic()
    {
        var args = GetVideoNodeParameters();

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio");
        node.OutputCodec = "mp3";

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);  
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void AudioExtractor_Mp3_English()
    {
        var args = GetVideoNodeParameters();

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio");
        node.Language = "en";
        node.OutputCodec = "mp3";

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void AudioExtractor_Mp3_Eac3_Fail()
    {
        var args = GetVideoNodeParameters();

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio");
        node.Codec = "eac3";
        node.OutputCodec = "mp3";
        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(2, output);
    }
    
    [TestMethod]
    public void AudioExtractor_Aac_2048k()
    {
        var args = GetVideoNodeParameters();

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio_2048.aac");
        node.OutputCodec = "aac";
        node.OutputBitrate = 2048;

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void AudioExtractor_Aac_128k()
    {
        var args = GetVideoNodeParameters();

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio_128.aac");
        node.OutputCodec = "aac";
        node.OutputBitrate = 128;

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }
}


#endif