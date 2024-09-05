// #if(DEBUG)
//
// using FileFlows.VideoNodes.FfmpegBuilderNodes;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using VideoNodes.Tests;
// using System.IO;
//
// namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;
//
// [TestClass]
// public class FfmpegBuilder_HdrTests: TestBase
// {
//     [TestMethod]
//     public void Encode()
//     {
//         string file = Path.Combine(TestPath, "HDR10Plus_PA_DTSX.mkv");
//         if (File.Exists(file) == false)
//             throw new FileNotFoundException(file);
//
//         var logger = new TestLogger();
//         string ffmpeg = FfmpegPath;
//         var vi = new VideoInfoHelper(ffmpeg, logger);
//         var vii = vi.Read(file);
//         var args = new NodeParameters(file, logger, false, string.Empty, new LocalFileService());
//         args.GetToolPathActual = (string tool) =>
//         {
//             if (tool.ToLowerInvariant() == "ffprobe")
//                 return "/usr/local/bin/ffprobe";
//             return ffmpeg;
//         };
//         args.TempPath = TempPath;
//         args.Parameters.Add("VideoInfo", vii);
//
//
//         FfmpegBuilderStart ffStart = new();
//         ffStart.PreExecute(args);
//         Assert.AreEqual(1, ffStart.Execute(args));
//
//         FfmpegBuilderHdr10 hdr10 = new();
//         hdr10.PreExecute(args);
//         hdr10.Execute(args);
//         
//         FfmpegBuilderExecutor ffExecutor = new();
//         ffExecutor.PreExecute(args);
//         int result = ffExecutor.Execute(args);
//         string log = logger.ToString();
//         Assert.AreEqual(1, result);
//     }
// }
//
// #endif