using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Replaces the original file
/// </summary>
public class ReplaceOriginal : Node
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 1;
    /// <summary>
    /// Gets the icon to use
    /// </summary>
    public override string Icon => "fas fa-file";
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/replace-original"; 
    /// <summary>
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;

    public string _Pattern = string.Empty;
    
    /// <summary>
    /// Gets or sets if the original files creation and last write time dates should be preserved
    /// </summary>
    [Boolean(1)]
    public bool PreserverOriginalDates { get; set; }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the next output to execute</returns>
    public override int Execute(NodeParameters args)
    {
        if (args.FileName == args.WorkingFile)
        {
            args.Logger?.ILog("Working file is same as original, nothing to do.");
            return 1;
        }
        var wf = new FileInfo(args.WorkingFile);
        if (args.FileName.ToLower().EndsWith(wf.Extension.ToLower()))
        {
            // easy replace
            bool moved = args.MoveFile(args.FileName);
            if(moved == false)
            {
                args.Logger?.ELog("Failed to move file to: "+ args.FileName);
                return -1;
            }

            if(PreserverOriginalDates && args.Variables.TryGetValue("ORIGINAL_CREATE_UTC", out object oCreateTimeUtc) &&
               args.Variables.TryGetValue("ORIGINAL_LAST_WRITE_UTC", out object oLastWriteUtc) &&
               oCreateTimeUtc is DateTime dtCreateTimeUtc && oLastWriteUtc is DateTime dtLastWriteUtc)
            {
                Helpers.FileHelper.SetCreationTime(args.FileName, dtCreateTimeUtc);
                Helpers.FileHelper.SetLastWriteTime(args.FileName, dtLastWriteUtc);
            }
        }
        else
        {
            // different extension, we will move the file, but then delete the original                
            string dest = Path.ChangeExtension(args.FileName, wf.Extension);
            if(args.MoveFile(dest) == false)
            {
                args.Logger?.ELog("Failed to move file to: " + dest);
                return -1;
            }
            if (dest.ToLower() != args.FileName.ToLower())
            {
                try
                {
                    System.IO.File.Delete(args.FileName);
                }
                catch (Exception ex)
                {
                    args.Logger?.ELog("Failed to delete orginal (with different extension): " + ex.Message);
                    return -1;
                }
            }
            

            if(PreserverOriginalDates && args.Variables.TryGetValue("ORIGINAL_CREATE_UTC", out object oCreateTimeUtc) &&
               args.Variables.TryGetValue("ORIGINAL_LAST_WRITE_UTC", out object oLastWriteUtc) &&
               oCreateTimeUtc is DateTime dtCreateTimeUtc && oLastWriteUtc is DateTime dtLastWriteUtc)
            {
                Helpers.FileHelper.SetCreationTime(dest, dtCreateTimeUtc);
                Helpers.FileHelper.SetLastWriteTime(dest, dtLastWriteUtc);
            }
        }

        return 1;
    }
}