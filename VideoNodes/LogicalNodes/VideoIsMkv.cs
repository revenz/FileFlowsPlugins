using System.IO;

namespace FileFlows.VideoNodes;

/// <summary>
/// Video is MKV
/// </summary>
public class VideoIsMkv : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-mkv";

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
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
            // MKV files have a magic number: 1A 45 DF A3 at the start
            byte[] mkvSignature = { 0x1A, 0x45, 0xDF, 0xA3 };
            byte[] fileSignature = new byte[4];

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(fileSignature, 0, 4) < 4)
                {
                    args.Logger?.ILog("File is too small to be an MKV.");
                    return 2; // Not an MKV
                }
            }

            bool isMkv = fileSignature[0] == mkvSignature[0] &&
                         fileSignature[1] == mkvSignature[1] &&
                         fileSignature[2] == mkvSignature[2] &&
                         fileSignature[3] == mkvSignature[3];

            args.Logger?.ILog(isMkv ? "File is an MKV." : "File is not an MKV.");
            return isMkv ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Error reading file: " + ex.Message);
            return 2;
        }
    }
}