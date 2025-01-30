
using System.IO;
using FileFlows.VideoNodes;

/// <summary>
/// Checks if a video file is in WMV format.
/// </summary>
public class VideoIsWmv : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-wmv";

    /// <summary>
    /// Executes the flow element to check if the file is a WMV file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>1 if the file is a WMV, 2 if not, -1 on failure.</returns>
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
            // WMV files have ASF header: 30 26 B2 75 8E 66 CF 11
            byte[] wmvSignature = { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11 };
            byte[] fileSignature = new byte[8];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 8) < 8)
                {
                    args.Logger?.ILog("File is too small to be a WMV.");
                    return 2; // Not a WMV
                }
            }

            bool isWmv = fileSignature[0] == wmvSignature[0] &&
                         fileSignature[1] == wmvSignature[1] &&
                         fileSignature[2] == wmvSignature[2] &&
                         fileSignature[3] == wmvSignature[3] &&
                         fileSignature[4] == wmvSignature[4] &&
                         fileSignature[5] == wmvSignature[5] &&
                         fileSignature[6] == wmvSignature[6] &&
                         fileSignature[7] == wmvSignature[7];

            args.Logger?.ILog(isWmv ? "File is a WMV." : "File is not a WMV.");
            return isWmv ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}