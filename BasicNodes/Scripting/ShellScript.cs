using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BasicNodes.Scripting;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Scripting;

/// <summary>
/// Flow element that executes a Shell script
/// </summary>
public class ShellScript : ScriptBase
{
    /// <inheritdoc />
    public override string Icon => "svg:bash";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/shell-script";

    /// <inheritdoc />
    protected override ScriptLanguage Language => ScriptLanguage.Shell;

    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    [Required]
    [DefaultValue(@"
# A Shell script can communicate with FileFlows to determine which output to call next by using exit codes.
# Exit codes are used to determine the output, so:
# Exit Code 0 corresponds to Finish Flow
# Exit Code 1 corresponds to Output 1
# Exit Code 2 corresponds to Output 2
# and so on. Exit codes outside the defined range will be treated as a failure output.

# Replace {file.FullName} and {file.Orig.FullName} with actual values
WorkingFile=""{file.FullName}""
OriginalFile=""{file.Orig.FullName}""

# Example commands using the variables
echo ""Working on file: $WorkingFile""
echo ""Original file location: $OriginalFile""

# Add your actual shell commands below
# Example: Copy the working file to a backup location
# cp ""$WorkingFile"" ""/path/to/backup/$(basename \""$WorkingFile\"")""

# Set the exit code to 1
exit 1
")]
    [Code(2, "sh")]
    public override string Code { get; set; }
}
