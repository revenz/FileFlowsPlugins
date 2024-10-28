using FileFlows.Plugin;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Runtime.InteropServices;
using FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that takes the built FFmpeg command and executes it
/// </summary>
public class FfmpegBuilderExecutor: FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string Icon => "far fa-file-video";
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.BuildEnd;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder";
    /// <inheritdoc />
    public override bool NoEditorOnAdd => true;

    /// <summary>
    /// Gets or sets if hardware decoding should be used
    /// </summary>
    [Select(nameof(HardwareDecodingOptions), 1)]
    [DefaultValue("auto")]
    public object HardwareDecoding { get; set; }
    
    private static List<ListOption>? _HardwareDecodingOptions;
    /// <summary>
    /// Gets the strict options
    /// </summary>
    public static List<ListOption> HardwareDecodingOptions
    {
        get
        {
            if (_HardwareDecodingOptions == null)
            {
                _HardwareDecodingOptions = new List<ListOption>
                {
                    new () { Label = "Off", Value = false },
                    new () { Label = "On", Value = true },
                    new () { Label = "Automatic", Value = "auto" }
                };
            }
            return _HardwareDecodingOptions;
        }
    }
    
    /// <summary>
    /// Gets or sets the strictness
    /// </summary>
    [DefaultValue("experimental")]
    [Select(nameof(StrictOptions), 2)]
    public string Strictness { get; set; }
    
    /// <summary>
    /// Gets or sets if the FFmpeg model should be kept
    /// </summary>
    [Boolean(3)]
    public bool KeepModel { get; set; }

    private static List<ListOption>? _StrictOptions;
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

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var model = this.Model;
        if (model == null)
        {
            args.FailureReason = "FFMPEG Builder model is null";
            args.Logger.ELog(args.FailureReason);
            return -1;
        }
        if (model.VideoInfo == null)
        {
            args.FailureReason = "FFMPEG Builder VideoInfo is null";
            args.Logger.ELog(args.FailureReason);
            return -1;
        }
        if (model.VideoInfo.FileName == null)
        {
            args.FailureReason = "FFMPEG Builder VideoInfo Filename is null";
            args.Logger.ELog(args.FailureReason);
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

        if (model.ForceEncode == false && hasChange == false && (string.IsNullOrWhiteSpace(model.Extension) ||
                                                                 args.WorkingFile.ToLower()
                                                                     .EndsWith("." + model.Extension.ToLower())))
        {
            DoClearModel();
            return 2; // nothing to do
        }

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

        // arguments to add after the inputs have been added
        // this is used by the qsv filter for hw decoding
        List<string> afterStartArguments = new();

        startArgs.AddRange(new[]
        {
            "-probesize", VideoInfoHelper.ProbeSize + "M",
            "-analyzeduration", VideoInfoHelper.AnalyzeDuration.ToString()
        });

        bool isEncodingVideo =
            model.VideoStreams.Any(x => x.Deleted == false && x.EncodingParameters?.Any() == true || x.Filter?.Any() == true);

        bool useHardwareEncoding = GetUseHardwareEncoding(args, ffArgs);
        
        if (isEncodingVideo == false)
        {
            args.Logger?.ILog("No video encoding, no need for hardware decoding");
        }
        else if (Environment.GetEnvironmentVariable("HW_OFF") == "1")
        {
            args.Logger?.ILog("HW_OFF detected");
        }
        else if (Variables.TryGetValue("HW_OFF", out object? oHwOff) && 
                 (oHwOff as bool? == true || oHwOff?.ToString() == "1")
                 )
        {
            args.Logger?.ILog("HW_OFF detected");
            
        }
        else if (useHardwareEncoding)
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
                string? pxtFormat = video?.Stream?.PixelFormat;
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
                    encodingParameters.Add("-map");
                    encodingParameters.Add("0:v:" + video.Stream.TypeIndex);
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
                    GetHardwareDecodingArgs(args, model, localFile, FFMPEG, video?.Stream?.Codec, pxtFormat, 
                        encodingParameters: encodingParameters,
                        inputPixelFormat: video.Stream.PixelFormat,
                        destPixelFormat: targetIs10Bit ? "p010le" : (video?.Stream?.Is10Bit == false && video?.Stream?.Is12Bit == false) ? "nv12" : null);
                if (decodingParameters.Any() == true)
                {
                    args.StatisticRecorderRunningTotals?.Invoke("DecoderParameters", string.Join(" ", decodingParameters));
                    if(decodingParameters.Any(x => x.StartsWith("-filter")))
                        afterStartArguments.AddRange(decodingParameters);
                    else
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

        ffArgs = [..startArgs, ..afterStartArguments, ..ffArgs];
        
        if (model.RemoveAttachments != true)
        {
            // FF-378: keep attachments (fonts etc)
            ffArgs.AddRange(new[] { "-map", "0:t?", "-c:t", "copy" });
        }

        // make any adjustments needed for hardware devices
        ffArgs = EncoderAdjustment.Run(args.Logger, model, ffArgs);

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
        DoClearModel();
        return 1;
    }

    /// <summary>
    /// Checks if the model should be kept and if not, clears it
    /// </summary>
    private void DoClearModel()
    {
        if (KeepModel)
            return;
        Args.Logger?.ILog("Clearing FFmpeg Builder Model");
        
        if(Variables.ContainsKey(MODEL_KEY))
            Variables.Remove(MODEL_KEY);
    }

    /// <summary>
    /// Gets if hardware decoding should be used
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="ffArgs">the FFmpeg arguments</param>
    /// <returns>true to use hardware decoding, otherwise false</returns>
    private bool GetUseHardwareEncoding(NodeParameters args, List<string> ffArgs)
    {
        if (Regex.IsMatch(HardwareDecoding?.ToString()?.ToLowerInvariant() ?? string.Empty, "^(0|false)$"))
            return false;
        if (Regex.IsMatch(HardwareDecoding?.ToString()?.ToLowerInvariant() ?? string.Empty, "^(1|true)$"))
            return true;
        // detect
        args.Logger?.ILog("Auto detecting if hardware decoding should be used.");
        var video = this.Model.VideoStreams.FirstOrDefault(x => x.Stream.IsImage == false);
        bool targetIs10Bit = string.Join(" ", ffArgs).Contains("p010le");
        if (targetIs10Bit && video?.Stream?.Is10Bit == true)
        {
            args.Logger?.ILog(
                "Source and target is 10-Bit, using hardware decoding");
            return true;
        }
        if (targetIs10Bit == false && video?.Stream?.Is10Bit == false && video?.Stream?.Is12Bit == false)
        {
            args.Logger?.ILog(
                "Source and target is 9-Bit, using hardware decoding");
            return true;
        }
        if (targetIs10Bit)
        {
            args.Logger?.ILog("Target is 10-Bit but source is 8-Bit, not using hardware decoding.");
            return false;
        }
        args.Logger?.ILog("Target and source not same bit, not using hardware decoding");
        return false;
    }

    /// <summary>
    /// Gets the hardware decoder to user
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="model">the FFmpeg Builder model</param>
    /// <param name="localFile">the local file</param>
    /// <param name="ffmpeg">the ffmpeg executable</param>
    /// <param name="codec">the code to use</param>
    /// <param name="pixelFormat">the pixel format to test</param>
    /// <param name="encodingParameters">the encoding parameters</param>
    /// <param name="inputPixelFormat">the input pixel format of the source video</param>
    /// <param name="destPixelFormat">the true pixel format we will be encoding in</param>
    /// <returns>the hardware decoding parameters</returns>
    internal static string[] GetHardwareDecodingArgs(NodeParameters args, FfmpegModel? model, string localFile, string ffmpeg, string codec, string pixelFormat, 
        List<string> encodingParameters = null, string inputPixelFormat = null, string destPixelFormat = null)
    {
        var testFile = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".hwtest.mkv");
        if (string.IsNullOrWhiteSpace(codec))
            return new string[] { };
        
        if (encodingParameters?.Any() == true)
            args.Logger?.ILog("Testing with encoding parameters:" + string.Join(" ", encodingParameters));
        
        var isH264 = codec.Contains("264");
        var isHevc = codec.Contains("265") || codec.ToLowerInvariant().Contains("hevc");

        bool pixelFormatChanged = inputPixelFormat != destPixelFormat;
        var decoders = //isH264 ? Decoders_h264(args, inputPixelFormat, pixelFormatChanged) :
            //isHevc ? Decoders_hevc(args, inputPixelFormat, pixelFormatChanged) :
            Decoders_Default(args, inputPixelFormat, pixelFormatChanged);
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
                // filter hardware encoders need to go later on 
                bool filterHardware = hw.Any(x => x.StartsWith("-filter"));
                if(filterHardware == false)
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
                if (filterHardware)
                    arguments.AddRange(hw);
                
                if (encodingParameters?.Any() == true)
                    arguments.AddRange(encodingParameters);

                arguments.AddRange(new[] { "-f", "null", "-" });
                
                if (arguments.Any(x => x.Contains("vaapi", StringComparison.InvariantCultureIgnoreCase)))
                {
                    args.Logger?.ILog("VAAPI detected adjusting parameters for testing");
                    arguments = new VaapiAdjustments().Run(args.Logger, model, arguments);
                }

                args.AdditionalInfoRecorder?.Invoke("Testing", string.Join(" ", hw), 1, new TimeSpan(0, 0, 10));

                try
                {
                    string line = string.Join("", arguments);
                    if (tested.Contains(line))
                        continue; // avoids testing twice if the #FORMAT# already tested one
                    
                    tested.Add(line);

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
            args.AdditionalInfoRecorder?.Invoke("Testing", null, 1, new TimeSpan(0, 0, 10));
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

    
    private static string[][] Decoders_h264(NodeParameters args, string inputPixelFormat, bool pixelFormatChanged)
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
            noNvidia ? null : ["-hwaccel", "cuda", "-hwaccel_output_format", "#FORMAT#"],
            noNvidia ? null : ["-hwaccel", "cuda", "-hwaccel_output_format", "cuda"], // this fails with Impossible to convert between the formats supported by the filter 'Parsed_crop_0' and the filter 'auto_scale_0'
            noNvidia ? null : ["-hwaccel", "cuda"],
            // noQsv || pixelFormatChanged == false ? null : [ "-filter:v:0", "vpp_qsv=format=" + (inputPixelFormat.Contains("10le") ? "yuv420p10le" : "nv12")],
            noQsv ? null : ["-hwaccel", "qsv", "-hwaccel_output_format", "#FORMAT#"],
            //noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "p010le" },
            noQsv ? null : ["-hwaccel", "qsv", "-hwaccel_output_format", "qsv"],
            noQsv ? null : ["-hwaccel", "qsv"],
            noVaapi ? null : ["-hwaccel", "vaapi"],
            noVaapi ? null : ["-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi"],
            noVulkan ? null : ["-hwaccel", "vulkan", "-hwaccel_output_format", "vulkan"],
            noDxva2 ? null : ["-hwaccel", "dxva2"],
            noD3d11va ? null : ["-hwaccel", "d3d11va"],
            noOpencl ? null : ["-hwaccel", "opencl"],
        }.Where(x => x != null).Select(x => x!).ToArray();
    }

    private static string[][] Decoders_hevc(NodeParameters args, string inputPixelFormat, bool pixelFormatChanged)
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
            noQsv || pixelFormatChanged == false ? null : [ "-filter:v:0", "vpp_qsv=format=" + (inputPixelFormat.Contains("10le") ? "yuv420p10le" : "nv12")],
            noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "#FORMAT#" },
            noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "qsv" },
            noQsv ? null : new [] { "-hwaccel", "qsv" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi" },
            noVaapi ? null : new [] { "-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi" },
            noVulkan ? null : new [] { "-hwaccel", "vulkan", "-hwaccel_output_format", "vulkan" },
            noDxva2 ? null : new [] { "-hwaccel", "dxva2" },
            noD3d11va ? null : new [] { "-hwaccel", "d3d11va" },
            noOpencl ? null : new [] { "-hwaccel", "opencl" },
        }.Where(x => x != null).Select(x => x!).ToArray();
    }

    private static string[][] Decoders_Default(NodeParameters args, string inputPixelFormat, bool pixelFormatChanged)
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

        bool isWindows = OperatingSystem.IsWindows();
        
        return new[]
        {
            noVideoToolbox || IsMac == false ? null : new [] {"-hwaccel", "videotoolbox"},
            noNvidia ? null : ["-hwaccel", "cuda", "-hwaccel_output_format", "#FORMAT#"],
            noNvidia ? null : ["-hwaccel", "cuda", "-hwaccel_output_format", "cuda"], // this fails with Impossible to convert between the formats supported by the filter 'Parsed_crop_0' and the filter 'auto_scale_0'
            noNvidia ? null : ["-hwaccel", "cuda"],
            noQsv || pixelFormatChanged == false ? null : [ "-filter:v:0", "vpp_qsv=format=" + (inputPixelFormat.Contains("10le") ? "yuv420p10le" : "nv12")],
            noQsv ? null : ["-hwaccel", "qsv", "-hwaccel_output_format", "#FORMAT#"],
            //noQsv ? null : new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "p010le" },
            noQsv ? null : ["-hwaccel", "qsv", "-hwaccel_output_format", "qsv"],
            noQsv ? null : ["-hwaccel", "qsv"],
            noVaapi ? null : ["-hwaccel", "vaapi"],
            noVaapi ? null : ["-hwaccel", "vaapi", "-v"],
            noVaapi ? null : ["-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi"],
            noVulkan ? null : ["-hwaccel", "vulkan", "-hwaccel_output_format", "vulkan"],
            noDxva2 || isWindows == false ? null : ["-hwaccel", "dxva2"],
            noD3d11va || isWindows == false ? null : ["-hwaccel", "d3d11va"],
            noOpencl ? null : ["-hwaccel", "opencl"],
        }.Where(x => x != null).Select(x => x!).ToArray();
    }
}
