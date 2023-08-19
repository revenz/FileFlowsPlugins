using FileFlows.VideoNodes.FfmpegBuilderNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlows.VideoNodes.VideoNodes;

/// <summary>
/// Node that converts a audio file into a video file and generates a video based on the audio
/// </summary>
public class AudioToVideo : EncodingNode
{
    public override int Outputs => 1;
    public override int Inputs => 1;
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/audio-to-video";

    public override string Icon => "fas fa-headphones";

    public enum VisualizationStyle
    {
        Waves = 1,
        AudioVectorScope = 2,
        Spectrum = 3
    }

    [DefaultValue(VisualizationStyle.Waves)]
    [Select(nameof(VisualizationOptions), 1)]
    public VisualizationStyle Visualization { get; set; }

    private static List<ListOption> _VisualisationOptions;
    public static List<ListOption> VisualizationOptions
    {
        get
        {
            if (_VisualisationOptions == null)
            {
                _VisualisationOptions = new List<ListOption>
                    {
                        new ListOption { Label = "Waves", Value = VisualizationStyle.Waves },
                        new ListOption { Label = "Audio Vector Scope", Value = VisualizationStyle.AudioVectorScope },
                        new ListOption { Label = "Spectrum", Value = VisualizationStyle.Spectrum },
                    };
            }
            return _VisualisationOptions;
        }
    }


    [DefaultValue("mkv")]
    [Select(nameof(ContainerOptions), 2)]
    public string Container { get; set; } = string.Empty;

    private static List<ListOption> _ContainerOptions;
    public static List<ListOption> ContainerOptions
    {
        get
        {
            if (_ContainerOptions == null)
            {
                _ContainerOptions = new List<ListOption>
                    {
                        new ListOption { Label = "MKV", Value = "mkv"},
                        new ListOption { Label = "MP4", Value = "mp4"}
                    };
            }
            return _ContainerOptions;
        }
    }

    [DefaultValue("1280x720")]
    [Select(nameof(ResolutionOptions), 3)]
    public string Resolution { get; set; }

    private static List<ListOption> _ResolutionOptions;
    public static List<ListOption> ResolutionOptions
    {
        get
        {
            if (_ResolutionOptions == null)
            {
                _ResolutionOptions = new List<ListOption>
                    {
                        new ListOption { Label = "480p", Value = "640x480"},
                        new ListOption { Label = "720p", Value = "1280x720"},
                        new ListOption { Label = "1080p", Value = "1920x1080"},
                        new ListOption { Label = "4K", Value = "3840x2160"}
                    };
            }
            return _ResolutionOptions;
        }
    }

    [DefaultValue(FfmpegBuilderVideoEncode.CODEC_H264)]
    [Select(nameof(CodecOptions), 4)]
    public string Codec { get; set; }

    private static List<ListOption> _CodecOptions;
    /// <summary>
    /// Gets or sets the codec options
    /// </summary>
    public static List<ListOption> CodecOptions
    {
        get
        {
            if (_CodecOptions == null)
            {
                _CodecOptions = new List<ListOption>
                {
                    new () { Label = "H.264", Value = FfmpegBuilderVideoEncode.CODEC_H264 },
                    //new () { Label = "H.264 (10-Bit)", Value = FfmpegBuilderVideoEncode.CODEC_H264_10BIT },
                    new () { Label = "H.265", Value = FfmpegBuilderVideoEncode.CODEC_H265 },
                    new () { Label = "H.265 (10-Bit)", Value = FfmpegBuilderVideoEncode.CODEC_H265_10BIT },
                };
            }
            return _CodecOptions;
        }
    }

    [Boolean(5)]
    [DefaultValue(true)]
    public bool HardwareEncoding { get; set; }

    [TextVariable(6)]
    [DefaultValue("#ff0090")]
    [ConditionEquals(nameof(Visualization), VisualizationStyle.Waves)]
    public string Color { get; set; }

    public override int Execute(NodeParameters args)
    {
        List<string> ffArgs = new List<string>();

        bool useHardwareEncoding = HardwareEncoding;
        if (Environment.GetEnvironmentVariable("HW_OFF") == "1")
            useHardwareEncoding = false;
        
        var encodingParameters = FfmpegBuilderVideoEncode.GetEncodingParameters(args, this.Codec, 28, useHardwareEncoding ? string.Empty : "CPU", 29.97f, speed: "fast");

        if (Container.ToLower() == "mp4")
            ffArgs.AddRange(new[] { "-movflags", "+faststart" });

        switch (Visualization)
        {
            case VisualizationStyle.Waves:
                var color = this.Color;
                if (Regex.IsMatch(color ?? String.Empty, "#[0-9a-fA-F]{6}") == false)
                    color = "#ff0090"; // use default colour
                ffArgs.AddRange(new[] { "-filter_complex", $"[0:a]showwaves=s={Resolution}:mode=line:s=hd1080:colors={color}[v]" });
                break;
            case VisualizationStyle.AudioVectorScope:
                ffArgs.AddRange(new[] { "-filter_complex", $"[0:a]avectorscope=s={Resolution}[v]" });
                break;
            case VisualizationStyle.Spectrum:
                ffArgs.AddRange(new[] { "-filter_complex", $"[0:a]showspectrum=s={Resolution}:mode=separate:color=intensity:slide=1:scale=cbrt[v]" });
                break;
        }
        ffArgs.AddRange(new[] { "-map", "[v]" });
        ffArgs.Add("-vcodec");
        ffArgs.AddRange(encodingParameters);
        ffArgs.AddRange(new[] { "-map", "0:a" });
        if (base.Encode(args, base.FFMPEG, ffArgs, this.Container) == false)
        {
            args.Logger?.ELog("Failed to encode");
            return -1;
        }
        return 1;
    }
}
