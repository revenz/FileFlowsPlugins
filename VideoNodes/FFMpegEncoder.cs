using System.Diagnostics;
using System.Text;

namespace FileFlows.VideoNodes;
public class FFMpegEncoder
{
    private string ffMpegExe;
    private ILogger Logger;

    StringBuilder outputBuilder = new (), errorBuilder = new ();
    TaskCompletionSource<bool> outputCloseEvent, errorCloseEvent;

    private Regex rgxTime = new Regex(@"(?<=(time=))([\d]+:?)+\.[\d]+");
    private Regex rgxFps = new Regex(@"(?<=(fps=[\s]?))([\d]+(\.[\d]+)?)");
    private Regex rgxSpeed = new Regex(@"(?<=(speed=[\s]?))([\d]+(\.[\d]+)?)");
    private Regex rgxBitrate = new Regex(@"(?<=(bitrate=[\s]?))([\d]+(\.[\d]+)?)kbits");

    public delegate void TimeEvent(TimeSpan time, DateTime startedAt);
    public event TimeEvent AtTime;
    public delegate void StatChange(string name, string value, bool recordStatistic = false);
    public event StatChange OnStatChange;

    private Process process;
    DateTime startedAt;
    private string? AbortReason;
    private CancellationToken _cancellationToken;
    private ProcessHelper _processHelper;

    public FFMpegEncoder(string ffMpegExe, ILogger logger, CancellationToken cancellationToken)
    {
        this.ffMpegExe = ffMpegExe;
        this.Logger = logger;
        _processHelper = new(logger, cancellationToken, false);
        _processHelper.OnStandardOutputReceived += OnOutputDataReceived;
        _processHelper.OnErrorOutputReceived += OnErrorDataReceived;
        _cancellationToken = cancellationToken;
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
    public (bool successs, string output, string? abortReason) Encode(string input, string output, List<string> arguments, bool dontAddInputFile = false, bool dontAddOutputFile = false, string strictness = "-2")
    {
        arguments ??= [];
        if (string.IsNullOrWhiteSpace(strictness))
            strictness = "-2";
        

        // -y means it will overwrite a file if output already exists
        if (dontAddInputFile == false) {
            arguments.Insert(0, "-i");
            arguments.Insert(1, input);
            arguments.Insert(2, "-y");
        }

        if (arguments.Any(x => x.Equals("webvtt", StringComparison.InvariantCultureIgnoreCase)))
        {
            arguments.Insert(0, "-c:s");
            arguments.Insert(1, "webvtt");
        }

        if (dontAddOutputFile == false)
        {
            if (arguments.Last() != "-")
            {
                arguments.AddRange(["-metadata", "comment=Created by FileFlows\nhttps://fileflows.com"]);
                // strict -2 needs to be just before the output file
                arguments.AddRange(["-strict", strictness]); 
                arguments.Add(output);
            }
            else
                Logger.ILog("Last argument '-' skipping adding output file");
        }

        string argsString = string.Join(" ", arguments.Select(x => x.IndexOf(' ') > 0 ? "\"" + x + "\"" : x));
        Logger.ILog(new string('-', ("FFmpeg.Arguments: " + argsString).Length));
        Logger.ILog("FFmpeg.Arguments: " + argsString);
        Logger.ILog(new string('-', ("FFmpeg.Arguments: " + argsString).Length));

        var task = ExecuteShellCommand(ffMpegExe, arguments, 0);
        task.Wait(_cancellationToken);
        Logger.ILog("Exit Code: " + task.Result.ExitCode);
        Logger.ILog("Completed: " + task.Result.Completed);
        if (task.Result.Completed && string.IsNullOrEmpty(AbortReason))
            AbortReason = "Process was terminated early";
        return (task.Result is { ExitCode: 0, Completed: true }, task.Result.Output, task.Result.AbortReason); // exitcode 0 means it was successful
    }

    /// <summary>
    /// Aborts the file process
    /// </summary>
    /// <param name="reason">the reason for the abort</param>
    private void Abort(string reason)
    {
        this.AbortReason = reason;
        Cancel();
    }

    /// <summary>
    /// Cancels the process
    /// </summary>
    internal void Cancel()
    {
        try
        {
            _processHelper.Kill();
        }
        catch (Exception)
        {
            // iGNORED
        }
    }

    public async Task<ProcessResult> ExecuteShellCommand(string command, List<string> arguments, int timeout = 0)
    {
        var hwDecoderIndex = arguments.FindIndex(x => x == "-hwaccel");
        string? decoder = null;
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
        else
        {
            var index = arguments.FindIndex(x => x.Contains("-c:v:"));
            if (index > 0 && index < arguments.Count - 2)
            {
                if(arguments[index + 1] != "copy")
                    decoder = "CPU";
                else
                    OnStatChange?.Invoke("Decoder", "Copy", recordStatistic: false);
            }
        }

        if (decoder != null)
        {
            Logger?.ILog("Decoder: " + decoder);
            OnStatChange?.Invoke("Decoder", decoder, recordStatistic: true);
        }

        string? encoder = null;
        if (arguments.Any(x =>
                x.Contains("hevc_qsv", StringComparison.InvariantCultureIgnoreCase) ||
                x.Contains("h264_qsv", StringComparison.InvariantCultureIgnoreCase) ||
                x.Contains("av1_qsv", StringComparison.InvariantCultureIgnoreCase)))
            encoder = "QSV";
        else if (arguments.Any(x => x.Contains("_nvenc", StringComparison.InvariantCultureIgnoreCase)))
            encoder = "NVIDIA";
        else if (arguments.Any(x => x.Contains("_amf", StringComparison.InvariantCultureIgnoreCase)))
            encoder = "AMF";
        else if (arguments.Any(x => x.Contains("_vaapi", StringComparison.InvariantCultureIgnoreCase)))
            encoder = "VAAPI";
        else if (arguments.Any(x => x.Contains("_videotoolbox", StringComparison.InvariantCultureIgnoreCase)))
            encoder = "VideoToolbox";
        else if (arguments.Any(x => x.Contains("libx", StringComparison.InvariantCultureIgnoreCase) || x.Contains("libvpx", StringComparison.InvariantCultureIgnoreCase)))
            encoder = "CPU";

        if (encoder != null)
        {
            Logger?.ILog("Encoder: " + encoder);
            OnStatChange?.Invoke("Encoder", encoder, recordStatistic: true);
        }

        var processHelper = new ProcessHelper(Logger, _cancellationToken, false);
        processHelper.OnStandardOutputReceived += OnOutputDataReceived;
        processHelper.OnErrorOutputReceived += OnErrorDataReceived;
        startedAt = DateTime.Now;
        var result = processHelper.ExecuteShellCommand(new()
        {
            Command = command,
            Silent = true,
            ArgumentList = arguments.ToArray()
        }).GetAwaiter().GetResult();
        
        return new()
        {
            Completed = result.Completed,
            ExitCode = result.ExitCode,
            Output = result.Output,
            AbortReason = AbortReason?.EmptyAsNull()
        };
    }
    
    private void OnOutputDataReceived(string data)
    {
        // The output stream has been closed i.e. the process has terminated
        if (data == null)
        {
            outputCloseEvent.SetResult(true);
            return;
        }
        CheckOutputLine(data);
    }

    private void OnErrorDataReceived(string data)
    {
        // The error stream has been closed i.e. the process has terminated
        if (data == null)
        {
            errorCloseEvent.SetResult(true);
            return;
        }
        
        if (data.ToLower().Contains("failed") || data.Contains("No capable devices found") || data.ToLower().Contains("error"))
        {
            Logger.ELog(data);
            errorBuilder.AppendLine(data);
        }
        else
        {
            CheckOutputLine(data);
        }
    }

    private int ErrorCount = 0;

    private void CheckOutputLine(string line)
    {
        if (line.Contains("Skipping NAL unit"))
            return; // just slightly ignore these
        if (line.Contains("Last message repeated"))
            return;

        if (line.Contains("error ", StringComparison.InvariantCultureIgnoreCase))
        {
            if (++ErrorCount > 20)
            {
                // Abort
                Logger.ELog("Maximum number of errors triggered: " + line);
                Abort(line);
            }
        }
        
        if (rgxTime.IsMatch(line))
        {
            var timeString = rgxTime.Match(line).Value;
            if(TimeSpan.TryParse(timeString, out var ts))
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

    /// <summary>
    /// Represents the result of a process execution.
    /// </summary>
    public struct ProcessResult
    {
        /// <summary>
        /// Indicates whether the process completed successfully.
        /// </summary>
        public bool Completed;

        /// <summary>
        /// The exit code of the process, if available.
        /// </summary>
        public int? ExitCode;

        /// <summary>
        /// The standard output of the process.
        /// </summary>
        public string Output;

        /// <summary>
        /// The reason for process abortion, if the process was aborted.
        /// </summary>
        public string AbortReason;
    }

}