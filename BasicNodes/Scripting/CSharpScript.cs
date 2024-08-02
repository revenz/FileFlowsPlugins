using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace BasicNodes.Scripting;

/// <summary>
/// Flow element that executes a CSharp script
/// </summary>
public class CSharpScript : ScriptBase
{
    /// <inheritdoc />
    public override string Icon => "svg:cs";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/charp-script";

    /// <inheritdoc />
    protected override ScriptLanguage Language => ScriptLanguage.CSharp;

    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    [Required]
    [DefaultValue(@"
// A C# script will have full access to the executing flow.
// Return the output to call next

// Replace these variables with actual values
string workingFile = Variables.file.FullName;
string originalFile = Variables.file.Orig.FullName;

// Example code using the variables
Console.WriteLine($""Working on file: {workingFile}"");
Console.WriteLine($""Original file location: {originalFile}"");

// Add your actual C# code below
return 1;
")]
    [Code(2, "csharp")]
    public override string Code { get; set; }
}
