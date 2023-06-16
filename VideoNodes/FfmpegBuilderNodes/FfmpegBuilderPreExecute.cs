using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that can alter the command line arguments that the FFmpeg Builder will run
/// </summary>
public class FfmpegBuilderPreExecute : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the Help URL for this node
    /// </summary>
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/pre-execute";

    /// <summary>
    /// Gets the icon for this node
    /// </summary>
    public override string Icon => "fas fa-mortar-pestle";

    /// <summary>
    /// Gets the number of outputs for this element
    /// </summary>
    public override int Outputs => 1; 

    [Required]
    [DefaultValue("// Custom javascript code that runs just before the FFmpeg Builder: Executor executes the FFmpeg process.\n// Here you can alter FFmpeg parameters etc.  See Help for more information.")]
    [Code(1)]
    public string Code { get; set; }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output number to execute next</returns>
    public override int Execute(NodeParameters args)
    {
        this.Model.PreExecuteCode = this.Code;
        return 1;
    }
}
