using System.Diagnostics;
using System.Text;

namespace FileFlows.VideoNodes;
public class FFMpegEncoder
{
    private string ffMpegExe;
    private ILogger Logger;

    StringBuilder outputBuilder, errorBuilder;
    TaskCompletionSource<bool> outputCloseEvent, errorCloseEvent;

    private Regex rgxTime = new Regex(@"(?<=(time=))([\d]+:?)+\.[\d]+");
    private Regex rgxFps = new Regex(@"(?<=(fps=[\s]?))([\d]+(\.[\d]+)?)");
    private Regex rgxSpeed = new Regex(@"(?<=(speed=[\s]?))([\d]+(\.[\d]+)?)");
    private Regex rgxBitrate = new Regex(@"(?<=(bitrate=[\s]?))([\d]+(\.[\d]+)?)kbits");

    public delegate void TimeEvent(TimeSpan time, DateTime startedAt);
    public event TimeEvent AtTime;
    public delegate void StatChange(string name, object value, bool recordStatistic = false);
    public event StatChange OnStatChange;

    private Process process;
    DateTime startedAt;

    public FFMpegEncoder(string ffMpegExe, ILogger logger)
    {
        this.ffMpegExe = ffMpegExe;
        this.Logger = logger;
    }

    /// <summary>
    /// Encodes using FFmpeg
    /// </summary>
    /// <param name="input">the input file</param>
    /// <param name="output">the output file</param>
    /// <param name="arguments">the FFmpeg arguments</param>
    /// <param name="dontAddInputFile">if the input file should not be added to the arguments</param>
    /// <param name="dontAddOutputFile">if the output file should not be added to the arguments</param>
    /// <param name="strictness">the strictness to use</param>
    /// <returns>the result and output of the encode</returns>
    public (bool successs, string output) Encode(string input, string output, List<string> arguments, bool dontAddInputFile = false, bool dontAddOutputFile = false, string strictness = "-2")
    {
        arguments ??= new List<string> ();
        if (string.IsNullOrWhiteSpace(strictness))
            strictness = "-2";

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
                arguments.AddRange(new string[]
                    { "-metadata", "comment=Created by FileFlows\nhttps://fileflows.com" });
                // strict -2 needs to be just before the output file
                arguments.AddRange(new[] { "-strict", strictness }); 
                arguments.Add(output);
            }
            else
                Logger.ILog("Last argument '-' skipping adding output file");
        }

        string argsString = String.Join(" ", arguments.Select(x => x.IndexOf(" ") > 0 ? "\"" + x + "\"" : x));
        Logger.ILog(new string('-', ("FFmpeg.Arguments: " + argsString).Length));
        Logger.ILog("FFmpeg.Arguments: " + argsString);
        Logger.ILog(new string('-', ("FFmpeg.Arguments: " + argsString).Length));

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

        var hwDecoderIndex = arguments.FindIndex(x => x == "-hwaccel");
        string decoder = null;
        if (hwDecoderIndex >= 0 && hwDecoderIndex < arguments.Count - 2)
        {
            var decoder2 = arguments[hwDecoderIndex + 1].ToLowerInvariant();
            foreach(var dec in new []
                    {
                        ("qsv", "QSV"), ("cuda", "NVIDIA"), ("amf", "AMD"), ("vulkan", "Vulkan"), ("vaapi", "VAAPI"), ("dxva2", "dxva2"),
                        ("d3d11va", "d3d11va"), ("opencl", "opencl")
                    })
            {
                if (decoder2 == dec.Item1)
                {
                    decoder = dec.Item2;
                    break;
                }
            }
        }
        else if(arguments.Any(x => x.Contains(":v:")))
        {
            decoder = "CPU";
        }

        if (decoder != null)
        {
            Logger?.ILog("Decoder: " + decoder);
            OnStatChange?.Invoke("Decoder", decoder, recordStatistic: true);
        }

        string encoder = null;
        if (arguments.Any(x =>
                x.ToLowerInvariant().Contains("hevc_qsv") || x.ToLowerInvariant().Contains("h264_qsv") ||
                x.ToLowerInvariant().Contains("av1_qsv")))
            encoder = "QSV";
        else if (arguments.Any(x => x.ToLowerInvariant().Contains("_nvenc")))
            encoder = "NVIDIA";
        else if (arguments.Any(x => x.ToLowerInvariant().Contains("_amf")))
            encoder = "AMF";
        else if (arguments.Any(x => x.ToLowerInvariant().Contains("_vaapi")))
            encoder = "VAAPI";
        else if (arguments.Any(x => x.ToLowerInvariant().Contains("_videotoolbox")))
            encoder = "VideoToolbox";
        else if (arguments.Any(x => x.ToLowerInvariant().Contains("libx") || x.ToLowerInvariant().Contains("libvpx")))
            encoder = "CPU";

        if (encoder != null)
        {
            Logger?.ILog("Encoder: " + encoder);
            OnStatChange?.Invoke("Encoder", encoder, recordStatistic: true);
        }

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

            startedAt = DateTime.Now;
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
            CheckOutputLine(e.Data);
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
            CheckOutputLine(e.Data);
        }
    }

    private void CheckOutputLine(string line)
    {
        if (line.Contains("Skipping NAL unit"))
            return; // just slightly ignore these
        
        if (rgxTime.IsMatch(line))
        {
            var timeString = rgxTime.Match(line).Value;
            var ts = TimeSpan.Parse(timeString);
            AtTime?.Invoke(ts, startedAt);
        }

        if (line.Contains("Exit Code"))
        {
            OnStatChange?.Invoke("Speed", null);
            OnStatChange?.Invoke("Bitrate", null);
            OnStatChange?.Invoke("FPS", null);
        }
        else
        {
            if (rgxSpeed.TryMatch(line, out Match matchSpeed))
                OnStatChange?.Invoke("Speed", matchSpeed.Value);

            if (rgxBitrate.TryMatch(line, out Match matchBitrate))
                OnStatChange?.Invoke("Bitrate", matchBitrate.Value);
            
            if (rgxFps.TryMatch(line, out Match matchFps))
                OnStatChange?.Invoke("FPS", matchFps.Value);
        }
        
        Logger.ILog(line);
        outputBuilder.AppendLine(line);
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