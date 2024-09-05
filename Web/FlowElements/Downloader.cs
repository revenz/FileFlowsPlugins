using System.Net;
using System.Text.RegularExpressions;
using FileFlows.Web.Helpers;

namespace FileFlows.Web.FlowElements;

/// <summary>
/// Downloads a URL
/// </summary>
public class Downloader : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Communication;

    /// <inheritdoc />
    public override string Icon => "fas fa-download";

    /// <inheritdoc />
    public override string CustomColor => "#40a6eb";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/web/downloader";

    /// <inheritdoc />
    public override string Group => "Web";

    /// <summary>
    /// Gets or sets the URL to download
    /// </summary>
    [TextVariable(1)]
    public string Url { get; set; } = null!;

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var url = args.ReplaceVariables(Url ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile;
        if (Regex.IsMatch(url, "^http(s)?://", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase) == false)
        {
            args.FailureReason = "Not a valid URL: " + url;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var result = DownloadHelper.Download(args.Logger!, url, args.TempPath, (percent) =>
            {
                args.PartPercentageUpdate?.Invoke(percent);
            });
        
        if(result.Failed(out var error))
        {
            args.FailureReason =error;
            args.Logger?.ELog(error);
            return -1;
        }

        args.SetWorkingFile(result.Value);

        return 1;
    }
}