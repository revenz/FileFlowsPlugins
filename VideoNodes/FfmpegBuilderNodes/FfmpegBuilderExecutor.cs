using FileFlows.Plugin;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Runtime.InteropServices;
using FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderExecutor: FfmpegBuilderNode
{
    public override string Icon => "far fa-file-video";
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.BuildEnd;

    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder";

    public override bool NoEditorOnAdd => true;

    /// <summary>
    /// Gets or sets if hardware decoding should be used
    /// </summary>
    [DefaultValue(true)]
    [Boolean(1)]
    public bool HardwareDecoding { get; set; }
    
    /// <summary>
    /// Gets or sets the strictness
    /// </summary>
    [DefaultValue("experimental")]
    [Select(nameof(StrictOptions), 2)]
    public string Strictness { get; set; }

    private static List<ListOption> _StrictOptions;
    /// <summary>
    /// Gets the strict options
    /// </summary>
    public static List<ListOption> StrictOptions
    {
        get
        {
            if (_StrictOptions == null)
            {
                _StrictOptions = new List<ListOption>
                {
                    new () { Label = "Experimental", Value = "experimental" },
                    new () { Label = "Unofficial", Value = "unofficial" },
                    new () { Label = "Normal", Value = "normal" },
                    new () { Label = "Strict", Value = "strict" },
                    new () { Label = "Very", Value = "very" },
                };
            }
            return _StrictOptions;
        }
    }


    public override int Execute(NodeParameters args)
    {
        var model = this.Model;
        if (model == null)
        {
            args.Logger.ELog("FFMPEG Builder model is null");
            return -1;
        }
        else if (model.VideoInfo == null)
        {
            args.Logger.ELog("FFMPEG Builder VideoInfo is null");
            return -1;
        }
        else if (model.VideoInfo.FileName == null)
        {
            args.Logger.ELog("FFMPEG Builder VideoInfo Filename is null");
            return -1;
        }
        List<string> ffArgs = new List<string>();

        if (model.CustomParameters?.Any() == true)
            ffArgs.AddRange(model.CustomParameters);

        bool hasChange = false;
        int actualIndex = 0;
        int overallIndex = 0;
        int currentType = 0;

        string sourceExtension = model.VideoInfo.FileName.Substring(model.VideoInfo.FileName.LastIndexOf(".") + 1).ToLower();
        string extension = (model.Extension?.EmptyAsNull() ?? "mkv").ToLower();

        foreach (var item in model.VideoStreams.Select((x, index) => (stream: (FfmpegStream)x, index, type: 1, list: model.VideoStreams.Select(x => (FfmpegStream)x).ToList())).Union(
                             model.AudioStreams.Select((x, index) => (stream: (FfmpegStream)x, index, type: 2, list: model.AudioStreams.Select(x => (FfmpegStream)x).ToList()))).Union(
                             model.SubtitleStreams.Select((x, index) => (stream: (FfmpegStream)x, index, type: 3, list: model.SubtitleStreams.Select(x => (FfmpegStream)x).ToList()))))
        {
            if (item.stream.Deleted)
            {
                hasChange = true;
                continue;
            }
            if (currentType != item.type)
            {
                actualIndex = 0;
                currentType = item.type;
            }

            VideoFileStream vfs = item.stream is FfmpegVideoStream ? ((FfmpegVideoStream)item.stream).Stream :
                item.stream is FfmpegAudioStream ? ((FfmpegAudioStream)item.stream).Stream :
                ((FfmpegSubtitleStream)item.stream).Stream;


            var streamArgs = item.stream.GetParameters(new ()
            {
                Logger = args.Logger,
                OutputOverallIndex = overallIndex,
                OutputTypeIndex = actualIndex,
                SourceExtension = sourceExtension,
                DestinationExtension = extension,
                UpdateDefaultFlag = item.list.Any(x => x.Deleted == false && x.IsDefault)
            });
            if (streamArgs?.Any() != true)
            {
                // stream was not included for some reason
                // eg an unsupported subtitle type was removed
                continue;
            }
            for (int i = 0; i < streamArgs.Length; i++)
            {
                streamArgs[i] = streamArgs[i].Replace("{sourceTypeIndex}", vfs.TypeIndex.ToString());
                streamArgs[i] = streamArgs[i].Replace("{index}", actualIndex.ToString());
            }

            ffArgs.AddRange(streamArgs);
            hasChange |= item.stream.HasChange | item.stream.ForcedChange;
            ++actualIndex;
            ++overallIndex;
        }

        if (model.MetadataParameters?.Any() == true)
        {
            hasChange = true;
            ffArgs.AddRange(model.MetadataParameters);
        }

        if (model.ForceEncode == false && hasChange == false && (string.IsNullOrWhiteSpace(model.Extension) || args.WorkingFile.ToLower().EndsWith("." + model.Extension.ToLower())))
            return 2; // nothing to do 

        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.IsFailed)
        {
            args.Logger?.ELog("Failed to get local file: " + localFile.Error);
            return -1;
        }

        List<string> startArgs = new List<string>();
        if (model.InputFiles?.Any() == false)
            model.InputFiles.Add(new InputFile(localFile));
        else
            model.InputFiles[0].FileName = localFile;

        startArgs.AddRange(new[] { "-fflags", "+genpts" }); //Generate missing PTS if DTS is present.

        startArgs.AddRange(new[] {
            "-probesize", VideoInfoHelper.ProbeSize + "M"
        });

        bool isEncodingVideo =
            model.VideoStreams.Any(x => x.Deleted == false && x.EncodingParameters?.Any() == true || x.Filter?.Any() == true);
        if (isEncodingVideo == false)
        {
            args.Logger?.ILog("No video encoding, no need for hardware decoding");
        }
        else if (Environment.GetEnvironmentVariable("HW_OFF") == "1")
        {
            args.Logger?.ILog("HW_OFF detected");
        }
        else if (Variables.TryGetValue("HW_OFF", out object oHwOff) && 
                 (oHwOff as bool? == true || oHwOff?.ToString() == "1")
                 )
        {
            args.Logger?.ILog("HW_OFF detected");
            
        }
        else if (HardwareDecoding)
        {
            // if(ffArgs.Any(x => x.Contains("_qsv")))
            // {
            //     // use qsv decoder
            //     args.Logger?.ILog("_qsv detected using qsv hardware decoding");
            //     startArgs.AddRange(new[] { "-hwaccel", "qsv" });// , "-hwaccel_output_format", "qsv" });
            // }
            // else if(ffArgs.Any(x => x.Contains("_nvenc")))
            // {
            //     // use nvidia decoder
            //     args.Logger?.ILog("_nvenc detected using cuda hardware decoding");
            //     startArgs.AddRange(new[] { "-hwaccel", "cuda" }); //, "-hwaccel_output_format", "cuda" });
            // }
            //else 
            {
                args.Logger?.ILog("Auto-detecting hardware decoder to use");
                var video = this.Model.VideoStreams.FirstOrDefault(x => x.Stream.IsImage == false);
                
                args.Logger?.ILog("Pixel Format: " + (video?.Stream?.PixelFormat?.EmptyAsNull() ?? "Unknown"));
                bool targetIs10Bit = string.Join(" ", ffArgs).Contains("p010le");
                string pxtFormat = video?.Stream?.PixelFormat;
                if (targetIs10Bit && video?.Stream?.Is10Bit == true)
                {
                    args.Logger?.ILog(
                        "Target is 10-Bit forcing p010le");
                    pxtFormat = "p010le";
                }
                else if (targetIs10Bit == false && video?.Stream?.Is10Bit == false && video?.Stream?.Is12Bit == false)
                {
                    args.Logger?.ILog(
                        "Target is 8-Bit forcing nv12");
                    pxtFormat = "nv12";
                }
                else if (targetIs10Bit)
                {
                    args.Logger?.ILog("Target is 10-Bit but source is 8-Bit, clearing pixel format to avoid color issues in output file.");
                    pxtFormat = string.Empty; // clear it, if we use a 8bit pixel format this will break the colours
                }

                List<string> encodingParameters = new ();
                if (video?.EncodingParameters?.Any() == true)
                {
                    encodingParameters.Add("-c:v:" + video.Stream.TypeIndex);
                    encodingParameters.AddRange(video.EncodingParameters);
                    if (video.Filter?.Any() == true)
                    {
                        encodingParameters.Add("-filter:v:" + video.Stream.TypeIndex);
                        encodingParameters.Add(string.Join(",", video.Filter));
                    }
                    encodingParameters = encodingParameters.Select(x =>
                        x.Replace("{index}", video.Stream.Index.ToString())).ToList();
                }

                var decodingParameters =
                    GetHardwareDecodingArgs(args, localFile, FFMPEG, video?.Stream?.Codec, pxtFormat, encodingParameters: encodingParameters);
                if (decodingParameters.Any() == true)
                {
                    args.StatisticRecorder("DecoderParameters", string.Join(" ", decodingParameters));
                    startArgs.AddRange(decodingParameters);
                }
            }
        }

        foreach (var file in model.InputFiles)
        {
            startArgs.Add("-i");
            startArgs.Add(file.FileName);
        }

        if (model.Watermark != null)
        {
            startArgs.AddRange(new[]
            {
                "-i",
                model.Watermark.Image,
                "-filter_complex",
                model.Watermark.Filter.Replace("::wmindex::", (model.InputFiles.Count - 1).ToString()) + "[watermark]"
            });

            for (int i = 0; i < ffArgs.Count; i++)
            {
                if (ffArgs[i] == "0:v:0")
                    ffArgs[i] = "[watermark]";
            }
        }
        
        startArgs.Add("-y");
        if(model.CutDuration != null)
            startArgs.AddRange(new [] { "-t", model.CutDuration.Value.ToString()});
        if(model.StartTime != null)
            startArgs.AddRange(new [] { "-ss", model.StartTime.Value.ToString()});
        
        if (extension.ToLower() == "mp4" && ffArgs.IndexOf("-movflags") < 0 && startArgs.IndexOf("-movflgs") < 0)
        {
            startArgs.AddRange(new[] { "-movflags", "+faststart" });
        }

        ffArgs = startArgs.Concat(ffArgs).ToList();
        if (model.RemoveAttachments != true)
        {
            // FF-378: keep attachments (fonts etc)
            ffArgs.AddRange(new[] { "-map", "0:t?", "-c:t", "copy" });
        }

        // make any adjustments needed for hardware devices
        ffArgs = EncoderAdjustments.EncoderAdjustment.Run(args.Logger, ffArgs);

        var ffmpeg = FFMPEG;
        
        if(string.IsNullOrWhiteSpace(model.PreExecuteCode) == false)
        {
            var preExecutor = new PreExecutor(args, model.PreExecuteCode, ffArgs);
            if (preExecutor.Run() == false)
                return -1;
            if (preExecutor.Args?.Any() == true && string.Join(" ", ffArgs) != string.Join(" ", preExecutor.Args))
            {
                args.Logger.ILog("Pre-Executor updated FFmpeg Arguments!");
                ffArgs = preExecutor.Args;
            }
        }


        if (Encode(args, ffmpeg, ffArgs, extension, dontAddInputFile: true, strictness: Strictness) == false)
            return -1;

        foreach (var file in model.InputFiles)
        {
            if (file.DeleteAfterwards)
            {
                if (System.IO.File.Exists(file.FileName) == false)
                    continue;

                args.Logger.ILog("Deleting file: " + file.FileName);
                try
                {
                    System.IO.File.Delete(file.FileName);
                }
                catch (Exception ex)
                {
                    args.Logger.WLog("Failed to delete file: " + ex.Message);
                }
            }
        }

        return 1;
    }

    internal static string[] GetHardwareDecodingArgs(NodeParameters args, string localFile, string ffmpeg, string codec, string pixelFormat, List<string> encodingParameters = null)
    {
        string testFile = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".hwtest.mkv");
        if (string.IsNullOrWhiteSpace(codec))
            return new string[] { };
        
        if (encodingParameters?.Any() == true)
            args.Logger?.ILog("Testing with encoding parameters:" + string.Join(" ", encodingParameters));
        
        bool isH264 = codec.Contains("264");
        bool isHevc = codec.Contains("265") || codec.ToLowerInvariant().Contains("hevc");

        var decoders = isH264 ? Decoders_h264(args) :
            isHevc ? Decoders_hevc(args) :
            Decoders_Default(args);
        try
        {
            List<string> tested = new List<string>();
            foreach (var hwOrig in decoders)
            {
                if (hwOrig == null)
                    continue;
                if (CanUseHardwareEncoding.DisabledByVariables(args, hwOrig))
                {
                    args.Logger?.ILog("HW disabled by variables: " + string.Join(" ", hwOrig));
                    continue;
                }
                
                if (hwOrig.Contains("#FORMAT#") && string.IsNullOrWhiteSpace(pixelFormat))
                {
                    args.Logger?.ILog("No pixel format detected skipping check for: " + string.Join(" ", hwOrig));
                    continue;
                }


                var hw = hwOrig.Select(x => x.Replace("#FORMAT#", pixelFormat)).ToArray();

                var arguments = new List<string>()
                {
                    "-y",
                };
                arguments.AddRange(hw);
                arguments.AddRange(new[]
                {
                    "-i", localFile,
                    "-frames:v", "1",
                    //"-ss", "10",
                    // instead of file output to null
                    //"-f", "null", "-",
                    //testFile
                });
                if (encodingParameters?.Any() == true)
                    arguments.AddRange(encodingParameters);

                arguments.AddRange(new[] { "-f", "null", "-" });
                
                if (arguments.Any(x => x.ToLowerInvariant().Contains("vaapi")))
                {
                    args.Logger?.ILog("VAAPI detected adjusting parameters for testing");
                    arguments = new VaapiAdjustments().Run(args.Logger, arguments);
                }

                args.AdditionalInfoRecorder("Testing", string.Join(" ", hw), 1, new TimeSpan(0, 0, 10));

                try
                {
                    
                    string line = string.Join("", arguments);
                    if (tested.Contains(line))
                        continue; // avoids testing twice if the #FORMAT# already tested one
                    
                    tested.Add(line);

                    DateTime dtStart = DateTime.Now;
                    const int timeout = 10;
                    var result = args.Execute(new ExecuteArgs
                    {
                        Command = ffmpeg,
                        ArgumentList = arguments.ToArray(),
                        Timeout = timeout
                    });
                    if (result.ExitCode == 0)
                    {
                        args.Logger?.ILog("Supported hardware decoding detected: " + string.Join(" ", hw));
                        return hw;
                    }

                    if (result.Completed == false)
                    {
                        // timeout
                        args.Logger?.ILog("Test timed out");
                        if (result.Output?.Contains("frame=") == true ||
                            result.StandardOutput?.Contains("frame=") == true)
                        {
                            args.Logger?.ILog(
                                "Sort of supported hardware decoding detected via frame=: " + string.Join(" ", hw));
                            //return hw;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            args.Logger?.ILog("No hardware decoding available");
            return new string[] { };
        }
        finally
        {
            args.AdditionalInfoRecorder("Testing", null, 1, new TimeSpan(0, 0, 10));
            try
            {
                if (System.IO.File.Exists(testFile))
                    System.IO.File.Delete(testFile);
            }
            catch (Exception)
            {
            }
        }
    }

    private static readonly bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    
    private static string[][] Decoders_h264(NodeParameters args)
    {
        bool noNvidia =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nonvidia" && x.Value as bool? == true);
        bool noQsv =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "noqsv" && x.Value as bool? == true);
        bool noVaapi =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novaapi" && x.Value as bool? == true);
        bool noAmf =
            args.Variables.Any(x =>
                (x.Key?.ToLowerInvariant() == "noamf" || x.Key?.ToLowerInvariant() == "noamd") &&
                x.Value as bool? == true);
        bool noVideoToolbox =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novideotoolbox" && x.Value as bool? == true);
        bool noVulkan =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novulkan" && x.Value as bool? == true);
        bool noDxva2 = OperatingSystem.IsWindows() == false || 
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nodxva2" && x.Value as bool? == true);
        bool noD3d11va = OperatingSystem.IsWindows() == false || 
                         args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nod3d11va" && x.Value as bool? == true);
        bool noOpencl =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "noopencl" && x.Value as bool? == true);

        return new[]
        {
            noVideoToolbox == false && IsMac ? new [] { "-hwaccel", "videotoolbox" } : null,
            noNvidia ? null : new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "#FORMAT#" },
            noNvidia ? null : new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "cuda" }, // this fails with Impossible to convert between the formats supported by the filter 'Parsed_crop_0' and the filter 'auto_scale_0'
            noNvidia ? null : new [] { "-hwaccel", "cuda" },
            noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "#FORMAT#" },
            //noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "p010le" },
            noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "qsv" },
            noQsv ? null : new [] { "-hwaccel", "qsv" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi" },
            noVulkan ? null : new [] { "-hwaccel", "vulkan", "-hwaccel_output_format", "vulkan" },
            noDxva2 ? null : new [] { "-hwaccel", "dxva2" },
            noD3d11va ? null : new [] { "-hwaccel", "d3d11va" },
            noOpencl ? null : new [] { "-hwaccel", "opencl" },
        };
    }

    private static string[][] Decoders_hevc(NodeParameters args)
    {
        bool noNvidia =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nonvidia" && x.Value as bool? == true);
        bool noQsv =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "noqsv" && x.Value as bool? == true);
        bool noVaapi =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novaapi" && x.Value as bool? == true);
        bool noAmf =
            args.Variables.Any(x =>
                (x.Key?.ToLowerInvariant() == "noamf" || x.Key?.ToLowerInvariant() == "noamd") &&
                x.Value as bool? == true);
        bool noVideoToolbox =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novideotoolbox" && x.Value as bool? == true);
        bool noVulkan =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novulkan" && x.Value as bool? == true);
        bool noDxva2 =
            OperatingSystem.IsWindows() == false || args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nodxva2" && x.Value as bool? == true);
        bool noD3d11va =
            OperatingSystem.IsWindows() == false || args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nod3d11va" && x.Value as bool? == true);
        bool noOpencl =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "noopencl" && x.Value as bool? == true);

        return new[]
        {
            noVideoToolbox == false && IsMac ? new [] { "-hwaccel", "videotoolbox" } : null,
            noNvidia ? null : new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "#FORMAT#" },
            noNvidia ? null : new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "cuda" }, // this fails with Impossible to convert between the formats supported by the filter 'Parsed_crop_0' and the filter 'auto_scale_0'
            noNvidia ? null : new [] { "-hwaccel", "cuda" },
            noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "#FORMAT#" },
            noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "qsv" },
            noQsv ? null : new [] { "-hwaccel", "qsv" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi" },
            noVulkan ? null : new [] { "-hwaccel", "vulkan", "-hwaccel_output_format", "vulkan" },
            noDxva2 ? null : new [] { "-hwaccel", "dxva2" },
            noD3d11va ? null : new [] { "-hwaccel", "d3d11va" },
            noOpencl ? null : new [] { "-hwaccel", "opencl" },
        };
    }

    private static string[][] Decoders_Default(NodeParameters args)
    {
        bool noNvidia =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nonvidia" && x.Value as bool? == true);
        bool noQsv =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "noqsv" && x.Value as bool? == true);
        bool noVaapi =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novaapi" && x.Value as bool? == true);
        bool noAmf =
            args.Variables.Any(x =>
                (x.Key?.ToLowerInvariant() == "noamf" || x.Key?.ToLowerInvariant() == "noamd") &&
                x.Value as bool? == true);
        bool noVideoToolbox =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novideotoolbox" && x.Value as bool? == true);
        bool noVulkan =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "novulkan" && x.Value as bool? == true);
        bool noDxva2 =
            OperatingSystem.IsWindows() == false || args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nodxva2" && x.Value as bool? == true);
        bool noD3d11va =
            OperatingSystem.IsWindows() == false || args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nod3d11va" && x.Value as bool? == true);
        bool noOpencl =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "noopencl" && x.Value as bool? == true);

        
        return new[]
        {
            noNvidia ? null : new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "#FORMAT#" },
            noNvidia ? null : new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "cuda" }, // this fails with Impossible to convert between the formats supported by the filter 'Parsed_crop_0' and the filter 'auto_scale_0'
            noNvidia ? null : new [] { "-hwaccel", "cuda" },
            noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "#FORMAT#" },
            //noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "p010le" },
            noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "qsv" },
            noQsv ? null : new [] { "-hwaccel", "qsv" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi", "-v" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi" },
            noVulkan ? null : new [] { "-hwaccel", "vulkan", "-hwaccel_output_format", "vulkan" },
            noDxva2 ? null : new [] { "-hwaccel", "dxva2" },
            noD3d11va ? null : new [] { "-hwaccel", "d3d11va" },
            noOpencl ? null : new [] { "-hwaccel", "opencl" },
        };
    }
}
