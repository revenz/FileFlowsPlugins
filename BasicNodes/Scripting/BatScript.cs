using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Scripting;

/// <summary>
/// Flow element that executes a bat script
/// </summary>
public class BatScript : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "svg:bat";
    /// <inheritdoc />
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/bat-script";
    
    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    [Required]
    [DefaultValue(@"REM This is a template batch file

REM Replace {file.FullName} and {file.Orig.FullName} with actual values
SET WorkingFile={file.FullName}
SET OriginalFile={file.Orig.FullName}

REM Example commands using the variables
echo Working on file: %WorkingFile%
echo Original file location: %OriginalFile%

REM Add your actual batch commands below
REM Example: Copy the working file to a backup location
REM copy ""%WorkingFile%"" ""C:\Backup\%~nxWorkingFile%""

REM Set the exit code to 0
EXIT /B 0
")]
    [Code(1, "bat")]
    public string Code { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrEmpty(Code))
        {
            args.FailureReason = "No code specified in .bat script";
            args.Logger?.ELog(args.FailureReason);
            return -1; // no code, flow cannot continue doesn't know what to do
        }

        if (OperatingSystem.IsWindows() == false)
        {
            args.FailureReason = "Cannot run a .bat file on a non Windows system";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        
        var batFile = System.IO.Path.Combine(args.TempPath, Guid.NewGuid() + ".bat");

        try
        {
            var code = "@echo off" + Environment.NewLine + args.ReplaceVariables(Code);
            args.Logger?.ILog("Executing code: \n" + code);
            System.IO.File.WriteAllText(batFile, code);
            args.Logger?.ILog($"Temporary bat file created: {batFile}");
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = batFile,
                WorkingDirectory = args.TempPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();

            process.WaitForExit();
            int exitCode = process.ExitCode;
            
            if(string.IsNullOrWhiteSpace(standardOutput) == false)
                args.Logger?.ILog($"Standard Output:\n{standardOutput}");
            if(string.IsNullOrWhiteSpace(standardError) == false)
                args.Logger?.WLog($"Standard Error:\n{standardError}");
            args.Logger?.ILog($"Exit Code: {exitCode}");

            return exitCode == 0 ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.FailureReason = "Failed executing bat script: " + ex.Message;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        finally
        {
            try
            {
                if (System.IO.File.Exists(batFile))
                {
                    System.IO.File.Delete(batFile);
                    args.Logger?.ILog($"Temporary bat file deleted: {batFile}");
                }
            }
            catch (Exception ex)
            {
                args.Logger?.WLog($"Failed to delete temporary bat file: {batFile}. Error: {ex.Message}");
            }
        }
    }
}