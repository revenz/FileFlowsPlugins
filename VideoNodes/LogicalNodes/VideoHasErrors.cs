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
        var result = ValidateFile(FFMPEG, file);
        if (result.NoErrors)
            return 2;
        
        args.Logger.WLog(result.Log);
        return 1;
    }

    /// <summary>
    /// Validates a video file for errors using FFmpeg.
    /// </summary>
    /// <param name="ffmpegPath">The path to the FFmpeg executable.</param>
    /// <param name="filename">The path to the video file to be validated.</param>
    /// <returns>
    /// A tuple containing a boolean indicating whether there are no errors (NoErrors)
    /// and a log message (Log) from FFmpeg.
    /// </returns>
    public static (bool NoErrors, string Log) ValidateFile(string ffmpegPath, string filename)
    {
        if (System.IO.File.Exists(ffmpegPath) == false)
            return (false, "FFmpeg does not exist at the specified path.");

        if (System.IO.File.Exists(filename) == false)
            return (false, "The input file does not exist.");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            ArgumentList = { "-v", "error", "-i", filename, "-f", "null", "-" },
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = new Process
        {
            StartInfo = processStartInfo
        };

        process.Start();
        string output = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (output.Contains("error"))
            return (false, output);

        return (true, string.Empty);
    }
}