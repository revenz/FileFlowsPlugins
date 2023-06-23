using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Set a node that adds a video tag to a file
/// </summary>
public class FfmpegBuilderVideoTag:FfmpegBuilderNode
{
    /// <summary>
    /// The number of outputs for this node
    /// </summary>
    public override int Outputs => 1;

    public override string Icon => "fas fa-tag";

    /// <summary>
    /// The Help URL for this node
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/video-tag";


    /// <summary>
    /// Gets or sets the tag to set
    /// </summary>
    [TextVariable(1)]
    [Required]
    public string Tag { get; set; }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">The node arguments</param>
    /// <returns>the output return</returns>
    public override int Execute(NodeParameters args)
    {
        string tag = args.ReplaceVariables(Tag, stripMissing: true);

        if (string.IsNullOrEmpty(Tag))
            return 1; // nothing to do

        var stream = Model.VideoStreams.Where(x => x.Deleted == false).First();
        stream.AdditionalParameters.AddRange(new[] { "-tag:v", tag });

        stream.ForcedChange = true;
        return 1;
    }
}
