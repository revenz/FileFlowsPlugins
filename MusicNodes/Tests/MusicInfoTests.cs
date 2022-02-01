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
                var info = new MusicInfoHelper(ffmpegExe, logger).Read(file);
                Assert.IsNotNull(info);
            }
        }
    }
}

#endif