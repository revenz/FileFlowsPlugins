// #if(DEBUG)
//
// using FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;
// using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
//
// namespace VideoNodes.Tests;
//
// [TestClass]
// public class VaapiAdjustmentTests: VideoTestBase
// {
//     [TestMethod]
//     public void Vaapi_CropAndScale()
//     {
//         var inputParameters = new List<string>
//         {
//             "-fflags",
//             "+genpts",
//             "-probesize",
//             "5M",
//             "-i",
//             "/media/test.mkv",
//             "-y",
//             "-movflags",
//             "+faststart",
//             "-map_metadata",
//             "-1",
//             "-map",
//             "0:v:0",
//             "-c:v:0",
//             "h264_vaapi",
//             "-qp",
//             "23",
//             "-preset",
//             "slower",
//             "-spatial-aq",
//             "1",
//             "-filter:v:0",
//             "crop=1904:800:8:2, scale=1280:-2:flags=lanczos",
//             "-metadata:s:v:0",
//             "title=",
//             "-map",
//             "0:a:0",
//             "-c:a:0",
//             "aac",
//             "-ac:a:0",
//             "2",
//             "-b:a:0",
//             "160k",
//             "-metadata:s:a:0",
//             "BPS=160000",
//             "-ar:a:0",
//             "48000",
//             "-filter:a:0",
//             "loudnorm=print_format=summary:linear=true:I=-24:LRA=7:TP=-2.0:measured_I=-18.14:measured_LRA=5.20:measured_tp=-4.31:measured_thresh=-28.14:offset=0.56, volume=.7",
//             "-metadata:s:a:0",
//             "title=Stereo",
//             "-metadata:s:a:0",
//             "language=eng"
//         };
//
//         var updated = new VaapiAdjustments().Run(Logger, null!, inputParameters);
//         for (int i = 0; i < updated.Count; i++)
//             Logger.ILog($"Updated[{i:00}] = {updated[i]}");
//         
//         Assert.AreEqual("-vf", updated[9]);
//         Assert.AreEqual("format=nv12,hwupload,scale_vaapi=1280:-2", updated[10]);
//         int index = updated.FindIndex(x => x.StartsWith("-filter:v"));
//         Assert.IsTrue(index > 0);
//         Assert.AreEqual("crop=1904:800:8:2", updated[index + 1]);
//     }
//     
//     
//     [TestMethod]
//     public void Vaapi_Scale()
//     {
//         var inputParameters = new List<string>
//         {
//             "-fflags",
//             "+genpts",
//             "-probesize",
//             "5M",
//             "-i",
//             "/media/test.mkv",
//             "-y",
//             "-movflags",
//             "+faststart",
//             "-map_metadata",
//             "-1",
//             "-map",
//             "0:v:0",
//             "-c:v:0",
//             "h264_vaapi",
//             "-qp",
//             "23",
//             "-preset",
//             "slower",
//             "-spatial-aq",
//             "1",
//             "-filter:v:0",
//             "scale=1280:-2:flags=lanczos",
//             "-metadata:s:v:0",
//             "title=",
//             "-map",
//             "0:a:0",
//             "-c:a:0",
//             "aac",
//             "-ac:a:0",
//             "2",
//             "-b:a:0",
//             "160k",
//             "-metadata:s:a:0",
//             "BPS=160000",
//             "-ar:a:0",
//             "48000",
//             "-filter:a:0",
//             "loudnorm=print_format=summary:linear=true:I=-24:LRA=7:TP=-2.0:measured_I=-18.14:measured_LRA=5.20:measured_tp=-4.31:measured_thresh=-28.14:offset=0.56, volume=.7",
//             "-metadata:s:a:0",
//             "title=Stereo",
//             "-metadata:s:a:0",
//             "language=eng"
//         };
//
//         var updated = new VaapiAdjustments().Run(Logger, null!, inputParameters);
//         for (int i = 0; i < updated.Count; i++)
//             Logger.ILog($"Updated[{i:00}] = {updated[i]}");
//         
//         Assert.AreEqual("-vf", updated[9]);
//         Assert.AreEqual("format=nv12,hwupload,scale_vaapi=1280:-2", updated[10]);
//         Assert.IsFalse(updated.Any(x => x.StartsWith("-filter:v")));
//     }
// }
// #endif