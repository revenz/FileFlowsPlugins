using BasicNodes;
using FileFlows.Plugin;
using System.Dynamic;

namespace FileFlows.BasicNodes;

/// <summary>
/// A special node that is not shown in the UI and only created
/// by the Flow Runner to execute a script.
/// This Node exists in this plugin to make use of the Javascript executor
/// </summary>
public class ScriptNode:Node
{
    /// <summary>
    /// Gets the number of inputs of this node
    /// </summary>
    public override int Inputs => 1;

    /// <summary>
    /// Gets or sets the model to pass to the node
    /// </summary>
    public ExpandoObject Model { get; set; }

    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    public string Code { get; set; }


    /// <summary>
    /// Executes the script node
    /// </summary>
    /// <param name="args">the NodeParameters passed into this from the flow runner</param>
    /// <returns>the output node to call next</returns>
    public override int Execute(NodeParameters args)
    {
        // will throw exception if invalid
        var script = new ScriptParser().Parse("ScriptNode", Code);

        // build up the entry point
        string epParams = string.Join(", ", script.Parameters?.Select(x => x.Name).ToArray());
        // all scripts must contain the "Script" method we then add this to call that 
        string entryPoint = $"Script({epParams});";

        var execArgs = new JavascriptExecutionArgs
        {
            Args = args,
            //Code = ("try\n{\n\t" + Code.Replace("\n", "\n\t") + "\n\n\t" + entryPoint + "\n} catch (err) { \n\tLogger.ELog(`Error in script [${err.line}]: ${err}`);\n\treturn -1;\n}").Replace("\t", "   ")
            Code = (Code + "\n\n" + entryPoint).Replace("\t", "   ").Trim()
        };

        if (script.Parameters?.Any() == true)
        {
            var dictModel = Model as IDictionary<string, object>;
            foreach (var p in script.Parameters) 
            {
                var value = dictModel?.ContainsKey(p.Name) == true ? dictModel[p.Name] : null;
                execArgs.AdditionalArguments.Add(p.Name, value);
            }
        }

        return JavascriptExecutor.Execute(execArgs);
    }
}
