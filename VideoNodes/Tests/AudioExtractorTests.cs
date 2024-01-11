#if(DEBUG)

namespace VideoNodes.Tests;

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class AudioExtractorTests : TestBase
{
    [TestMethod]
    public void AudioExtractor_Mp3_Basic()
    {
        var logger = new TestLogger();
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio");
        node.OutputCodec = "mp3";

        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);  
        int output = node.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void AudioExtractor_Mp3_English()
    {
        var logger = new TestLogger();
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio");
        node.Language = "en";
        node.OutputCodec = "mp3";

        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void AudioExtractor_Mp3_Eac3_Fail()
    {
        var logger = new TestLogger();
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio");
        node.Codec = "eac3";
        node.OutputCodec = "mp3";

        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(2, output);
    }
    [TestMethod]
    public void AudioExtractor_Mp3_Eac3_Pass()
    {
        var logger = new TestLogger();
        string file = TestFile_Pgs;
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio_eac3");
        node.Codec = "eac3";
        node.OutputCodec = "mp3";

        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void AudioExtractor_Aac_2048k()
    {
        var logger = new TestLogger();
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio_2048.aac");
        node.OutputCodec = "aac";
        node.OutputBitrate = 2048;

        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void AudioExtractor_Aac_128k()
    {
        var logger = new TestLogger();
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);

        VideoExtractAudio node = new();
        node.OutputFile = Path.Combine(TempPath, "Audio_128.aac");
        node.OutputCodec = "aac";
        node.OutputBitrate = 128;

        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vidFile = new VideoFile();
        vidFile.PreExecute(args);
        Assert.AreEqual(1, vidFile.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(1, output);
    }
}


#endif