using System.Diagnostics;

namespace FileFlows.VideoNodes;

/// <summary>
/// Tests a video file for errors
/// </summary>
public class VideoHasErrors: VideoNode
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets the help URL 
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-has-errors";
    /// <summary>
    /// Gets the icon for this flow element
    /// </summary>
    public override string Icon => "fas fa-exclamation-circle";

    /// <summary>
    /// Gets or sets if hardware decoding should be used
    /// </summary>
    [DefaultValue(true)]
    [Boolean(1)]
    public bool HardwareDecoding { get; set; }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var file = args.FileService.GetLocalPath(args.WorkingFile);
        if (file.IsFailed)
        {
            args.Logger?.ILog("Failed to get file: " + file.Error);
            return -1;
        }
        
        var videoInfo = GetVideoInfo(args);
        
        var result = ValidateFile(args, FFMPEG, file, videoInfo, useHardwareDecoding: HardwareDecoding);
        if (result.NoErrors)
            return 2;
        
        return 1;
    }

    /// <summary>
    /// Validates a video file for errors using FFmpeg.
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <param name="ffmpegPath">The path to the FFmpeg executable.</param>
    /// <param name="filename">The path to the video file to be validated.</param>
    /// <param name="info">The video information for the current file</param>
    /// <param name="useHardwareDecoding">If hardware decoding should be attempted</param>
    /// <returns>
    /// A tuple containing a boolean indicating whether there are no errors (NoErrors)
    /// and a log message (Log) from FFmpeg.
    /// </returns>
    public static (bool NoErrors, string Log) ValidateFile(NodeParameters args, string ffmpegPath, string filename, VideoInfo info, bool useHardwareDecoding)
    {
        if (System.IO.File.Exists(ffmpegPath) == false)
            return (false, "FFmpeg does not exist at the specified path.");

        if (System.IO.File.Exists(filename) == false)
            return (false, "The input file does not exist.");
        
        var video = info.VideoStreams.FirstOrDefault(x => x.IsImage == false);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            //ArgumentList = { "-v", "error", "-i", filename, "-f", "null", "-" },
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        processStartInfo.ArgumentList.Add("-v");
        processStartInfo.ArgumentList.Add("error");

        if (useHardwareDecoding)
        {
            var hardwareDecodingArgs =
                FfmpegBuilderNodes.FfmpegBuilderExecutor.GetHardwareDecodingArgs(args, filename, ffmpegPath,
                    video?.Codec);
            if (hardwareDecodingArgs?.Any() == true)
            {
                foreach (var hwArg in hardwareDecodingArgs)
                    processStartInfo.ArgumentList.Add(hwArg);
            }
        }

        processStartInfo.ArgumentList.Add("-i");
        processStartInfo.ArgumentList.Add(filename);
        processStartInfo.ArgumentList.Add("-f");
        processStartInfo.ArgumentList.Add("null");
        processStartInfo.ArgumentList.Add("-");

        Process process = new Process
        {
            StartInfo = processStartInfo
        };

        var line = "Executing: " + (ffmpegPath.IndexOf(" ") > 0 ? "\"" + ffmpegPath + "\"" : ffmpegPath) + " " +
                      string.Join(" ",
                          processStartInfo.ArgumentList.Select(x => x.IndexOf(" ") > 0 ? "\"" + x + "\"" : x)
                              .ToArray());
        args.Logger?.ILog(new string('-', line.Length));
        args.Logger?.ILog(line);
        args.Logger?.ILog(new string('-', line.Length));

        process.Start();
        string output = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (output.Contains("error"))
        {
            args.Logger?.ILog("Errors detected in file");
            args.Logger?.WLog(output);
            return (false, output);
        }

        args.Logger?.ILog("No errors detected");
        if(string.IsNullOrWhiteSpace(output) == false)
            args.Logger?.ILog(output);

        return (true, string.Empty);
    }
}