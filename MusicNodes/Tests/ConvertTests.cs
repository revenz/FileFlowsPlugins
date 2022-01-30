#if(DEBUG)


namespace FileFlows.MusicNodes.Tests
{
    using FileFlows.MusicNodes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class ConvertTests
    {
        [TestMethod]
        public void Convert_FlacToAac()
        {

            const string file = @"D:\music\unprocessed\01-billy_joel-you_may_be_right.flac";

            ConvertToAAC node = new ();
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\music\temp";
            new MusicFile().Execute(args); // need to read the music info and set it
            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }

        [TestMethod]
        public void Convert_FlacToMp3()
        {

            const string file = @"D:\music\unprocessed\01-billy_joel-you_may_be_right.flac";

            ConvertToMP3 node = new();
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\music\temp";
            new MusicFile().Execute(args); // need to read the music info and set it
            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }
        [TestMethod]
        public void Convert_Mp3ToWAV()
        {

            const string file = @"D:\music\unprocessed\04-billy_joel-scenes_from_an_italian_restaurant-b2125758.mp3";

            ConvertToWAV node = new();
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\music\temp";
            new MusicFile().Execute(args); // need to read the music info and set it
            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }

        [TestMethod]
        public void Convert_Mp3ToOgg()
        {

            const string file = @"D:\music\unprocessed\04-billy_joel-scenes_from_an_italian_restaurant-b2125758.mp3";

            ConvertToOGG node = new();
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\music\temp";
            new MusicFile().Execute(args); // need to read the music info and set it
            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }


        [TestMethod]
        public void Convert_AacToMp3()
        {

            const string file = @"D:\music\temp\37f315a0-4afc-4a72-a0b4-eb7eb681b9b3.aac";

            ConvertToMP3 node = new();
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\music\temp";
            new MusicFile().Execute(args); // need to read the music info and set it
            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }

        [TestMethod]
        public void Convert_Mp3_AlreadyMp3()
        {

            const string file = @"D:\videos\music\13-the_cranberries-why.mp3";

            ConvertAudio node = new();
            node.SkipIfCodecMatches = true;
            node.Codec = "mp3";

            node.Bitrate = 192;
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\music\temp";
            new MusicFile().Execute(args); // need to read the music info and set it
            int output = node.Execute(args);

            Assert.AreEqual(2, output);
        }
    }
}

#endif