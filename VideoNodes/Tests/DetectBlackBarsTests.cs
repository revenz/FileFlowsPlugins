#if(DEBUG)

namespace VideoNodes.Tests
{
    using FileFlows.VideoNodes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class DetectBlackBarsTests
    {
        [TestMethod]
        public void DetectBlackBars_Test_01()
        {
            const string file = @"D:\videos\unprocessed\Fast Five (2011) Bluray-2160p.mkv";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);

            DetectBlackBars node = new();
            //node.OutputFile = file + ".sup";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPath = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";

            new VideoFile().Execute(args);

            int output = node.Execute(args);

            string crop = args.Parameters[DetectBlackBars.CROP_KEY] as string;
            Assert.IsFalse(string.IsNullOrWhiteSpace(crop));

            Assert.AreEqual(1, output);
        }
    }
}


#endif