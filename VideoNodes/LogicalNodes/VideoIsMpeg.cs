using System.IO;

namespace FileFlows.VideoNodes;

/// <summary>
/// Checks if a video file is in MPEG format.
/// </summary>
public class VideoIsMpeg : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-mpeg";

    /// <summary>
    /// Executes the flow element to check if the file is an MPEG file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>1 if the file is an MPEG, 2 if not, -1 on failure.</returns>
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
            // MPEG files typically start with 0x00 0x00 0x01 (video stream) or 0x00 0x00 0x01 0xB3 (for MPEG-1).
            byte[] mpegSignature = { 0x00, 0x00, 0x01 };
            byte[] fileSignature = new byte[3];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 3) < 3)
                {
                    args.Logger?.ILog("File is too small to be an MPEG.");
                    return 2; // Not an MPEG
                }
            }

            bool isMpeg = fileSignature[0] == mpegSignature[0] &&
                          fileSignature[1] == mpegSignature[1] &&
                          fileSignature[2] == mpegSignature[2];

            args.Logger?.ILog(isMpeg ? "File is an MPEG." : "File is not an MPEG.");
            return isMpeg ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}
