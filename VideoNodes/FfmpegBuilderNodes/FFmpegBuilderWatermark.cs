using System.Globalization;
using FileFlows.Plugin.Types;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that adds a watermark to a video file
/// </summary>
public class FFmpegBuilderWatermark: FfmpegBuilderNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/watermark";
    /// <inheritdoc />
    public override string Icon => "far fa-copyright";
    /// <inheritdoc />
    public override LicenseLevel LicenseLevel => LicenseLevel.Enterprise;

    /// <summary>
    /// Gets or sets the watermark image
    /// </summary>
    [File(1)]
    [Required]
    public string Image { get; set; }
    
    /// <summary>
    /// Gets or sets the position
    /// </summary>
    [Select(nameof(PositionOptions), 2)]
    [DefaultValue(WatermarkPosition.BottomRight)]
    public WatermarkPosition Position { get; set; }

    private static List<ListOption> _PositionOptions;
    /// <summary>
    /// Gets the available position options
    /// </summary>
    public static List<ListOption> PositionOptions
    {
        get
        {
            if (_PositionOptions == null)
            {
                _PositionOptions = new ()
                {
                    new () { Label = $"Enums.{nameof(WatermarkPosition)}.{nameof(WatermarkPosition.Center)}", Value = WatermarkPosition.Center },
                    new () { Label = $"Enums.{nameof(WatermarkPosition)}.{nameof(WatermarkPosition.TopLeft)}", Value = WatermarkPosition.TopLeft },
                    new () { Label = $"Enums.{nameof(WatermarkPosition)}.{nameof(WatermarkPosition.TopRight)}", Value = WatermarkPosition.TopRight },
                    new () { Label = $"Enums.{nameof(WatermarkPosition)}.{nameof(WatermarkPosition.BottomRight)}", Value = WatermarkPosition.BottomRight },
                    new () { Label = $"Enums.{nameof(WatermarkPosition)}.{nameof(WatermarkPosition.BottomLeft)}", Value = WatermarkPosition.BottomLeft }
                };
            }

            return _PositionOptions;
        }
    }
    
    /// <summary>
    /// Gets or sets the x-axis position of the watermark
    /// </summary>
    [NumberPercent(3, "Labels.Pixels", 10, true)]
    [ConditionEquals(nameof(Position), WatermarkPosition.Center, inverse: true)]
    public NumberPercent XPos { get; set; }
    
    /// <summary>
    /// Gets or sets the y-axis position of the watermark
    /// </summary>
    [NumberPercent(4, "Labels.Pixels", 10, true)]
    [ConditionEquals(nameof(Position), WatermarkPosition.Center, inverse: true)]
    public NumberPercent YPos { get; set; }

    
    /// <summary>
    /// Gets or sets the width of the watermark
    /// </summary>
    [NumberPercent(5, "Labels.Pixels", 0, false)]
    public NumberPercent Width { get; set; }
    
    /// <summary>
    /// Gets or sets the height of the watermark
    /// </summary>
    [NumberPercent(6, "Labels.Pixels", 0, false)]
    public NumberPercent Height { get; set; }
    
    /// <summary>
    /// Gets or sets the opacity of the watermark
    /// </summary>
    [Slider(7)]
    [Range(0, 100)]
    [DefaultValue(100)]
    public int Opacity { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var localResult = args.FileService.GetLocalPath(Image);
        if (localResult.Failed(out string error))
        {
            args.FailureReason = "Failed to get watermark image: " + error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var localFile = localResult.Value;
        
        // ffmpeg -i input.mp4 -i watermark.png -filter_complex "[1][0]scale2ref=oh*mdar:ih*0.2[logo][video];[video][logo]overlay=(main_w-overlay_w):(main_h-overlay_h)" output_bottom_right.mp4

        var model = GetModel();
        model.InputFiles.Add(new (localFile));
        string filter;

        int xPos = XPos.Value;
        if (XPos.Percentage)
        {
            float percentage = XPos.Value / 100f;
            xPos = (int)Math.Round(model.VideoInfo.VideoStreams[0].Width * percentage);
        }
        int yPos = YPos.Value;
        if (YPos.Percentage)
        {
            float percentage = YPos.Value / 100f;
            yPos = (int)Math.Round(model.VideoInfo.VideoStreams[0].Height * percentage);
        }

        switch (Position)
        {
            case WatermarkPosition.TopLeft:
                args.Logger?.ILog("Top Left watermark");
                filter = xPos + ":" + yPos;
                break;
            case WatermarkPosition.TopRight:
                args.Logger?.ILog("Top Right watermark");
                filter = $"W-w-{xPos}:{yPos}";
                break;
            case WatermarkPosition.BottomRight:
                args.Logger?.ILog("Bottom Right watermark");
                filter = $"W-w-{xPos}:H-h-{yPos}";
                break;
            case WatermarkPosition.BottomLeft:
                args.Logger?.ILog("Bottom Left watermark");
                filter = $"{xPos}:H-h-{yPos}";
                break;
            case WatermarkPosition.Center:
            default:
                args.Logger?.ILog("Centering watermark");
                filter = "(W-w)/2:(H-h)/2";
                break;
        }

        List<string> filterParts = new List<string>()
        {
            "overlay=" + filter
        };
        //
        // if (Opacity is > 0 and < 100)
        // {
        //     string opacity = (Opacity / 100f).ToString(); 
        //     filter = $"[::wmindex::]format=rgba,colorchannelmixer=aa={opacity}[wm];[0][wm]" + filter;
        // }
        //
        // int width = Width?.Value ?? 0;
        // if (width> 0 && Width.Percentage)
        // {
        //     float percentage = Width.Value / 100f;
        //     width = (int)Math.Round(model.VideoInfo.VideoStreams[0].Width * percentage);
        // }
        //
        // int height = Height?.Value ?? 0;
        // if (height > 0 && Height.Percentage)
        // {
        //     float percentage = Height.Value / 100f;
        //     height = (int)Math.Round(model.VideoInfo.VideoStreams[0].Height * percentage);
        // }
        //
        // if (width > 0 && height > 0)
        // {
        //     // scale width/height
        // }
        // else if (width > 0)
        // {
        //     // scale width
        // }
        // else if (height > 0)
        // {
        //     // scale height
        // }
        
        string watermarkLabel = "[::wmindex::]";
        int insertIndex = 0;
        if (Opacity > 0 && Opacity < 100)
        {
            string opacity = (Opacity / 100f).ToString(CultureInfo.InvariantCulture); 
            filterParts.Insert(insertIndex, $"{watermarkLabel}format=rgba,colorchannelmixer=aa={opacity}[wm];");
            ++insertIndex;
            watermarkLabel = "[wm]";
        }

        int width = Width?.Value ?? 0;
        if (width > 0 && Width.Percentage)
        {
            float percentage = Width.Value / 100f;
            width = (int)Math.Round(model.VideoInfo.VideoStreams[0].Width * percentage);
        }

        int height = Height?.Value ?? 0;
        if (height > 0 && Height.Percentage)
        {
            float percentage = Height.Value / 100f;
            height = (int)Math.Round(model.VideoInfo.VideoStreams[0].Height * percentage);
        }

        if (width > 0 && height > 0)
        {
            filterParts.Insert(insertIndex++, $"{watermarkLabel}scale={width}:{height}[wm];");
        }
        else if (width > 0)
        {
            filterParts.Insert(insertIndex++, $"{watermarkLabel}scale={width}:-1[wm];");
        }
        else if (height > 0)
        {
            // Scale height, maintain aspect ratio for width
            filterParts.Insert(insertIndex++, $"{watermarkLabel}scale=-1:{height}[wm];");
        }
        
        filterParts.Insert(insertIndex++, "[0]" + watermarkLabel);

        filter = string.Join(string.Empty, filterParts);
        
        //=
        //     "[::wmindex::]" + "[0]scale2ref=oh*mdar:ih*0.2[logo][video];[video][logo]overlay=(main_w-overlay_w):(main_h-overlay_h)";
        // filter =
        //     //$"overlay=10:10";
        //     "overlay=(main_w-overlay_w):(main_h-overlay_h)";

        //model.VideoStreams[0].FilterComplex.Add(filter);
        model.Watermark = new()
        {
            Image = localFile,
            Filter = filter
        };
        model.ForceEncode = true;

        return 1;
    }
    
    
    /// <summary>
    /// Enum representing the position of a watermark.
    /// </summary>
    public enum WatermarkPosition
    {
        /// <summary>
        /// Center position.
        /// </summary>
        Center = 0,
    
        /// <summary>
        /// Top-left position.
        /// </summary>
        TopLeft = 1,
    
        /// <summary>
        /// Top-right position.
        /// </summary>
        TopRight = 2,
    
        /// <summary>
        /// Bottom-right position.
        /// </summary>
        BottomRight = 3,
    
        /// <summary>
        /// Bottom-left position.
        /// </summary>
        BottomLeft = 4,
    
        /// <summary>
        /// Custom position.
        /// </summary>
        Custom = 5
    }

}

/// <summary>
/// The watermark model of the FFmpegBuilder
/// </summary>
internal class WatermarkModel
{
    /// <summary>
    /// The image for the watermark
    /// </summary>
    public string Image { get; init; }
    /// <summary>
    /// The filter for the watermark
    /// </summary>
    public string Filter { get; init; }
}