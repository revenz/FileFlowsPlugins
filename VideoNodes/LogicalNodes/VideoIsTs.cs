using System.IO;
using FileFlows.VideoNodes;

/// <summary>
/// Checks if a video file is in TS format.
/// </summary>
public class VideoIsTs : VideoNode
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;

    /// <inheritdoc />
    public override string Icon => "fas fa-question";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-ts";

    /// <summary>
    /// Executes the flow element to check if the file is a TS file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>1 if the file is a TS, 2 if not, -1 on failure.</returns>
    public override int Execute(NodeParameters args)
    {
        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.IsFailed)
        {
            args.Logger?.ELog("Failed to get local file: " + localFile.Error);
            return -1;
        }

        var localFilePath = localFile.Value;

        try
        {
            // TS files typically start with 0x47 (this is the "GOP" start byte for MPEG transport streams)
            byte[] tsSignature = { 0x47 }; // 'GOP' start byte
            byte[] fileSignature = new byte[1];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 1) < 1)
                {
                    args.Logger?.ILog("File is too small to be a TS.");
                    return 2; // Not a TS
                }
            }

            bool isTs = fileSignature[0] == tsSignature[0];

            args.Logger?.ILog(isTs ? "File is a TS." : "File is not a TS.");
            return isTs ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}
