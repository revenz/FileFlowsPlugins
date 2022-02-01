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
    public class MusicInfoTests
    {
        [TestMethod]
        public void MusicInfo_SplitTrack()
        {

            const string file = @"\\oracle\music\The Cranberries\No Need To Argue\The Cranberries - No Need To Argue - 00 - I Don't Need (Demo).mp3";
            const string ffmpegExe = @"C:\utils\ffmpeg\ffmpeg.exe";

            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpegExe;
            args.TempPath = @"D:\music\temp";

            var musicInfo = new MusicInfoHelper(ffmpegExe, args.Logger).Read(args.WorkingFile);

            Assert.AreEqual(9, musicInfo.Track);
        }

        [TestMethod]
        public void MusicInfo_NormalTrack()
        {

            const string file = @"\\oracle\music\Taylor Swift\Speak Now\Taylor Swift - Speak Now - 08 - Never Grow Up.mp3";
            const string ffmpegExe = @"C:\utils\ffmpeg\ffmpeg.exe";

            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => ffmpegExe;
            args.TempPath = @"D:\music\temp";

            var musicInfo = new MusicInfoHelper(ffmpegExe, args.Logger).Read(args.WorkingFile);

            Assert.AreEqual(8, musicInfo.Track);
        }

        [TestMethod]
        public void MusicInfo_GetMetaData()
        {
            const string ffmpegExe = @"C:\utils\ffmpeg\ffmpeg.exe";
            var logger = new TestLogger();
            foreach (string file in Directory.GetFiles(@"D:\videos\music"))
            {
                var args = new FileFlows.Plugin.NodeParameters(file, logger, false, string.Empty);
                args.GetToolPathActual = (string tool) => ffmpegExe;

                // laod the variables
                Assert.AreEqual(1, new MusicFile().Execute(args));

                var mi = new MusicInfoHelper(ffmpegExe, args.Logger).Read(args.WorkingFile);

                string folder = args.ReplaceVariables("{mi.ArtistThe} ({mi.Year})");
                Assert.AreEqual($"{mi.Artist} ({mi.Date.Year})", folder);

                string fname = args.ReplaceVariables("{mi.Artist} - {mi.Album} - {mi.Track:##} - {mi.Title}");
                Assert.AreEqual($"{mi.Artist} - {mi.Track.ToString("00")} - {mi.Title}", fname);
            }
        }

        [TestMethod]
        public void MusicInfo_FileNameMetadata()
        {
            const string ffmpegExe = @"C:\utils\ffmpeg\ffmpeg.exe";
            var logger = new TestLogger();
            string file = @"\\jor-el\music\Meat Loaf\Bat out of Hell II- Back Into Hell… (1993)\Meat Loaf - Bat out of Hell II- Back Into Hell… - 03 - I’d Do Anything for Love (but I Won’t Do That).flac";
            
            var mi = new MusicInfo();

            new MusicInfoHelper(ffmpegExe, logger).ParseFileNameInfo(file, mi);

            Assert.AreEqual("Meat Loaf", mi.Artist);
            Assert.AreEqual("Bat out of Hell II- Back Into Hell…", mi.Album);
            Assert.AreEqual(1993, mi.Date.Year);
            Assert.AreEqual("I’d Do Anything for Love (but I Won’t Do That)", mi.Title);
            Assert.AreEqual(3, mi.Track);
        }
    }
}

#endif