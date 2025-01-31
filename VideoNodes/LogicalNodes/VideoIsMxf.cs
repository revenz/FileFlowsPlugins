using System.IO;

namespace FileFlows.VideoNodes;

/// <summary>
/// Checks if a video file is in MXF format.
/// </summary>
public class VideoIsMxf : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-mxf";

    /// <summary>
    /// Executes the flow element to check if the file is an MXF file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>1 if the file is an MXF, 2 if not, -1 on failure.</returns>
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
            // MXF files start with the signature: 06 0E 2B 34
            byte[] mxfSignature = { 0x06, 0x0E, 0x2B, 0x34 };
            byte[] fileSignature = new byte[4];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 4) < 4)
                {
                    args.Logger?.ILog("File is too small to be an MXF.");
                    return 2; // Not an MXF
                }
            }

            bool isMxf = fileSignature[0] == mxfSignature[0] &&
                         fileSignature[1] == mxfSignature[1] &&
                         fileSignature[2] == mxfSignature[2] &&
                         fileSignature[3] == mxfSignature[3];

            args.Logger?.ILog(isMxf ? "File is an MXF." : "File is not an MXF.");
            return isMxf ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}