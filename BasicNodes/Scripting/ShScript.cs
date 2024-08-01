using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Scripting;

/// <summary>
/// Flow element that executes a SH script
/// </summary>
public class ShScript : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "svg:sh";
    /// <inheritdoc />
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/sh-script";

    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    [Required]
    [DefaultValue(@"# This is a template shell script

# Replace {file.FullName} and {file.Orig.FullName} with actual values
WorkingFile=""{file.FullName}""
OriginalFile=""{file.Orig.FullName}""

# Example commands using the variables
echo ""Working on file: $WorkingFile""
echo ""Original file location: $OriginalFile""

# Add your actual shell commands below
# Example: Copy the working file to a backup location
# cp ""$WorkingFile"" ""/path/to/backup/$(basename \""$WorkingFile\"")""

# Set the exit code to 0
exit 0
")]
    [Code(1, "sh")]
    public string Code { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrEmpty(Code))
        {
            args.FailureReason = "No code specified in SH script";
            args.Logger?.ELog(args.FailureReason);
            return -1; // no code, flow cannot continue doesn't know what to do
        }

        if (OperatingSystem.IsWindows())
        {
            args.FailureReason = "Cannot run a SH script on a Windows system";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var shFile = System.IO.Path.Combine(args.TempPath, Guid.NewGuid() + ".sh");

        try
        {
            var code = args.ReplaceVariables(Code);
            args.Logger?.ILog("Executing code: \n" + code);
            System.IO.File.WriteAllText(shFile, code);
            args.Logger?.ILog($"Temporary SH file created: {shFile}");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{shFile}\"",
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

            if (!string.IsNullOrWhiteSpace(standardOutput))
                args.Logger?.ILog($"Standard Output:\n{standardOutput}");
            if (!string.IsNullOrWhiteSpace(standardError))
                args.Logger?.WLog($"Standard Error:\n{standardError}");
            args.Logger?.ILog($"Exit Code: {exitCode}");

            return exitCode == 0 ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.FailureReason = "Failed executing SH script: " + ex.Message;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        finally
        {
            try
            {
                if (System.IO.File.Exists(shFile))
                {
                    System.IO.File.Delete(shFile);
                    args.Logger?.ILog($"Temporary SH file deleted: {shFile}");
                }
            }
            catch (Exception ex)
            {
                args.Logger?.WLog($"Failed to delete temporary SH file: {shFile}. Error: {ex.Message}");
            }
        }
    }
}
