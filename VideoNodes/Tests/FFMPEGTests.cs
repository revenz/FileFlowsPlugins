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
    public class FFMPEGTests
    {
        [TestMethod]
        public void FFMPEG_Variables_Test()
        {
            const string file = @"D:\videos\unprocessed\Home and Away - 2022-02-23 18 30 00 - Home And Away.ts";
            var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
            var vii = vi.Read(file);

            FFMPEG node = new();
            //node.OutputFile = file + ".sup";
            var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
            args.GetToolPathActual = (string tool) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";

            args.Variables.Add("SomeVars", "i am batman");
            node.CommandLine = "-i {workingFile} {SomeVars} -o {output}";
            node.Extension = ".mkv";

            var results = node.GetFFMPEGArgs(args, "file");
            Assert.AreEqual("-i", results[0]);
            Assert.AreEqual(args.WorkingFile, results[1]);
            Assert.AreEqual("i", results[2]);
            Assert.AreEqual("am", results[3]);
            Assert.AreEqual("batman", results[4]);
            Assert.AreEqual("-o", results[5]);
            Assert.AreEqual("file", results[6]);

        }
    }
}


#endif