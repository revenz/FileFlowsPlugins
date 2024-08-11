using FileFlows.Nextcloud.Helpers;

namespace FileFlows.Nextcloud.FlowElements;

/// <summary>
/// Uploads a file to Nextcloud
/// </summary>
public class UploadToNextcloud : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string CustomColor => "#0082c9";
    /// <inheritdoc /> 
    public override string Icon => "svg:nextcloud";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/nextcloud/upload-to-next-cloud";

    /// <summary>
    /// Gets or sets the destination path
    /// </summary>
    [Folder(1)]
    public string DestinationPath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the file to unpack
    /// </summary>
    [TextVariable(2)]
    public string File { get; set; } = null!;
    
    public override int Execute(NodeParameters args)
    {
        var settings = args.GetPluginSettings<PluginSettings>();

        if (string.IsNullOrWhiteSpace(settings?.Url))
        {
            args.FailureReason = "No Nextcloud URL set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        if (string.IsNullOrWhiteSpace(settings?.Username))
        {
            args.FailureReason = "No Nextcloud User set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        if (string.IsNullOrWhiteSpace(settings?.Password))
        {
            args.FailureReason = "No Nextcloud Password set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var file = args.ReplaceVariables(File ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile;
        var destination = args.ReplaceVariables(DestinationPath ?? string.Empty, stripMissing: true);

        if (string.IsNullOrWhiteSpace(destination))
        {
            args.FailureReason = "No Destination set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var local = args.FileService.GetLocalPath(file);
        if (local.Failed(out var error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var uploader = new NextcloudUploader(args.Logger!, settings.Url, settings.Username, settings.Password);
        var result = uploader.UploadFile(local.Value, destination);
        if(result.Failed(out error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        
        args.Logger?.ILog("File successfully uploaded");
        return 1;
    }
}