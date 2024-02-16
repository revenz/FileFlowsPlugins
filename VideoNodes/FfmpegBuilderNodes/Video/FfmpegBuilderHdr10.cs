// using System.Text.Json;
// using System.Text.Json.Serialization;
//
// namespace FileFlows.VideoNodes.FfmpegBuilderNodes;
//
// /// <summary>
// /// FFmpeg Builder flow element that encodes a video using HDR 10+
// /// </summary>
// public class FfmpegBuilderHdr10 : FfmpegBuilderNode
// {
//     /// <inheritdoc />
//     public override int Inputs => 1;
//
//     /// <inheritdoc />
//     public override int Outputs => 2;
//
//     /// <inheritdoc />
//     public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/hdr-10";
//     
//     /// <summary>
//     /// Gets or sets the quality of the video encode
//     /// </summary>
//     [Slider(1, inverse: true)]
//     [Range(0, 51)]
//     [DefaultValue(28)]
//     public int Quality { get; set; }
//     
//     /// <summary>
//     /// Gets or sets the speed to encode
//     /// </summary>
//     [Select(nameof(SpeedOptions), 2)]
//     public string Speed { get; set; }
//
//     private static List<ListOption> _SpeedOptions;
//     /// <summary>
//     /// Gets or sets the codec options
//     /// </summary>
//     public static List<ListOption> SpeedOptions
//     {
//         get
//         {
//             if (_SpeedOptions == null)
//             {
//                 _SpeedOptions = new List<ListOption>
//                 {
//                     new () { Label = "Very Slow", Value = "veryslow" },
//                     new () { Label = "Slower", Value = "slower" },
//                     new () { Label = "Slow", Value = "slow" },
//                     new () { Label = "Medium", Value = "medium" },
//                     new () { Label = "Fast", Value = "fast" },
//                     new () { Label = "Faster", Value = "faster" },
//                     new () { Label = "Very Fast", Value = "veryfast" },
//                     new () { Label = "Super Fast", Value = "superfast" },
//                     new () { Label = "Ultra Fast", Value = "ultrafast" },
//                 };
//             }
//             return _SpeedOptions;
//         }
//     }
//
//     /// <inheritdoc />
//     public override int Execute(NodeParameters args)
//     {
//         var ffprobeResult = GetFFprobe(args);
//         if (ffprobeResult.Failed(out string ffprobeError))
//         {
//             args.FailureReason = ffprobeError;
//             args.Logger?.ELog(ffprobeError);
//             return -1;
//         }
//
//         var model = GetModel();
//         var video = model.VideoStreams.FirstOrDefault();
//         if (video == null)
//         {
//             args.Logger?.WLog("No video stream found in FFmpeg Builder");
//             return 2;
//         }
//
//         string ffprobe = ffprobeResult.Value;
//         var sideDataResult = GetColorData(args, ffprobe, args.WorkingFile);
//         if (sideDataResult.Failed(out string error))
//         {
//             args.Logger?.ILog("Failed ot get HDR10 info: " + error);
//             return 2;
//         }
//
//         var sd = sideDataResult.Value;
//         string gx = sd.GreenX.Split('/')[0];
//         string gy = sd.GreenY.Split('/')[0];
//         string bx = sd.BlueX.Split('/')[0];
//         string by = sd.BlueY.Split('/')[0];
//         string rx = sd.RedX.Split('/')[0];
//         string ry = sd.RedY.Split('/')[0];
//         string wpx = sd.WhitePointX.Split('/')[0];
//         string wpy = sd.WhitePointY.Split('/')[0];
//         string minLum = sd.MinLuminance.Split('/')[0];
//         string maxLum = sd.MaxLuminance.Split('/')[0];
//         string display = $@"G({gx},{gy})B({bx},{by})R({rx},{ry})WP({wpx},{wpy})L({maxLum},{minLum})";
//         
//         args.Logger?.ILog("Display Information: " + display);
//
//         video.EncodingParameters = new()
//         {
//             "libx265",
//             "-x265-params",
//             $"hdr-opt=1:repeat-headers=1:colorprim=bt2020:transfer=smpte2084:colormatrix=bt2020nc:master-display={display}:max-cll=0,0",
//             "-crf",
//             Quality.ToString(),
//             "-preset",
//             Speed,
//             "-pix_fmt",
//             "yuv420p10le"
//         };
//         // qsv
//         // video.EncodingParameters = new()
//         // {
//         //     "hevc_qsv",
//         //     "-global_quality",
//         //     "28",
//         //     "-colorspace",
//         //     "bt2020nc",
//         //     "-master-display",
//         //     display,
//         //     "-max-cll",
//         //     "0,0"
//         // };
//         // nvenc
//         // video.EncodingParameters = new()
//         // {
//         //     "hevc_nvenc",
//         //     "-rc",
//         //     "vbr_hq",
//         //     "-preset",
//         //     "slow",
//         //     "-b:v",
//         //     "10M",
//         //     "-colorspace",
//         //     "bt2020nc",
//         //     "master-display",
//         //     display,
//         //     "max-cll",
//         //     "0,0"
//         // };
//         video.Codec = "hevc";
//
//         return 1;
//     }
//
//     private Result<SideData> GetColorData(NodeParameters args, string ffprobe, string file)
//     {
//         var result = args.Execute(new()
//         {
//             Command = ffprobe,
//             ArgumentList = new[]
//             {
//                 "-hide_banner",
//                 "-loglevel",
//                 "warning",
//                 "-select_streams",
//                 "v",
//                 "-print_format",
//                 "json",
//                 "-show_frames",
//                 "-read_intervals",
//                 "%+#1",
//                 "-show_entries",
//                 "frame=color_space,color_primaries,color_transfer,side_data_list,pix_fmt",
//                 "-i",
//                 file
//             }
//         });
//         if (result.ExitCode != 0)
//             return Result<SideData>.Fail("FFprobe failed to get HDR+ information");
//
//         RootObject? rootObject;
//         try
//         {
//             string json = result.StandardOutput?.EmptyAsNull() ?? result.Output;
//             rootObject = JsonSerializer.Deserialize<RootObject>(json);
//         }
//         catch (Exception ex)
//         {
//             return Result<SideData>.Fail("Error parsing FFprobe JSON: " + ex.Message);
//         }
//
//         var sideData = rootObject?.Frames?.FirstOrDefault()?.SideDataList?.FirstOrDefault();
//         if (sideData == null || sideData.BlueX == null)
//             return Result<SideData>.Fail("No side data found");
//         
//         return sideData;
//     }
//
//     public bool Hdr10MetadataExist(NodeParameters args, string hdr10plusTool, string file)
//     {
//         
//     }
//
//     /// <summary>
//     /// Represents the side data of the frame.
//     /// </summary>
//     public class SideData
//     {
//         [JsonPropertyName("side_data_type")]
//         public string SideDataType { get; set; }
//
//         [JsonPropertyName("red_x")]
//         public string RedX { get; set; }
//
//         [JsonPropertyName("red_y")]
//         public string RedY { get; set; }
//
//         [JsonPropertyName("green_x")]
//         public string GreenX { get; set; }
//
//         [JsonPropertyName("green_y")]
//         public string GreenY { get; set; }
//
//         [JsonPropertyName("blue_x")]
//         public string BlueX { get; set; }
//
//         [JsonPropertyName("blue_y")]
//         public string BlueY { get; set; }
//
//         [JsonPropertyName("white_point_x")]
//         public string WhitePointX { get; set; }
//
//         [JsonPropertyName("white_point_y")]
//         public string WhitePointY { get; set; }
//
//         [JsonPropertyName("min_luminance")]
//         public string MinLuminance { get; set; }
//
//         [JsonPropertyName("max_luminance")]
//         public string MaxLuminance { get; set; }
//
//         [JsonPropertyName("max_content")]
//         public int MaxContent { get; set; }
//
//         [JsonPropertyName("max_average")]
//         public int MaxAverage { get; set; }
//     }
//
//     /// <summary>
//     /// Represents a frame.
//     /// </summary>
//     public class Frame
//     {
//         [JsonPropertyName("pix_fmt")]
//         public string PixFmt { get; set; }
//
//         [JsonPropertyName("color_space")]
//         public string ColorSpace { get; set; }
//
//         [JsonPropertyName("color_primaries")]
//         public string ColorPrimaries { get; set; }
//
//         [JsonPropertyName("color_transfer")]
//         public string ColorTransfer { get; set; }
//
//         [JsonPropertyName("side_data_list")]
//         public List<SideData> SideDataList { get; set; }
//     }
//
//     /// <summary>
//     /// Represents the root object of the JSON.
//     /// </summary>
//     public class RootObject
//     {
//         [JsonPropertyName("frames")]
//         public List<Frame> Frames { get; set; }
//     }
// }