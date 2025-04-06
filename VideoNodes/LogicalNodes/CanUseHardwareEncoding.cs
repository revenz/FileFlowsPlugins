using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes;

/// <summary>
/// Node for checking if Flow Runner has access to hardware
/// </summary>
public class CanUseHardwareEncoding:Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;

    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/can-use-hardware-encoding";

    public override string Icon => "fas fa-eye";

    public override FlowElementType Type => FlowElementType.Logic;

    [Select(nameof(EncoderOptions), 1)]
    public HardwareEncoder Encoder { get; set; }

    private static List<ListOption> _EncoderOptions;
    public static List<ListOption> EncoderOptions
    {
        get
        {
            if (_EncoderOptions == null)
            {
                _EncoderOptions = new List<ListOption>
                {
                    new ListOption { Label = "NVIDIA", Value = "###GROUP###" },
                    new ListOption { Label = "NVIDIA H.264", Value = HardwareEncoder.Nvidia_H264 },
                    new ListOption { Label = "NVIDIA H.265", Value = HardwareEncoder.Nvidia_Hevc },
                    new ListOption { Label = "NVIDIA AV1", Value = HardwareEncoder.Nvidia_Hevc },

                    new ListOption { Label = "AMD", Value = "###GROUP###" },
                    new ListOption { Label = "AMD H.264", Value = HardwareEncoder.Amd_H264 },
                    new ListOption { Label = "AMD H.265", Value = HardwareEncoder.Amd_Hevc },

                    new ListOption { Label = "Intel QSV", Value = "###GROUP###" },
                    new ListOption { Label = "Intel QSV H.264", Value = HardwareEncoder.Qsv_H264 },
                    new ListOption { Label = "Intel QSV H.265", Value = HardwareEncoder.Qsv_Hevc },

                    new ListOption { Label = "VAAPI", Value = "###GROUP###" },
                    new ListOption { Label = "VAAPI H.264", Value = HardwareEncoder.Vaapi_H264 },
                    new ListOption { Label = "VAAPI H.265", Value = HardwareEncoder.Vaapi_Hevc },

                    new ListOption { Label = "VideoToolbox (MacOS)", Value = "###GROUP###" },
                    new ListOption { Label = "VideoToolbox H.264", Value = HardwareEncoder.VideoToolbox_H264},
                    new ListOption { Label = "VideoToolbox H.265", Value = HardwareEncoder.VideoToolbox_Hevc },
                };
            }
            return _EncoderOptions;
        }
    }

    public enum HardwareEncoder
    {
        Nvidia_H264 = 1,
        Amd_H264 = 2,
        Qsv_H264 = 3,
        Vaapi_H264 = 4,
        VideoToolbox_H264 = 5,

        Nvidia_Hevc = 11,
        Amd_Hevc = 12,
        Qsv_Hevc = 13,
        Vaapi_Hevc = 14,
        VideoToolbox_Hevc = 15,
        
        Nvidia_AV1 = 21,
        Amd_AV1 = 22,
        Qsv_AV1 = 23,
    }

    public override int Execute(NodeParameters args)
    {
        bool canProcess = false;

        switch (Encoder)
        {
            case HardwareEncoder.Nvidia_H264: canProcess = CanProcess_Nvidia_H264(args); break;
            case HardwareEncoder.Nvidia_Hevc: canProcess = CanProcess_Nvidia_Hevc(args); break;
            case HardwareEncoder.Nvidia_AV1: canProcess = CanProcess_Nvidia_AV1(args); break;

            case HardwareEncoder.Amd_H264: canProcess = CanProcess_Amd_H264(args); break;
            case HardwareEncoder.Amd_Hevc: canProcess = CanProcess_Amd_Hevc(args); break;
            case HardwareEncoder.Amd_AV1: canProcess = CanProcess_Amd_AV1(args); break;

            case HardwareEncoder.Qsv_H264: canProcess = CanProcess_Qsv_H264(args); break;
            case HardwareEncoder.Qsv_Hevc: canProcess = CanProcess_Qsv_Hevc(args); break;

            case HardwareEncoder.Vaapi_H264: canProcess = CanProcess_Vaapi_H264(args); break;
            case HardwareEncoder.Vaapi_Hevc: canProcess = CanProcess_Vaapi_Hevc(args); break;

            case HardwareEncoder.VideoToolbox_H264: canProcess = CanProcess_VideoToolbox_H264(args); break;
            case HardwareEncoder.VideoToolbox_Hevc: canProcess = CanProcess_VideoToolbox_Hevc(args); break;
        }

        return canProcess ? 1 : 2;
    }

    /// <summary>
    /// Checks if this flow runner can use NVIDIA HEVC encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Nvidia_Hevc(NodeParameters args) => CanProcess(args, "hevc_nvenc");
    
    /// <summary>
    /// Checks if this flow runner can use NVIDIA AV1 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Nvidia_AV1(NodeParameters args) => CanProcess(args, "av1_nvenc");
    
    /// <summary>
    /// Checks if this flow runner can use QSV AV1 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Qsv_AV1(NodeParameters args) => CanProcess(args, "av1_qsv");

    /// <summary>
    /// Checks if this flow runner can use NVIDIA H.264 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Nvidia_H264(NodeParameters args) => CanProcess(args, "h264_nvenc");

    /// <summary>
    /// Checks if this flow runner can use VideoToolbox HEVC encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_VideoToolbox_Hevc(NodeParameters args) => CanProcess(args, "hevc_videotoolbox");

    /// <summary>
    /// Checks if this flow runner can use VideoToolbox H.264 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_VideoToolbox_H264(NodeParameters args) => CanProcess(args, "h264_videotoolbox");
    
    /// <summary>
    /// Checks if this flow runner can use AND AV1 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Amd_AV1(NodeParameters args) => CanProcess(args, "hevc_av1");

    /// <summary>
    /// Checks if this flow runner can use AND HEVC encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Amd_Hevc(NodeParameters args) => CanProcess(args, "hevc_amf");

    /// <summary>
    /// Checks if this flow runner can use AND H.264 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Amd_H264(NodeParameters args) => CanProcess(args, "h264_amf");


    /// <summary>
    /// Checks if this flow runner can use Intels QSV HEVC encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Qsv_Hevc(NodeParameters args) => CanProcess(args, "hevc_qsv", "-global_quality", "28", "-load_plugin", "hevc_hw");

    /// <summary>
    /// Checks if this flow runner can use Intels QSV H.264 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Qsv_H264(NodeParameters args) => CanProcess(args, "h264_qsv");

    /// <summary>
    /// Checks if this flow runner can use VAAPI HEVC encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Vaapi_Hevc(NodeParameters args) => CanProcess(args, "hevc_vaapi");

    /// <summary>
    /// Checks if this flow runner can use VAAPI H.264 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Vaapi_H264(NodeParameters args) => CanProcess(args, "h264_vaapi");


    /// <summary>
    /// Gets if a encoder/decoder has been disabled by a variable
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="parameters">the parameters to check</param>
    /// <returns>if a encoder/decoder has been disabled by a variable</returns>
    internal static bool DisabledByVariables(NodeParameters args, string[] parameters)
    {
        if (parameters.Any(x => x.ToLower().Contains("nvenc")))
        {
            if (args.Variables.FirstOrDefault(x => x.Key.ToLowerInvariant() == "nonvidia").Value as bool? == true)
                return true;
        }
        else if (parameters.Any(x => x.ToLower().Contains("qsv")))
        {
            if (args.Variables.FirstOrDefault(x => x.Key.ToLowerInvariant() == "noqsv").Value as bool? == true)
                return true;
        }
        else if (parameters.Any(x => x.ToLower().Contains("vaapi")))
        {
            if (args.Variables.FirstOrDefault(x => x.Key.ToLowerInvariant() == "novaapi").Value as bool? == true)
                return true;
        }
        else if (parameters.Any(x => x.ToLower().Contains("amf")))
        {
            if (args.Variables.FirstOrDefault(x => x.Key.ToLowerInvariant() == "noamf" || x.Key.ToLowerInvariant() == "noamd").Value as bool? == true)
                return true;
        }
        else if (parameters.Any(x => x.ToLower().Contains("videotoolbox")))
        {
            if (args.Variables.FirstOrDefault(x => x.Key.ToLowerInvariant() == "novideotoolbox").Value as bool? == true)
                return true;
        }
        return false;
    }

    private static bool CanProcess(NodeParameters args, params string[] encodingParams)
    {
        if (encodingParams.Any(x => x.Contains("vaapi") && OperatingSystem.IsWindows()))
            return false; // dont bother checking vaapi on windows, unlikely the user has added support for it
        
        if (DisabledByVariables(args, encodingParams))
            return false;

        string ffmpeg = args.GetToolPath("FFmpeg");
        if (string.IsNullOrEmpty(ffmpeg))
        {
            args.Logger.ELog("FFmpeg tool not found.");
            return false;
        }

        return CanProcess(args, ffmpeg, encodingParams);
    }

    /// <summary>
    /// Tests if the encoding parameters can be executed
    /// </summary>
    /// <param name="args">the node paramterse</param>
    /// <param name="ffmpeg">the location of ffmpeg</param>
    /// <param name="encodingParams">the encoding parameter to test</param>
    /// <returns>true if can be processed</returns>
    internal static bool CanProcess(NodeParameters args, string ffmpeg, string[] encodingParams)
    {
        bool can = CanExecute(args, ffmpeg, encodingParams);
        if (can == false && encodingParams?.Contains("amf") == true)
        {
            // AMD/AMF has a issue where it reports false at first but then passes
            // https://github.com/revenz/FileFlows/issues/106
            Thread.Sleep(2000);
            can = CanExecute(args, ffmpeg, encodingParams);
        }
        return can;
    }
    
    private static bool CanExecute(NodeParameters args, string ffmpeg, string[] encodingParams)
    {
        bool vaapi = encodingParams.Any(x => x.Contains("vaapi")) && VaapiHelper.VaapiLinux;
        List<string> arguments = encodingParams.ToList();
        if (vaapi)
            arguments.AddRange(new [] { "-vf", "format=nv12,hwupload", "-strict", "-2"});
        arguments.InsertRange(0, new []
        {
            "-loglevel",
            "error",
            "-f",
            "lavfi",
            "-i",
            "color=black:s=1080x1080",
            "-vframes",
            "1",
            "-an",
            "-c:v"
        });
        if (vaapi)
        {
            arguments.InsertRange(0,
                new[] { "-fflags", "+genpts", "-vaapi_device", VaapiHelper.VaapiRenderDevice });
            arguments.Add(FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".mkv"));
        }
        else
        {
                
            arguments.AddRange(new []
            {
                "-f",
                "null",
                "-"
            });
        }
        var cmd = args.Process.ExecuteShellCommand(new ExecuteArgs
        {
            Command = ffmpeg,
            ArgumentList = arguments.ToArray(),
            Silent = true
        }).Result;
        args.Logger?.ILog(new string ('=', 100) + "\n\nOutput:\n" + cmd.Output + "\n\n" + new string('=', 100));
        args.Logger?.ILog(new string ('=', 100) + "\n\nStandardOutput:\n" + cmd.StandardOutput + "\n\n" + new string('=', 100));
        args.Logger?.ILog(new string ('=', 100) + "\n\nStandardError:\n" + cmd.StandardError + "\n\n" + new string('=', 100));
        string? output = cmd.Output?.Contains("va_openDriver() returns 0") == true ? null : cmd.Output;
        if (cmd.ExitCode != 0 || string.IsNullOrWhiteSpace(output) == false)
        {
            string asStr = string.Join(" ", arguments.Select(x => x.Contains(" ") ? "\"" + x + "\"" : x));
            args.Logger?.WLog($"Cant process '{ffmpeg} {asStr}': {cmd.Output ?? ""}");
            return false;
        }
        return true;
    }
}
