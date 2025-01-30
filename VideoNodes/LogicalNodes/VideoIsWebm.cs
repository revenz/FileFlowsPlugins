using System.IO;

namespace FileFlows.VideoNodes;

/// <summary>
/// Checks if a video file is in WebM format.
/// </summary>
public class VideoIsWebM : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-webm";

    /// <summary>
    /// Executes the flow element to check if the file is a WebM file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>1 if the file is a WebM, 2 if not, -1 on failure.</returns>
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
            // WebM files have the same magic number as MKV: 1A 45 DF A3, but their format type is "webm"
            byte[] webmSignature = { 0x1A, 0x45, 0xDF, 0xA3 };
            byte[] fileSignature = new byte[4];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 4) < 4)
                {
                    args.Logger?.ILog("File is too small to be a WebM.");
                    return 2; // Not a WebM
                }
            }

            bool isWebM = fileSignature[0] == webmSignature[0] &&
                          fileSignature[1] == webmSignature[1] &&
                          fileSignature[2] == webmSignature[2] &&
                          fileSignature[3] == webmSignature[3];

            args.Logger?.ILog(isWebM ? "File is a WebM." : "File is not a WebM.");
            return isWebM ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}