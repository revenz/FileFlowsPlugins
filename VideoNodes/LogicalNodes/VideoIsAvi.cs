
using System.IO;

namespace FileFlows.VideoNodes;

/// <summary>
/// Checks if a video file is in AVI format.
/// </summary>
public class VideoIsAvi : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-avi";

    /// <summary>
    /// Executes the flow element to check if the file is an AVI file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>1 if the file is an AVI, 2 if not, -1 on failure.</returns>
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
            // AVI files have the signature: 52 49 46 46 (RIFF) followed by 41 56 49 20 (AVI )
            byte[] aviSignature = { 0x52, 0x49, 0x46, 0x46 }; // RIFF
            byte[] fileSignature = new byte[4];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 4) < 4)
                {
                    args.Logger?.ILog("File is too small to be an AVI.");
                    return 2; // Not an AVI
                }

                // Read next 4 bytes for "AVI "
                fs.Seek(4, SeekOrigin.Current);
                byte[] aviIdentifier = { 0x41, 0x56, 0x49, 0x20 };
                byte[] fileIdentifier = new byte[4];
                if (fs.Read(fileIdentifier, 0, 4) < 4)
                {
                    return 2; // Not an AVI
                }

                bool isAvi = fileSignature[0] == aviSignature[0] &&
                             fileSignature[1] == aviSignature[1] &&
                             fileSignature[2] == aviSignature[2] &&
                             fileSignature[3] == aviSignature[3] &&
                             fileIdentifier[0] == aviIdentifier[0] &&
                             fileIdentifier[1] == aviIdentifier[1] &&
                             fileIdentifier[2] == aviIdentifier[2] &&
                             fileIdentifier[3] == aviIdentifier[3];

                args.Logger?.ILog(isAvi ? "File is an AVI." : "File is not an AVI.");
                return isAvi ? 1 : 2;
            }
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}
