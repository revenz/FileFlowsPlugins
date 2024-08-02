using System.ComponentModel;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace BasicNodes.Scripting;

/// <summary>
/// Base for a script
/// </summary>
public abstract class ScriptBase : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override bool FailureNode => true;

    /// <summary>
    /// Gets or sets the number of outputs
    /// </summary>
    [DefaultValue(1)]
    [NumberInt(1)]
    public new int Outputs { get; set; }
    
    /// <summary>
    /// Gets the language of this script
    /// </summary>
    protected abstract ScriptLanguage Language { get; }
    
    /// <summary>
    /// Gets or sets the code of the script
    /// </summary>
    public virtual string Code { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrEmpty(Code))
        {
            args.FailureReason = $"No code specified in {Language} script";
            args.Logger?.ELog(args.FailureReason);
            return -1; // no code, flow cannot continue doesn't know what to do
        }

        var result = args.ScriptExecutor.Execute(new()
        {
            Args = args,
            Code = Language is ScriptLanguage.CSharp or ScriptLanguage.JavaScript ? Code : args.ReplaceVariables(Code),
            ScriptType = ScriptType.Flow,
            Language = Language
        });
        
        if (result.Failed(out var error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }

        if (result.Value > Outputs)
        {
            args.FailureReason = "Unexpected output: " + result.Value;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        return result.Value;
    }
}