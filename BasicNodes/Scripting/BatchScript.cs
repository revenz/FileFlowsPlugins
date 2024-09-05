using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Scripting;

/// <summary>
/// Flow element that executes a bat script
/// </summary>
public class BatchScript : ScriptBase
{
    /// <inheritdoc />
    public override string Icon => "svg:dos";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/scripting/batch-script";
    /// <inheritdoc />
    protected override ScriptLanguage Language => ScriptLanguage.Batch;
    
    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    [Required]
    [DefaultValue(@"
REM A Batch script can communicate with FileFlows to determine which output to call next by using exit codes.
REM Exit codes are used to determine the output, so:
REM Exit Code 0 corresponds to Finish Flow
REM Exit Code 1 corresponds to Output 1
REM Exit Code 2 corresponds to Output 2
REM and so on. Exit codes outside the defined range will be treated as a failure output.

REM Replace {file.FullName} and {file.Orig.FullName} with actual values
SET WorkingFile={file.FullName}
SET OriginalFile={file.Orig.FullName}

REM Example commands using the variables
echo Working on file: %WorkingFile%
echo Original file location: %OriginalFile%

REM Add your actual batch commands below
REM Example: Copy the working file to a backup location
REM copy ""%WorkingFile%"" ""C:\Backup\%~nxWorkingFile%""

REM Set the exit code to 1
EXIT /B 1
")]
    [Code(2, "bat")]
    public override string Code { get; set; }
}