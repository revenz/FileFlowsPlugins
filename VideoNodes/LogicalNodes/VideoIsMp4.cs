using System.IO;

namespace FileFlows.VideoNodes;

/// <summary>
/// Checks if a video file is in MP4 format.
/// </summary>
public class VideoIsMp4 : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-mp4";

    /// <summary>
    /// Executes the flow element to check if the file is an MP4.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>1 if the file is an MP4, 2 if not, -1 on failure.</returns>
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
            // MP4 files have magic numbers: 00 00 00 ?? 66 74 79 70 (?? can be any byte)
            byte[] mp4Signature = { 0x66, 0x74, 0x79, 0x70 };
            byte[] fileSignature = new byte[8];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 8) < 8)
                {
                    args.Logger?.ILog("File is too small to be an MP4.");
                    return 2; // Not an MP4
                }
            }

            bool isMp4 = fileSignature[4] == mp4Signature[0] &&
                         fileSignature[5] == mp4Signature[1] &&
                         fileSignature[6] == mp4Signature[2] &&
                         fileSignature[7] == mp4Signature[3];

            args.Logger?.ILog(isMp4 ? "File is an MP4." : "File is not an MP4.");
            return isMp4 ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}