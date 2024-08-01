using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Scripting;

/// <summary>
/// Flow element that executes a PowerShell script
/// </summary>
public class PowerShellScript : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "svg:ps1";
    /// <inheritdoc />
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/powershell-script";
    
    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    [Required]
    [DefaultValue(@"# This is a template PowerShell script

# Replace {file.FullName} and {file.Orig.FullName} with actual values
$WorkingFile = '{file.FullName}'
$OriginalFile = '{file.Orig.FullName}'

# Example commands using the variables
Write-Output ""Working on file: $WorkingFile""
Write-Output ""Original file location: $OriginalFile""

# Add your actual PowerShell commands below
# Example: Copy the working file to a backup location
# Copy-Item -Path $WorkingFile -Destination ""C:\Backup\$([System.IO.Path]::GetFileName($WorkingFile))""

# Set the exit code to 0
exit 0
")]
    [Code(1, "powershell")]
    public string Code { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrEmpty(Code))
        {
            args.FailureReason = "No code specified in PowerShell script";
            args.Logger?.ELog(args.FailureReason);
            return -1; // no code, flow cannot continue doesn't know what to do
        }

        if (OperatingSystem.IsWindows() == false)
        {
            args.FailureReason = "Cannot run a PowerShell file on a non Windows system";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        
        var ps1File = System.IO.Path.Combine(args.TempPath, Guid.NewGuid() + ".ps1");

        try
        {
            var code = args.ReplaceVariables(Code);
            args.Logger?.ILog("Executing code: \n" + code);
            System.IO.File.WriteAllText(ps1File, code);
            args.Logger?.ILog($"Temporary PowerShell file created: {ps1File}");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{ps1File}\"",
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
            args.FailureReason = "Failed executing PowerShell script: " + ex.Message;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        finally
        {
            try
            {
                if (System.IO.File.Exists(ps1File))
                {
                    System.IO.File.Delete(ps1File);
                    args.Logger?.ILog($"Temporary ps1 file deleted: {ps1File}");
                }
            }
            catch (Exception ex)
            {
                args.Logger?.WLog($"Failed to delete temporary ps1 file: {ps1File}. Error: {ex.Message}");
            }
        }
    }
}