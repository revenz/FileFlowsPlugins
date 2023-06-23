using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Node that adds custom parameters to the FFMPEG Builder
/// </summary>
public class FfmpegBuilderCustomParameters : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the Help URL for this node
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/custom-parameters";

    /// <summary>
    /// Gets the icon for this node
    /// </summary>
    public override string Icon => "fas fa-plus-square";

    /// <summary>
    /// Gets the number of outputs for this node
    /// </summary>
    public override int Outputs => 1; 


    /// <summary>
    /// Gets or sets the parameters to add
    /// </summary>
    [TextVariable(1)]
    [Required]
    public string Parameters { get; set; }

    /// <summary>
    /// Gets or sets if the video should be forcable encoded when these parameters are added
    /// </summary>
    [Boolean(2)]
    [DefaultValue(true)]
    public bool ForceEncode { get; set; }


    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output number to execute next</returns>
    public override int Execute(NodeParameters args)
    {
        string parameters = args.ReplaceVariables(Parameters);
        if (string.IsNullOrWhiteSpace(parameters))
            return 1;

        string[] split = Regex.Split(parameters, "(\"[^\"]+\"|[^\\s\"]+)");
        foreach(var parameter in split)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                continue;

            string actual = parameter;
            if (parameter.StartsWith("\"") && parameter.EndsWith("\""))
                actual = parameter[1..^1];
            this.Model.CustomParameters.Add(actual);
        }

        this.Model.ForceEncode = ForceEncode;

        return 1;
    }
}
