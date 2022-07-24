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

        public (bool successs, string output) Encode(string input, string output, List<string> arguments, bool dontAddInputFile = false, bool dontAddOutputFile = false)
        {
            arguments ??= new List<string> ();

            // -y means it will overwrite a file if output already exists
            if (dontAddInputFile == false) {
                arguments.Insert(0, "-i");
                arguments.Insert(1, input);
                arguments.Insert(2, "-y");
            }

            if (dontAddOutputFile == false)
            {
                if (arguments.Last() != "-")
                {
                    // strict -2 needs to be just before the output file
                    arguments.AddRange(new[] { "-strict", "-2" }); // allow experimental stuff
                    arguments.Add(output);
                }
                else
                    Logger.ILog("Last argument '-' skipping adding output file");
            }

            string argsString = String.Join(" ", arguments.Select(x => x.IndexOf(" ") > 0 ? "\"" + x + "\"" : x));
            Logger.ILog(new string('-', ("FFMpeg.Arguments: " + argsString).Length));
            Logger.ILog("FFMpeg.Arguments: " + argsString);
            Logger.ILog(new string('-', ("FFMpeg.Arguments: " + argsString).Length));

            var task = ExecuteShellCommand(ffMpegExe, arguments, 0);
            task.Wait();
            Logger.ILog("Exit Code: " + task.Result.ExitCode);
            return (task.Result.ExitCode == 0, task.Result.Output); // exitcode 0 means it was successful
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

        public async Task<ProcessResult> ExecuteShellCommand(string command, List<string> arguments, int timeout = 0)
        {
            var result = new ProcessResult();

            using (var process = new Process())
            {
                this.process = process;

                process.StartInfo.FileName = command;
                if (arguments?.Any() == true)
                {
                    foreach (string arg in arguments)
                        process.StartInfo.ArgumentList.Add(arg);
                }
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
                        result.Output = $"{outputBuilder}{errorBuilder}";
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
            else if (e.Data.Contains("Skipping NAL unit"))
            {
                return; // just slighlty ignore these
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