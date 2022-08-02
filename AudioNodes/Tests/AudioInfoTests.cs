#if(DEBUG)


namespace FileFlows.AudioNodes.Tests
{
    using FileFlows.AudioNodes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class AudioInfoTests
    {
        [TestMethod]
        public void AudioInfo_SplitTrack()
        {

            const string file = @"\\oracle\Audio\The Cranberries\No Need To Argue\The Cranberries - No Need To Argue - 00 - I Don't Need (Demo).mp3";
            const string ffmpegExe = @"C:\utils\ffmpeg\ffmpeg.exe";

            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpegExe;
            args.TempPath = @"D:\music\temp";

            var AudioInfo = new AudioInfoHelper(ffmpegExe, args.Logger).Read(args.WorkingFile);

            Assert.AreEqual(9, AudioInfo.Track);
        }

        [TestMethod]
        public void AudioInfo_NormalTrack()
        {

            const string file = @"\\oracle\Audio\Taylor Swift\Speak Now\Taylor Swift - Speak Now - 08 - Never Grow Up.mp3";
            const string ffmpegExe = @"C:\utils\ffmpeg\ffmpeg.exe";

            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpegExe;
            args.TempPath = @"D:\music\temp";

            var AudioInfo = new AudioInfoHelper(ffmpegExe, args.Logger).Read(args.WorkingFile);

            Assert.AreEqual(8, AudioInfo.Track);
        }

        [TestMethod]
        public void AudioInfo_GetMetaData()
        {
            const string ffmpegExe = @"C:\utils\ffmpeg\ffmpeg.exe";
            var logger = new TestLogger();
            foreach (string file in Directory.GetFiles(@"D:\videos\Audio"))
            {
                var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty);
                args.GetToolPathActual = (string tool) => ffmpegExe;

                // laod the variables
                Assert.AreEqual(1, new AudioFile().Execute(args));

                var audio = new AudioInfoHelper(ffmpegExe, args.Logger).Read(args.WorkingFile);

                string folder = args.ReplaceVariables("{audio.ArtistThe} ({audio.Year})");
                Assert.AreEqual($"{audio.Artist} ({audio.Date.Year})", folder);

                string fname = args.ReplaceVariables("{audio.Artist} - {audio.Album} - {audio.Track:##} - {audio.Title}");
                Assert.AreEqual($"{audio.Artist} - {audio.Track.ToString("00")} - {audio.Title}", fname);
            }
        }

        [TestMethod]
        public void AudioInfo_FileNameMetadata()
        {
            const string ffmpegExe = @"C:\utils\ffmpeg\ffmpeg.exe";
            var logger = new TestLogger();
            string file = @"\\jor-el\Audio\Meat Loaf\Bat out of Hell II- Back Into Hell… (1993)\Meat Loaf - Bat out of Hell II- Back Into Hell… - 03 - I’d Do Anything for Love (but I Won’t Do That).flac";
            
            var audio = new AudioInfo();

            new AudioInfoHelper(ffmpegExe, logger).ParseFileNameInfo(file, audio);

            Assert.AreEqual("Meat Loaf", audio.Artist);
            Assert.AreEqual("Bat out of Hell II- Back Into Hell…", audio.Album);
            Assert.AreEqual(1993, audio.Date.Year);
            Assert.AreEqual("I’d Do Anything for Love (but I Won’t Do That)", audio.Title);
            Assert.AreEqual(3, audio.Track);
        }
    }
}

#endif