#if(DEBUG)


namespace FileFlows.MusicNodes.Tests;

using FileFlows.MusicNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TestClass]
public class AudioFileNormalizationTests
{
    [TestMethod]
    public void AudioFileNormalization_Mp3()
    {

        const string file = @"D:\music\unprocessed\01-billy_joel-movin_out.mp3";

        AudioFileNormalization node = new ();
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";
        new MusicFile().Execute(args); // need to read the music info and set it
        int output = node.Execute(args);

        string log = logger.ToString();

        Assert.AreEqual(1, output);
    }
    [TestMethod]
    public void AudioFileNormalization_Bulk()
    {

        foreach (var file in Directory.GetFiles(@"d:\music\unprocessed"))
        {

            AudioFileNormalization node = new();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\music\temp";
            new MusicFile().Execute(args); // need to read the music info and set it
            int output = node.Execute(args);

            string log = logger.ToString();

            Assert.AreEqual(1, output);
        }
    }


    [TestMethod]
    public void AudioFileNormalization_ConvertFlacToMp3()
    {

        const string file = @"D:\music\flacs\03-billy_joel-dont_ask_me_why.flac";
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
        args.TempPath = @"D:\music\temp";

        new MusicFile().Execute(args); // need to read the music info and set it

        ConvertToMP3 convertNode = new();
        int output = convertNode.Execute(args);


        AudioFileNormalization normalNode = new();
        output = normalNode.Execute(args);

        string log = logger.ToString();

        Assert.AreEqual(1, output);
    }
}

#endif