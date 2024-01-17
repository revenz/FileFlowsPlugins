// #if(DEBUG)
//
// namespace VideoNodes.Tests;
//
// using FileFlows.VideoNodes;
// using FileFlows.VideoNodes.FfmpegBuilderNodes;
// using FileFlows.VideoNodes.VideoNodes;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
//
// [TestClass]
// public class EndCreditsTests : TestBase
// {
//     [TestMethod]
//     public void EndCredits_Base()
//     {
//         var logger = new TestLogger();
//         string file = @"D:\videos\testfiles\pgs.mkv";
//         var vi = new VideoInfoHelper(FfmpegPath, logger);
//         var vii = vi.Read(file);
//
//         var args = new NodeParameters(file, logger, false, string.Empty, null);
//         args.GetToolPathActual = (string tool) => FfmpegPath;
//         args.TempPath = TempPath;
//
//         var node = new DetectEndCredits();
//         node.PreExecute(args);
//         var result = node.Execute(args);
//         var log = logger.ToString();
//         Assert.AreEqual(1, result);
//     }
// }
//
// #endif