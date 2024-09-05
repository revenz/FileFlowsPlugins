using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Scripting;

/// <summary>
/// Flow element that executes a PowerShell script
/// </summary>
public class PowerShellScript : ScriptBase
{
    /// <inheritdoc />
    public override string Icon => "svg:powershell";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/powershell-script";

    /// <inheritdoc />
    protected override ScriptLanguage Language => ScriptLanguage.PowerShell;
    
    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    [Required]
    [DefaultValue(@"
# A PowerShell script can communicate with FileFlows to determine which output to call next by using exit codes.
# Exit codes are used to determine the output, so:
# Exit Code 0 corresponds to Finish Flow
# Exit Code 1 corresponds to Output 1
# Exit Code 2 corresponds to Output 2
# and so on. Exit codes outside the defined range will be treated as a failure output.

# Replace {file.FullName} and {file.Orig.FullName} with actual values
$WorkingFile = '{file.FullName}'
$OriginalFile = '{file.Orig.FullName}'

# Example commands using the variables
Write-Output ""Working on file: $WorkingFile""
Write-Output ""Original file location: $OriginalFile""

# Add your actual PowerShell commands below
# Example: Copy the working file to a backup location
# Copy-Item -Path $WorkingFile -Destination ""C:\Backup\$([System.IO.Path]::GetFileName($WorkingFile))""

# Set the exit code to 1
exit 1
")]
    [Code(2, "powershell")]
    public override string Code { get; set; }
}