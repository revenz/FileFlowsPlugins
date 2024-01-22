#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using AudioNodes.Tests;

namespace FileFlows.AudioNodes.Tests;

[TestClass]
public class ConvertTests
{
    [TestMethod]
    public void Convert_FlacToAac()
    {
        //const string file = @"/home/john/Music/unprocessed/Aqua - Aquarium - 03 - Barbie Girl.flac";
        const string file = "/home/john/Music/unprocessed/Christina Perri - Lovestrong. (2011) - 04 - Distance.mp3";

        foreach (var codec in new[] { "MP3", "aac", "ogg"})
        {
            foreach (int quality in new[] { 0, 10 })
            {
                var logger = new TestLogger();
                ConvertAudio node = new();
                node.Codec = codec;
                node.Bitrate = quality + 10;
                node.HighEfficiency = true;
                var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty, new LocalFileService());
                args.GetToolPathActual = (string tool) =>
                {
                    if(tool.ToLowerInvariant().Contains("ffmpeg")) return @"/usr/local/bin/ffmpeg";
                    if(tool.ToLowerInvariant().Contains("ffprobe")) return @"/usr/local/bin/ffprobe";
                    return tool;
                };
                args.TempPath = @"/home/john/temp";
                var af = new AudioFile();
                Assert.IsTrue(af.PreExecute(args));
                af.Execute(args); // need to read the Audio info and set it
                Assert.IsTrue(node.PreExecute(args));
                int output = node.Execute(args);

                var log = logger.ToString();
                Assert.AreEqual(1, output);
                var fi = new FileInfo(args.WorkingFile);
                File.Move(args.WorkingFile, Path.Combine(fi.DirectoryName, quality + fi.Extension), true);
            }
        }
    }

    [TestMethod]
    public void Convert_FlacToMp3()
    {

        const string file = @"D:\music\unprocessed\01-billy_joel-you_may_be_right.flac";

        ConvertToMP3 node = new();
        var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        new AudioFile().Execute(args); // need to read the Audio info and set it
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }
    [TestMethod]
    public void Convert_Mp3ToWAV()
    {

        const string file = @"D:\music\unprocessed\04-billy_joel-scenes_from_an_italian_restaurant-b2125758.mp3";

        ConvertToWAV node = new();
        var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        new AudioFile().Execute(args); // need to read the Audio info and set it
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void Convert_Mp3ToOgg()
    {

        const string file = @"D:\music\unprocessed\04-billy_joel-scenes_from_an_italian_restaurant-b2125758.mp3";

        ConvertToOGG node = new();
        var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        new AudioFile().Execute(args); // need to read the Audio info and set it
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void Convert_AacToMp3()
    {

        const string file = @"D:\music\temp\37f315a0-4afc-4a72-a0b4-eb7eb681b9b3.aac";

        ConvertToMP3 node = new();
        var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        new AudioFile().Execute(args); // need to read the Audio info and set it
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void Convert_Mp3_AlreadyMp3()
    {

        const string file = @"D:\videos\Audio\13-the_cranberries-why.mp3";

        ConvertAudio node = new();
        node.SkipIfCodecMatches = true;
        node.Codec = "mp3";

        node.Bitrate = 192;
        var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        new AudioFile().Execute(args); // need to read the Audio info and set it
        int output = node.Execute(args);

        Assert.AreEqual(2, output);
    }

    [TestMethod]
    public void Convert_VideoToMp3()
    {

        const string file = @"D:\videos\testfiles\basic.mkv";

        ConvertToMP3 node = new();
        var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        //new AudioFile().Execute(args); // need to read the Audio info and set it
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void Convert_VideoToAac()
    {

        const string file = @"D:\videos\testfiles\basic.mkv";

        ConvertToAAC node = new();
        var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        //new AudioFile().Execute(args); // need to read the Audio info and set it
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void Convert_TwoPass()
    {

        const string file = @"D:\music\flacs\01-billy_joel-you_may_be_right.flac";

        ConvertToAAC node = new();
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        new AudioFile().Execute(args); // need to read the Audio info and set it
        node.Normalize = true;
        int output = node.Execute(args);

        string log = logger.ToString();

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void Convert_TwoPass_VideoFile()
    {

        const string file = @"D:\videos\testfiles\basic.mkv";

        ConvertToAAC node = new();
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        new AudioFile().Execute(args); // need to read the Audio info and set it
        node.Normalize = true;
        int output = node.Execute(args);

        string log = logger.ToString();

        Assert.AreEqual(1, output);
    }
}

#endif