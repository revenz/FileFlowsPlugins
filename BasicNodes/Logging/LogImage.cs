using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FileFlows.BasicNodes.Logging;

/// <summary>
/// Flow that logs a image to the Flow logger
/// </summary>
public class LogImage : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "far fa-image";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/log-image"; 
    /// <inheritdoc />
    public override bool FailureNode => true;

    /// <inheritdoc />
    public override string Group => "Logging";
    
    /// <summary>
    /// Gets or sets the file to log
    /// </summary>
    [TextVariable(1)]
    [Required]
    public string ImageFile { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var file = args.ReplaceVariables(ImageFile ?? string.Empty, stripMissing: true);
        if (string.IsNullOrWhiteSpace(file))
        {
            args.Logger?.WLog("No image defined");
            return 2;
        }

        var localFileResult = args.FileService.GetLocalPath(file);
        if (localFileResult.Failed(out var error))
        {
            args.Logger?.WLog(error);
            return 2;
        }

        var localFile = localFileResult.Value;
        var info = args.ImageHelper.GetInfo(localFile);
        if(info.Failed(out error))
        {
            args.Logger?.ILog("Not an image: " + file);
            args.Logger?.ILog(error);
            return 2;
        }
        args.LogImage(localFile);

        return 1;
    }
}
