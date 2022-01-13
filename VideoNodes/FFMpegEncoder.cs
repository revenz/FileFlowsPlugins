namespace FileFlows.VideoNodes
{
    using System.Diagnostics;
    using System.Text;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;

    public class FFMpegEncoder
    {
        private string ffMpegExe;
        private ILogger Logger;

        StringBuilder outputBuilder, errorBuilder;
        TaskCompletionSource<bool> outputCloseEvent, errorCloseEvent;

        private Regex rgxTime = new Regex(@"(?<=(time=))([\d]+:?)+\.[\d]+");

        public delegate void TimeEvent(TimeSpan time);
        public event TimeEvent AtTime;

        private Process process;

        public FFMpegEncoder(string ffMpegExe, ILogger logger)
        {
            this.ffMpegExe = ffMpegExe;
            this.Logger = logger;
        }

        public bool Encode(string input, string output, string arguments, bool dontAddInputFile = false)
        {
            // -y means it will overwrite a file if output already exists
            if(dontAddInputFile == false)
                arguments = $"-i \"{input}\" -y {arguments} \"{output}\"";
            else
                arguments = $"{arguments} \"{output}\"";

            Logger.ILog(new string('=', ("FFMpeg.Arguments: " + arguments).Length));
            Logger.ILog("FFMpeg.Arguments: " + arguments);
            Logger.ILog(new string('=', ("FFMpeg.Arguments: " + arguments).Length));

            var task = ExecuteShellCommand(ffMpegExe, arguments, 0);
            task.Wait();
            Logger.ILog("Exit Code: " + task.Result.ExitCode);
            return task.Result.ExitCode == 0; // exitcode 0 means it was successful
        }

        internal void Cancel()
        {
            try
            {
                if (this.process != null)
                {
                    this.process.Kill();
                    this.process = null;
                }

            }
            catch (Exception) { }
        }

        public async Task<ProcessResult> ExecuteShellCommand(string command, string arguments, int timeout = 0)
        {
            var result = new ProcessResult();

            using (var process = new Process())
            {
                this.process = process;
                // If you run bash-script on Linux it is possible that ExitCode can be 255.
                // To fix it you can try to add '#!/bin/bash' header to the script.

                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                outputBuilder = new StringBuilder();
                outputCloseEvent = new TaskCompletionSource<bool>();

                process.OutputDataReceived += OnOutputDataReceived;

                errorBuilder = new StringBuilder();
                errorCloseEvent = new TaskCompletionSource<bool>();

                process.ErrorDataReceived += OnErrorDataReceived;

                bool isStarted;

                try
                {
                    isStarted = process.Start();
                }
                catch (Exception error)
                {
                    // Usually it occurs when an executable file is not found or is not executable

                    result.Completed = true;
                    result.ExitCode = -1;
                    result.Output = error.Message;

                    isStarted = false;
                }

                if (isStarted)
                {
                    // Reads the output stream first and then waits because deadlocks are possible
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Creates task to wait for process exit using timeout
                    var waitForExit = WaitForExitAsync(process, timeout);

                    // Create task to wait for process exit and closing all output streams
                    var processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);

                    // Waits process completion and then checks it was not completed by timeout
                    if (
                        (
                            (timeout > 0 && await Task.WhenAny(Task.Delay(timeout), processTask) == processTask) ||
                            (timeout == 0 && await Task.WhenAny(processTask) == processTask)
                        )
                         && waitForExit.Result)
                    {
                        result.Completed = true;
                        result.ExitCode = process.ExitCode;

                        // Adds process output if it was completed with error
                        if (process.ExitCode != 0)
                        {
                            result.Output = $"{outputBuilder}{errorBuilder}";
                        }
                    }
                    else
                    {
                        try
                        {
                            // Kill hung process
                            process.Kill();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            process = null;

            return result;
        }
        public void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // The output stream has been closed i.e. the process has terminated
            if (e.Data == null)
            {
                outputCloseEvent.SetResult(true);
            }
            else
            {
                if (e.Data.Contains("Skipping NAL unit"))
                    return; // just slighlty ignore these
                if (rgxTime.IsMatch(e.Data))
                {
                    var timeString = rgxTime.Match(e.Data).Value;
                    var ts = TimeSpan.Parse(timeString);
                    Logger.DLog("TimeSpan Detected: " + ts);
                    if (AtTime != null)
                        AtTime.Invoke(ts);
                }
                Logger.ILog(e.Data);
                outputBuilder.AppendLine(e.Data);
            }
        }

        public void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // The error stream has been closed i.e. the process has terminated
            if (e.Data == null)
            {
                errorCloseEvent.SetResult(true);
            }
            else if (e.Data.ToLower().Contains("failed") || e.Data.Contains("No capable devices found") || e.Data.ToLower().Contains("error"))
            {
                Logger.ELog(e.Data);
                errorBuilder.AppendLine(e.Data);
            }
            else
            {
                if (rgxTime.IsMatch(e.Data))
                {
                    var timeString = rgxTime.Match(e.Data).Value;
                    var ts = TimeSpan.Parse(timeString);
                    if (AtTime != null)
                        AtTime.Invoke(ts);
                }
                Logger.ILog(e.Data);
                outputBuilder.AppendLine(e.Data);
            }
        }


        private static Task<bool> WaitForExitAsync(Process process, int timeout)
        {
            if (timeout > 0)
                return Task.Run(() => process.WaitForExit(timeout));
            return Task.Run(() =>
            {
                process.WaitForExit();
                return Task.FromResult<bool>(true);
            });
        }


        public struct ProcessResult
        {
            public bool Completed;
            public int? ExitCode;
            public string Output;
        }
    }
}