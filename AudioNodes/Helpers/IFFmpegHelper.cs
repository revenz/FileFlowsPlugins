using System.Diagnostics;

namespace FileFlows.AudioNodes.Helpers;

/// <summary>
/// Interface for FFmpeg Helper
/// </summary>
public interface IFFmpegHelper
{
    /// <summary>
    /// Reads the info for a file
    /// </summary>
    /// <param name="file">the path to the file to read</param>
    /// <returns>the file output</returns>
    Result<string> ReadFFmpeg(string file);
    
    /// <summary>
    /// Reads the info for a file from FFprobe
    /// </summary>
    /// <param name="file">the path to the file to read</param>
    /// <returns>the file output</returns>
    Result<string> ReadFFprobe(string file);
}

/// <summary>
/// FFmpeg Helper
/// </summary>
/// <param name="ffmpeg">the FFmpeg executable</param>
/// <param name="ffprobe">the FFprobe executable</param>
public class FFmpegHelper(string ffmpeg, string ffprobe) : IFFmpegHelper
{
    /// <inheritdoc />
    public Result<string> ReadFFmpeg(string file)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = ffmpeg,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                ArgumentList = {
                    "-hide_banner", 
                    "-i",
                    file
                }
            };

            process.Start();

            // Read asynchronously from both output and error streams
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            bool exited = process.WaitForExit(60000);
            if (!exited)
            {
                process.Kill();
                string pkOutput = errorTask.Result.EmptyAsNull() ?? outputTask.Result;
                return Result<string>.Fail("Process timed out." + Environment.NewLine + pkOutput);
            }

            // Await both the output and error tasks
            string output = outputTask.Result;
            string error = errorTask.Result;

            return output?.EmptyAsNull() ?? error;
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"An error occurred: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Result<string> ReadFFprobe(string file)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = ffprobe,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                ArgumentList = {
                    "-v", "error",
                    "-select_streams", "a:0",
                    "-show_format",
                    "-of", "json",
                    file
                }
            };

            process.Start();

            // Read asynchronously from both output and error streams
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            bool exited = process.WaitForExit(60000);
            if (!exited)
            {
                process.Kill();
                string pkOutput = errorTask.Result.EmptyAsNull() ?? outputTask.Result;
                return Result<string>.Fail("Process timed out." + Environment.NewLine + pkOutput);
            }

            // Await both the output and error tasks
            string output = outputTask.Result;
            string error = errorTask.Result;

            if (string.IsNullOrEmpty(error) == false)
                return Result<string>.Fail($"Failed reading ffmpeg info: {error}");

            return output;
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"An error occurred: {ex.Message}");
        }
    }

} 