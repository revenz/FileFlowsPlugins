namespace FileFlows.VideoNodes;

/// <summary>
/// Node for checking if Flow Runner has access to hardware
/// </summary>
public class CanUseHardwareEncoding:Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;

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

                    new ListOption { Label = "AMD", Value = "###GROUP###" },
                    new ListOption { Label = "AMD H.264", Value = HardwareEncoder.Amd_H264 },
                    new ListOption { Label = "AMD H.265", Value = HardwareEncoder.Amd_Hevc },

                    new ListOption { Label = "Intel QSV", Value = "###GROUP###" },
                    new ListOption { Label = "Intel QSV H.264", Value = HardwareEncoder.Qsv_H264 },
                    new ListOption { Label = "Intel QSV H.265", Value = HardwareEncoder.Qsv_Hevc },

                    new ListOption { Label = "VAAPI", Value = "###GROUP###" },
                    new ListOption { Label = "VAAPI H.264", Value = HardwareEncoder.Vaapi_H264 },
                    new ListOption { Label = "VAAPI H.265", Value = HardwareEncoder.Vaapi_Hevc },
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

        Nvidia_Hevc = 11,
        Amd_Hevc = 12,
        Qsv_Hevc = 13,
        Vaapi_Hevc = 14,
    }

    public override int Execute(NodeParameters args)
    {
        bool canProcess = false;

        switch (Encoder)
        {
            case HardwareEncoder.Nvidia_H264: canProcess = CanProcess_Nvidia_H264(args); break;
            case HardwareEncoder.Nvidia_Hevc: canProcess = CanProcess_Nvidia_Hevc(args); break;

            case HardwareEncoder.Amd_H264: canProcess = CanProcess_Amd_H264(args); break;
            case HardwareEncoder.Amd_Hevc: canProcess = CanProcess_Amd_Hevc(args); break;

            case HardwareEncoder.Qsv_H264: canProcess = CanProcess_Qsv_H264(args); break;
            case HardwareEncoder.Qsv_Hevc: canProcess = CanProcess_Qsv_Hevc(args); break;

            case HardwareEncoder.Vaapi_H264: canProcess = CanProcess_Vaapi_H264(args); break;
            case HardwareEncoder.Vaapi_Hevc: canProcess = CanProcess_Vaapi_Hevc(args); break;
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
    /// Checks if this flow runner can use NVIDIA H.264 encoder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>true if can use it, otherwise false</returns>
    internal static bool CanProcess_Nvidia_H264(NodeParameters args) => CanProcess(args, "h264_nvenc");

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
    internal static bool CanProcess_Qsv_Hevc(NodeParameters args) => CanProcess(args, "hevc_qsv -global_quality 28 -load_plugin hevc_hw");

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

    private static bool CanProcess(NodeParameters args, string encodingParams)
    {
        string ffmpeg = args.GetToolPath("FFMpeg");
        if (string.IsNullOrEmpty(ffmpeg))
        {
            args.Logger.ELog("FFMpeg tool not found.");
            return false;
        }
        return CanProcess(args, ffmpeg, encodingParams);
    }

    internal static bool CanProcess(NodeParameters args, string ffmpeg, string encodingParams)
    {
        string cmdArgs = $"-loglevel error -f lavfi -i color=black:s=1080x1080 -vframes 1 -an -c:v {encodingParams} -f null -\"";
        var cmd = args.Process.ExecuteShellCommand(new ExecuteArgs
        {
            Command = ffmpeg,
            Arguments = cmdArgs,
            Silent = true
        }).Result;
        if (cmd.ExitCode != 0 || string.IsNullOrWhiteSpace(cmd.Output) == false)
        {
            args.Logger?.WLog($"Cant process '{encodingParams}': {cmd.Output ?? ""}");
            return false;
        }
        return true;
    }
}
