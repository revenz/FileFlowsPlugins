// #if(DEBUG)
//
// using FileFlows.VideoNodes.FfmpegBuilderNodes;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using VideoNodes.Tests;
//
// namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;
//
// /// <summary>
// /// Tests the subtitle burning in
// /// </summary>
// [TestClass]
// public class FFmpegBuilder_SubtitleBurnInTests : VideoTestBase
// {
//     NodeParameters args;
//
//     /// <summary>
//     /// Sets up the test environment before each test.
//     /// Initializes video parameters and executes the video file setup.
//     /// </summary>
//     private void InitVideo(string file)
//     {
//         args = GetVideoNodeParameters(file);
//         VideoFile vf = new VideoFile();
//         vf.PreExecute(args);
//         vf.Execute(args);
//
//         FfmpegBuilderStart ffStart = new();
//         ffStart.PreExecute(args);
//         Assert.AreEqual(1, ffStart.Execute(args));
//     }
//
//     /// <summary>
//     /// Burn In
//     /// </summary>
//     [TestMethod]
//     public void BurnIn()
//     {
//         InitVideo(VideoSubtitles);
//
//         var ffSubtitleBurnIn = new FfmpegBuilderSubtitleBurnIn()
//         {
//             CustomTrackSelection = true,
//             TrackSelectionOptions = new()
//             {
//                 new("Language", "English")
//             }
//         };
//
//         ffSubtitleBurnIn.PreExecute(args);
//         ffSubtitleBurnIn.Execute(args);
//
//         var ffRemoveSubtitles = new FfmpegBuilderAudioTrackRemover();
//         ffRemoveSubtitles.RemoveAll = true;
//         ffRemoveSubtitles.StreamType = "Subtitle";
//         ffRemoveSubtitles.PreExecute(args);
//         ffRemoveSubtitles.Execute(args);
//
//         var ffExecutor = new FfmpegBuilderExecutor();
//         ffExecutor.PreExecute(args);
//         int result = ffExecutor.Execute(args);
//
//         Assert.AreEqual(1, result);
//     }
// }
//
// #endif