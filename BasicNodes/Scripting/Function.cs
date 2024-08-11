using System.ComponentModel;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// A flow element that executes custom code
/// </summary>
public class Function : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "svg:javascript";
    /// <inheritdoc />
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/scripting/function";
    /// <inheritdoc />
    public override string Group => "Scripting:0";
    
    /// <summary>
    /// Gets or sets the number of outputs
    /// </summary>
    [DefaultValue(1)]
    [NumberInt(1)]
    public new int Outputs { get; set; }

    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    [Required]
    [DefaultValue("// Custom javascript code that you can run against the flow file.\n// Flow contains helper functions for the Flow.\n// Variables contain variables available to this flow element from previous flow elements.\n// Logger lets you log messages to the flow output.\n\n// return 0 to complete the flow.\n// return -1 to signal an error in the flow\n// return 1+ to select which output node will be processed next\n\nif(Variables.file.Size === 0)\n\treturn -1;\n\nreturn 1;")]
    [Code(2)]
    public string Code { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrEmpty(Code))
        {
            args.FailureReason = "No code specified in Function script";
            args.Logger?.ELog(args.FailureReason);
            return -1; // no code, flow cannot continue doesn't know what to do
        }

        try
        {
            return args.ScriptExecutor.Execute(new FileFlows.Plugin.Models.ScriptExecutionArgs
            {
                Args = args,
                Logger = args.Logger,
                TempPath = args.TempPath,
                Language = ScriptLanguage.JavaScript,
                ScriptType = ScriptType.Flow,
                Code = Code
            });
        }
        catch (Exception ex)
        {
            args.FailureReason = "Failed executing function: " + ex.Message;
            args.Logger?.ELog("Failed executing function: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}
