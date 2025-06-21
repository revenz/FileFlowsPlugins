using System.Globalization;
using System.IO;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// AutoCRF encoder using ab-av1's crf-search
/// </summary>
public class FfmpegBuilderVideoEncodeAutoCrf : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override string HelpUrl =>
        "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/video-encode-auto-crf";

    [Select(nameof(CodecOptions), 1)]
    [DefaultValue("hevc")]
    public string Codec { get; set; }

    [NumberFloat(2)] [DefaultValue(11.5f)] public float MaxBitrate { get; set; }

    [Boolean(3)] [DefaultValue(false)] public bool FixDolby5 { get; set; }

    [Boolean(4)] [DefaultValue(false)] public bool ErrorOnFail { get; set; }

    public static List<ListOption> CodecOptions => new()
    {
        new() { Label = "H.264", Value = "h264" },
        new() { Label = "HEVC", Value = "hevc" },
        new() { Label = "AV1", Value = "av1" }
    };

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        // Checking dependencies
        var abAv1Result = FindTool(args, "ab-av1", ["/opt/autocrf", "/usr/local/bin"]);
        if (abAv1Result.Failed(out var error))
            return args.Fail(error);
        var abAv1 = abAv1Result.Value;;

        if (FindTool(args, "ffmpeg", "/opt/ffmpeg-static/bin", "/opt/ffmpeg-uranite-static/bin").Failed(out error))
            return args.Fail(error);

        if (FindTool(args, "ffmpeg", "/usr/local/bin").Failed(out error))
            return args.Fail(error);

        if (Model.VideoInfo.VideoStreams?.Any() != true)
            return args.Fail("No video streams found.");


        Codec = Codec?.EmptyAsNull() ?? "hevc";

        var video = Model.VideoStreams?.FirstOrDefault(x => x.Deleted == false);
        if (video?.Stream == null)
            return args.Fail("No video stream");

        string currentCodec = video.Stream.Codec?.ToLowerInvariant() ?? string.Empty;

        var localFileResult = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFileResult.Failed(out error))
            return args.Fail(error);

        var localFile = localFileResult.Value;

        var videoBitRate = GetBitrate(args, Model.VideoInfo, localFile);
        if (videoBitRate <= 0)
            return args.Fail("Unable to determine video bitrate");


        var targetBitRate = MaxBitrate * 1024 * 1024;
        var bitratePercent = (int)Math.Floor((100 / videoBitRate) * targetBitRate);

        // Video Description
        var videoDescription = $"{BytesToHuman(videoBitRate)} ${Codec}";
        List<string> videoColors = [];
        if (video.Stream.HDR)
            videoColors.Add("HDR");

        if (video.Stream.DolbyVision)
            videoColors.Add("DoVi");

        if (videoColors.Count > 0)
            videoDescription += " (" + string.Join(" / ", videoColors) + " )";

        // Okay, Let's go!
        var forceEncode = false;
        var firstTryPercentage = 85;
        var secondTryPercentage = 100;
        var firstTryScore = 97;
        var secondTryScore = 95;
        var preset = "slow";

        args.Logger?.ILog($"Video is ${videoDescription}");

        // if we're cropping black bars, we will force the encode
        bool croppingBlackBars =
            video.Filter?.Any(x => x?.StartsWith("crop=", StringComparison.InvariantCultureIgnoreCase) == true) == true;
        
        forceEncode |= DolbyVisionFix(args, video);
        forceEncode |= croppingBlackBars;

        // If the bitrate is more than we want then we don't care what the codec is
        if (videoBitRate > targetBitRate)
        {
            args.Logger?.WLog("Unacceptable bitrate");
            args.Logger?.WLog($"Bitrate is ${BytesToHuman(videoBitRate)}, higher than ${MaxBitrate} MBps");
            args.Logger?.ILog("Will fallback to bitrate encoding");
            forceEncode = true;

            if (firstTryPercentage > bitratePercent)
            {
                firstTryPercentage = bitratePercent;
            }

            secondTryPercentage = bitratePercent;
        }
        else
        {
            targetBitRate = videoBitRate;
        }

        // The bitrate is good so we check if the codec is already hevc
        if (forceEncode == false && Codec.Equals(currentCodec, StringComparison.CurrentCultureIgnoreCase))
        {
            args.Logger?.ILog($"Bitrate ({videoBitRate}) and codec ({currentCodec}) acceptable, skipping encode.");
            return 2;
        }


        string encoder = GetEncoder(args);

        args.Logger?.ILog($"Targeting {firstTryPercentage}% size, {firstTryScore}% VMAF");
        var attempt = CrfSearch(args, abAv1, localFile, encoder, preset,
            85, 97, videoBitRate, video);

        if (attempt.Winner == null)
        {
            args.Logger?.ILog(
                $"First attempt failed retrying with {secondTryPercentage}% size, {secondTryScore}% VMAF");
            attempt = CrfSearch(args, abAv1, localFile, encoder, preset,
                secondTryPercentage, secondTryScore, videoBitRate, video);
        }

        if (attempt.Winner != null)
        {
            var crf_arg = GetCrfArg(encoder);
            args.Variables["ManualParameters"] = $"{attempt.Command} ${crf_arg}:v ${attempt.Winner.Crf}";
            args.Logger.ILog($"Attempt successful with {attempt.Winner.Size}% size, ${attempt.Winner.Score}% VMAF");
            args.AdditionalInfoRecorder("Score", attempt.Winner.Score, 1000, null);
            args.AdditionalInfoRecorder("CRF", attempt.Winner.Crf, 1000, null);
            return 1;
        }

        if (attempt.Error)
        {
            args.Logger?.ELog(attempt.Message);
            return args.Fail($"AutoCRF: {attempt.Message}");
        }


        // fallback
        if (forceEncode == false)
        {
            args.Logger?.ILog("Falling back to copy as codec and bitrate are acceptable");
            return 2;
        }

        // setup bitrate encode
        args.Variables["ManualParameters"] = string.Join(" ", attempt.Command);
        var t = targetBitRate / 1024.00 / 1024.00;

        video.AdditionalParameters.AddRange([
            "-b:v:{index}",
            $"{t:F2}M",
            "-minrate",
            $"{(t * 0.75):F2}M",
            "-maxrate",
            $"{(t * 1.25):F2}M",
            "-bufsize",
            $"{Math.Round(t)}M"
        ]);
        args.Logger?.ILog(
            $"Falling back to bitrate encoding as video is unacceptable ${BytesToHuman(targetBitRate)}");
        args.AdditionalInfoRecorder("Score", "Not found", 1000, null);
        args.AdditionalInfoRecorder("CRF", BytesToHuman(targetBitRate), 1000, null);

        // Falling back bitrate encode as we could not find a suitable CRF
        return 1;
    }

    private bool DolbyVisionFix(NodeParameters args, FfmpegVideoStream video)
    {
        bool forceEncode = false;
        if (video.Stream.DolbyVision == false || video.Stream.HDR  || FixDolby5 == false) 
            return forceEncode;

        var ffmpeg = args.GetToolPath("ffmpeg");
        
        args.Logger?.ILog("Video is DoVi without a fallback, so were creating one");
        forceEncode = true;
        args.Logger?.ILog("Testing for openCL");
        var processResult = args.Execute(new ExecuteArgs()
        {
            Command = ffmpeg,
            ArgumentList =
            [
                "-hwaccel", "opencl", "-f", "lavfi", "-i", "testsrc=size=640x480:rate=25", "-t", "1", "-c:v",
                "libx264", "-f", "null", "-"
            ]
        });
        if (processResult.ExitCode == 0)
        {
            Model.CustomParameters.AddRange(
            [
                "-init_hw_device",
                "opencl=ocl",
                "-filter_hw_device",
                "ocl"
            ]);

            video.Filter.Add(
                "format=p010le,hwupload=derive_device=opencl,tonemap_opencl=tonemap=bt2390:transfer=smpte2084:matrix=bt2020:primaries=bt2020:format=p010le,hwdownload,format=p010le");
        }
        else
        {
            args.Logger?.WLog("Could not find openCL, you may want the oneVPL DockerMod");
            video.Filter.Add(
                "tonemapx=tonemap=bt2390:transfer=smpte2084:matrix=bt2020:primaries=bt2020"
            );
        }

        args.Logger?.WLog("QSV does not support dolby vision 5 decode properly so we are disabling it");
        Variables["NoQSV"] = true;

        return forceEncode;
    }

    private float GetBitrate(NodeParameters args, VideoInfo videoInfo, string localFile)
    {
        var video = videoInfo.VideoStreams.FirstOrDefault(x => x.Bitrate > 0);

        if(video != null)
            return video.Bitrate;
        

        args.Logger?.ILog("Bitrate not found in metadata, calculating...");

        float GetEstimatedVideoBitrate(float totalBitrate)
        {
            if (videoInfo.AudioStreams?.Any() != true)
                return totalBitrate;

            foreach (var audio in videoInfo.AudioStreams)
                totalBitrate -= audio.Bitrate > 0 ? audio.Bitrate : totalBitrate * 0.05f;

            return Math.Max(0, totalBitrate);
        }

        if (videoInfo.Bitrate > 0)
            return GetEstimatedVideoBitrate(videoInfo.Bitrate);

        // Fallback to file size-based calculation
        var fileSize = new FileInfo(localFile).Length; // bytes
        var duration = videoInfo.VideoStreams[0].Duration;
        if (duration.TotalSeconds < 1)
        {
            args.Logger?.WLog("No duration available to calculate bitrate.");
            return 0;
        }

        var totalBitrate = (float)(fileSize * 8 / duration.TotalSeconds); // bits per second
        args.Logger?.ILog($"Calculated total bitrate from file size and duration: {totalBitrate} bps");

        return GetEstimatedVideoBitrate(totalBitrate);
    }


    /// <summary>
    /// Gets the encoder to use
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the encoder to use</returns>
    private string GetEncoder(NodeParameters args)
    {
        bool noNvidia =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "nonvidia" && x.Value as bool? == true);
        bool noQsv =
            args.Variables.Any(x => x.Key?.ToLowerInvariant() == "noqsv" && x.Value as bool? == true);

        switch (Codec)
        {
            case "hevc":
            {
                if (noQsv == false && CanUseHardwareEncoding.CanProcess_Qsv_Hevc(args))
                    return "hevc_qsv";
                if (noNvidia == false && CanUseHardwareEncoding.CanProcess_Nvidia_Hevc(args))
                    return "hevc_nvenc";
                return "hevc";
            }
            case "h264":
            {
                if (noQsv == false && CanUseHardwareEncoding.CanProcess_Qsv_H264(args))
                    return "h264_qsv";
                if (noNvidia == false && CanUseHardwareEncoding.CanProcess_Nvidia_H264(args))
                    return "h264_nvenc";
                return "h264";
            }
            case "av1":
            {
                if (noQsv == false && CanUseHardwareEncoding.CanProcess_Qsv_AV1(args))
                    return "av1_qsv";
                if (noNvidia == false && CanUseHardwareEncoding.CanProcess_Nvidia_AV1(args))
                    return "av1_nvenc";
                return "libsvtav1";
            }
        }

        return Codec.ToLower();
    }

    private static string GetCrfArg(string codec)
    {
        codec = codec.ToLowerInvariant();
        if (codec.Contains("nvenc")) return "-cq";
        if (codec.Contains("vaapi")) return "-q";
        if (codec.Contains("qsv")) return "-global_quality";
        return "-crf";
    }

    private static Result<string> FindTool(NodeParameters args, string tool, params string[] paths)
    {
        foreach (var path in paths)
        {
            string fullPath = Path.Combine(path, tool);
            if (File.Exists(fullPath))
                return fullPath;
        }

        return Result<string>.Fail($"Tool {tool} not found in any provided paths");
    }

    private CrfSearchResult CrfSearch(NodeParameters args, string abAv1, string localFile, string targetCodec,
        string preset,
        int bitratePercent, int targetPercent, float videoBitrate, FfmpegVideoStream videoStream)

    {
        List<string> command = [targetCodec, "-preset", preset];
        command.AddRange(["-g", (videoStream.Stream.FramesPerSecond * 10).ToString(CultureInfo.InvariantCulture)]);

        if (targetCodec.Contains("qsv", StringComparison.InvariantCultureIgnoreCase))
            command.AddRange(["-look_ahead", "1", "-extbrc", "1", "look_ahead_depth", "40"]);

        var videoPixelFormat = "yuv420p";

        if (videoStream.Stream.Is10Bit)
        {
            videoPixelFormat = "yuv420p10le";
            if (targetCodec.Contains("hevc", StringComparison.InvariantCultureIgnoreCase))
                command.AddRange(["-pix_fmt:v:0", "p010le", "-profile:v:0", "main10"]);
        }

        var targetBitRate = (bitratePercent / 100f) * videoBitrate;

        args.Logger?.ILog($"Searching for CRF under {BytesToHuman(targetBitRate)} @ {targetPercent}% original quality");


        var executeArgs = new ExecuteArgs();
        executeArgs.Command = abAv1;
        executeArgs.ArgumentList =
        [
            "crf-search",
            "-i",
            localFile,
            "--preset",
            preset,
            "-e",
            targetCodec,
            "--temp-dir",
            args.TempPath,
            "--min-vmaf",
            targetPercent.ToString(),
            "--max-encoded-percent",
            bitratePercent.ToString(),
            "--pix-format",
            videoPixelFormat,
            "--min-crf",
            "5",
            "--max-crf",
            "25",
            "--min-samples",
            "5",
            //"--sample-duration",
            //"5s",
        ];


        // if (OperatingSystem.IsWindows() == false) {
        //     executeArgs.Command = "bash";
        //     var args = executeArgs.argumentList.join(" ");
        //     var cache = args.TempPath.replace(/[^\/]+$/, "");
        //
        //     executeArgs.argumentList = [
        //         "-c",
        //         `XDG_CACHE_HOME='${cache}' PATH=${path}:\$PATH ${abAv1} ${args}`,
        //         "/dev/null",
        //     ];
        // }

        var returnValue = new CrfSearchResult();
        returnValue.Command = command;

        executeArgs.Error += (line) =>
        {
            if (string.IsNullOrWhiteSpace(line))
                return;


            line = line.Substring(line.IndexOf(" ", StringComparison.Ordinal) + 1);

            Match match;

            match = Regex.Match(line, @"encoding sample (\d+)/(\d+).* crf (\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                args.AdditionalInfoRecorder("Sampling", $"CRF {match.Groups[3].Value}", 1, null);
                if (double.TryParse(match.Groups[1].Value, out double part) &&
                    double.TryParse(match.Groups[2].Value, out double total) && total != 0)
                {
                    float percent = (float)((100f / total) * part);
                    args.PartPercentageUpdate(percent);
                }

                return;
            }

            match = Regex.Match(line, @"eta (\d+) (seconds|minutes|hours|days|weeks)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                args.AdditionalInfoRecorder("ETA", $"{match.Groups[1].Value} {match.Groups[2].Value}", 1, null);
                return;
            }

            match = Regex.Match(line, @"crf ([0-9.]+) VMAF ([0-9.]+) predicted.*\(([0-9.]+)%", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                returnValue.Data.Add(new()
                {
                    Crf = match.Groups[1].Value.Trim(),
                    Score = match.Groups[2].Value.Trim(),
                    Size = match.Groups[3].Value.Trim()
                });
            }

            match = Regex.Match(line, @"crf ([0-9.]+) successful", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string successfulCrf = match.Groups[1].Value;
                foreach (var entry in returnValue.Data)
                {
                    if (entry.Crf == successfulCrf)
                    {
                        returnValue.Winner = entry;
                        break;
                    }
                }
            }
        };

        var executeAbAv1 = args.Execute(executeArgs);

        if (executeAbAv1.ExitCode != 0)
        {
            if (executeAbAv1.Output.Contains("Failed to find a suitable crf",
                    StringComparison.InvariantCultureIgnoreCase))
            {
                returnValue.Message = "Failed to find a suitable crf";
                if (ErrorOnFail)
                {
                    returnValue.Error = true;
                }
            }
            else
            {
                args.Logger?.WLog(executeAbAv1.Output);
                returnValue.Message =
                    "Failed to execute ab-av1: " + executeAbAv1.ExitCode;
                returnValue.Error = true;
            }
        }

        PrintTable(args.Logger, returnValue);

        return returnValue;
    }

    /// <summary>
    /// Prints the CRF Search Result to the log as a table
    /// </summary>
    /// <param name="logger">the logger</param>
    /// <param name="result">the search result</param>
    void PrintTable(ILogger logger, CrfSearchResult result)
    {
        logger.ILog(" ");
        logger.ILog("| CRF | Score | Size |");
        logger.ILog("----------------------");

        foreach (var line in result.Data ?? [])
        {
            string crf = line.Crf?.PadLeft(4).Substring(0, Math.Min(4, line.Crf.Length)) ?? "    ";
            string score = line.Score?.PadLeft(5) ?? "     ";
            string size = line.Size?.PadLeft(3) ?? "   ";
            logger.ILog($"|{crf} | {score} | {size}% |");
        }

        if (result.Winner != null)
        {
            string crf = result.Winner.Crf?.PadLeft(4).Substring(0, Math.Min(4, result.Winner.Crf.Length)) ??
                         "    ";
            string score = result.Winner.Score?.PadLeft(5) ?? "     ";
            string size = result.Winner.Size?.PadLeft(3) ?? "   ";
            logger.ILog("----------------------");
            logger.ILog($"|{crf} | {score} | {size}% |");
        }

        logger.ILog(" ");
    }



    class CrfSearchResult
    {
        public List<string> Command;
        public List<CrfScore> Data;
        public CrfScore Winner;
        public string Message;
        public bool Error;

        public static CrfSearchResult Failed(string message) => new CrfSearchResult
        {
            Message = message,
            Error = true,
            Data = new List<CrfScore>()
        };
    }

    class CrfScore
    {
        public string Crf { get; set; }
        public string Score { get; set; }
        public string Size { get; set; }
    }

    private static string BytesToHuman(float bytes)
    {
        string[] sizes = { "Bps", "KBps", "MBps", "GBps", "TBps" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

}