using System.Text.RegularExpressions;
using FileFlows.Web.Helpers;

namespace FileFlows.Web.FlowElements;

/// <summary>
/// Input for a URL
/// </summary>
public class InputUrl : Node
{
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Input;
    /// <inheritdoc />
    public override string Icon => "fas fa-globe";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/web/input-url";
    /// <inheritdoc />
    public override string Group => "Web";

    /// <summary>
    /// Gets or sets if this should download the URL
    /// </summary>
    [Boolean(1)]
    public bool Download { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string url = args.WorkingFile;
        args.Variables["Url"] = url;
        if (Download == false || Regex.IsMatch(url, "^http(s)?://", RegexOptions.IgnoreCase) == false)
            return 1;
        
        var result = DownloadHelper.Download(args.Logger!, url, args.TempPath, (percent) =>
        {
            args.PartPercentageUpdate?.Invoke(percent);
        });
        
        if(result.Failed(out var error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }

        args.SetWorkingFile(result.Value);
        
        return 1;
    }
}