using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Checks if a file has hard links
/// </summary>
public class HasHardLinks: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-link";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/has-hard-links";
    /// <inheritdoc />
    public override bool NoEditorOnAdd => true;

    /// <summary>
    /// Gets or sets the name of the file to check
    /// Leave blank to test the working file
    /// </summary>
    [TextVariable(1)]
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the number of hard links to check for
    /// </summary>
    [NumberInt(2)]
    [Range(1, 1000)]
    public int Count { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string file = args.ReplaceVariables(FileName ?? string.Empty, true)?.EmptyAsNull() ?? args.WorkingFile;
        if(string.IsNullOrWhiteSpace(file))
        {
            args.FailureReason = "FileName not set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        int count = Math.Min(1, Count);
        args.Logger?.ILog("Required hard links: " + count);

        bool hasHardLinks = false;
        if (OperatingSystem.IsWindows())
            hasHardLinks = HasHardLinkWindows(args, file, count);
        else if(OperatingSystem.IsLinux())
            hasHardLinks = HasHardLinkLinux(args, file, count);
        else if(OperatingSystem.IsMacOS())
            hasHardLinks = HasHardLinkMacOS(args, file, count);
        else
        {
            args.Logger?.WLog("Unable to determine operating system to check for hard links");
        }

        return hasHardLinks ? 1 : 2;

    }


    /// <summary>
    /// Checks if a file has hard links on Windows.
    /// </summary>
    /// <param name="args">Node parameters containing logger.</param>
    /// <param name="file">File path to check for hard links.</param>
    /// <param name="count">the required number of hard links</param>
    /// <returns>True if the file has hard links; otherwise, false.</returns>
    bool HasHardLinkWindows(NodeParameters args, string file, int count)
    {
        try
        {
            args.Logger?.ILog("HasHardLinkWindows: " + file);
            // Get file attributes
            FileAttributes attributes = System.IO.File.GetAttributes(file);

            // Check if the file has the ReparsePoint or Directory flags set
            // A file with these flags set may have hard links
            if ((attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint ||
                (attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                args.Logger?.ILog("The file may have hard links.");
                return true;
            }
            
            args.Logger?.ILog("The file does not appear to have hard links.");
            return false;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Failed to test if file has hard links: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Checks if a file has hard links on Linux.
    /// </summary>
    /// <param name="args">Node parameters containing logger.</param>
    /// <param name="file">File path to check for hard links.</param>
    /// <param name="count">the required number of hard links</param>
    /// <returns>True if the file has hard links; otherwise, false.</returns>
    bool HasHardLinkLinux(NodeParameters args, string file, int count)
    {
        try
        {
            args.Logger?.ILog("HasHardLinkLinux: " + file);
            Process process = new Process();
            process.StartInfo.FileName = "stat";
            process.StartInfo.ArgumentList.Add("-c");
            process.StartInfo.ArgumentList.Add("%h");
            process.StartInfo.ArgumentList.Add(file);
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            args.Logger?.ILog("Exit Code: "+ process.ExitCode);
            if(string.IsNullOrWhiteSpace(output) == false)
                args.Logger?.ILog("Standard Output: " + output);
            if(string.IsNullOrWhiteSpace(error) == false)
                args.Logger?.ILog("Error Output: " + error);

            if (int.TryParse(output.Trim(), out var linkCount))
            {
                if (linkCount > 0)
                {
                    args.Logger?.ILog($"The file has {linkCount} hard links.");
                    if (linkCount >= count)
                        return true;
                }
                args.Logger?.ILog("The file does not have the required number of hard links.");
                return false;
            }
            
            if(string.IsNullOrWhiteSpace(error))
                args.Logger?.ELog(error);
            
            args.Logger?.ILog("Failed to retrieve link count.");
            return false;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog($"An error occurred: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Checks if a file has hard links on macOS.
    /// </summary>
    /// <param name="args">Node parameters containing logger.</param>
    /// <param name="file">File path to check for hard links.</param>
    /// <param name="count">the required number of hard links</param>
    /// <returns>True if the file has hard links; otherwise, false.</returns>
    public bool HasHardLinkMacOS(NodeParameters args, string file, int count)
    {
        try
        {
            args.Logger?.ILog("HasHardLinkMacOS: " + file);
            Process process = new Process();
            process.StartInfo.FileName = "stat";
            process.StartInfo.ArgumentList.Add("-f");
            process.StartInfo.ArgumentList.Add("%l");
            process.StartInfo.ArgumentList.Add(file);
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            if(string.IsNullOrWhiteSpace(output) == false)
                args.Logger?.ILog("Standard Output: " + output);
            if(string.IsNullOrWhiteSpace(error) == false)
                args.Logger?.ILog("Error Output: " + error);

            if (int.TryParse(output.Trim(), out var linkCount))
            {
                if (linkCount > 0)
                {
                    args.Logger?.ILog($"The file has {linkCount} hard links.");
                    if (linkCount >= count)
                        return true;
                }
                args.Logger?.ILog("The file does not have hard links.");
                return false; 
            }
            
            args.Logger?.ILog("The file does not have the required number of hard links.");
            return false;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog($"An error occurred: {ex.Message}");
            return false;
        }
    }
}