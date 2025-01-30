using System.IO;
using FileFlows.VideoNodes;

/// <summary>
/// Checks if a video file is in MOV format.
/// </summary>
public class VideoIsMov : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-mov";

    /// <summary>
    /// Executes the flow element to check if the file is a MOV file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>1 if the file is a MOV, 2 if not, -1 on failure.</returns>
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
            // MOV files typically start with "ftyp" and "moov" headers (in the file's signature)
            byte[] movSignature = { 0x66, 0x74, 0x79, 0x70 }; // 'ftyp'
            byte[] fileSignature = new byte[4];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 4) < 4)
                {
                    args.Logger?.ILog("File is too small to be a MOV.");
                    return 2; // Not a MOV
                }
            }

            bool isMov = fileSignature[0] == movSignature[0] &&
                         fileSignature[1] == movSignature[1] &&
                         fileSignature[2] == movSignature[2] &&
                         fileSignature[3] == movSignature[3];

            args.Logger?.ILog(isMov ? "File is a MOV." : "File is not a MOV.");
            return isMov ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}
