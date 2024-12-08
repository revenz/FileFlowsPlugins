using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Helpers;

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
        var wfExtension = FileHelper.GetExtension(args.WorkingFile);
        if (args.FileName.ToLower().EndsWith(wfExtension.ToLower()))
        {
            // easy replace
            var moveResult = args.MoveFile(args.FileName);
            if (moveResult.Failed(out var error))
            {
                args.Logger?.ELog("Failed to move file to: " + args.FileName + " => " + error);
                return -1;
            }
            if(moveResult.Value == false)
            {
                args.Logger?.ELog("Failed to move file to: "+ args.FileName);
                return -1;
            }

            if(PreserverOriginalDates && args.Variables.TryGetValue("ORIGINAL_CREATE_UTC", out object oCreateTimeUtc) &&
               args.Variables.TryGetValue("ORIGINAL_LAST_WRITE_UTC", out object oLastWriteUtc) &&
               oCreateTimeUtc is DateTime dtCreateTimeUtc && oLastWriteUtc is DateTime dtLastWriteUtc)
            {
                args.FileService.SetCreationTimeUtc(args.FileName, dtCreateTimeUtc);
                args.FileService.SetLastWriteTimeUtc(args.FileName, dtLastWriteUtc);
            }
        }
        else
        {
            // different extension, we will move the file, but then delete the original                
            string dest = FileHelper.ChangeExtension(args.FileName, wfExtension);
            if(args.MoveFile(dest).Failed(out var error))
            {
                args.Logger?.ELog("Failed to move file to: " + dest + Environment.NewLine + error);
                return -1;
            }
            args.SetWorkingFile(dest);

            if (string.Equals(dest, args.FileName, StringComparison.CurrentCultureIgnoreCase) == false)
            {
                var result = args.FileService.FileDelete(args.FileName);
                if (result.IsFailed)
                {
                    args.Logger?.ELog("Failed to delete original (with different extension): " + result.Error);
                    return -1;
                }
            }


            if(PreserverOriginalDates && args.Variables.TryGetValue("ORIGINAL_CREATE_UTC", out object oCreateTimeUtc) &&
               args.Variables.TryGetValue("ORIGINAL_LAST_WRITE_UTC", out object oLastWriteUtc) &&
               oCreateTimeUtc is DateTime dtCreateTimeUtc && oLastWriteUtc is DateTime dtLastWriteUtc)
            {
                args.FileService.SetCreationTimeUtc(args.FileName, dtCreateTimeUtc);
                args.FileService.SetLastWriteTimeUtc(args.FileName, dtLastWriteUtc);
            }
        }

        return 1;
    }
}