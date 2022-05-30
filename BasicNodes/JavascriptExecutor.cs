using FileFlows.Plugin;
using Jint;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BasicNodes;

/// <summary>
/// A Javascript code executor
/// </summary>
internal class JavascriptExecutor
{
    /// <summary>
    /// Delegate used by the executor so log messages can be passed from the javascript code into the flow runner
    /// </summary>
    /// <param name="values">the parameters for the logger</param>
    delegate void LogDelegate(params object[] values);

    /// <summary>
    /// Executes javascript
    /// </summary>
    /// <param name="execArgs">the execution arguments</param>
    /// <returns>the output to be called next</returns>
    public static int Execute(JavascriptExecutionArgs execArgs)
    {
        if (string.IsNullOrEmpty(execArgs?.Code))
            return -1; // no code, flow cannot continue doesnt know what to do
        var args = execArgs.Args;
        try
        {
            long fileSize = 0;
            var fileInfo = new FileInfo(args.WorkingFile);
            if (fileInfo.Exists)
                fileSize = fileInfo.Length;

            // replace Variables. with dictionary notation
            string tcode = execArgs.Code;
            foreach (string k in args.Variables.Keys.OrderByDescending(x => x.Length))
            {
                // replace Variables.Key or Variables?.Key?.Subkey etc to just the variable
                // so Variables.file?.Orig.Name, will be replaced to Variables["file.Orig.Name"] 
                // since its just a dictionary key value 
                string keyRegex = @"Variables(\?)?\." + k.Replace(".", @"(\?)?\.");


                object? value = args.Variables[k];
                if (value is JsonElement jElement)
                {
                    if (jElement.ValueKind == JsonValueKind.String)
                        value = jElement.GetString();
                    if (jElement.ValueKind == JsonValueKind.Number)
                        value = jElement.GetInt64();
                }

                tcode = Regex.Replace(tcode, keyRegex, "Variables['" + k + "']");
            }

            var sb = new StringBuilder();
            var log = new
            {
                ILog = new LogDelegate(args.Logger.ILog),
                DLog = new LogDelegate(args.Logger.DLog),
                WLog = new LogDelegate(args.Logger.WLog),
                ELog = new LogDelegate(args.Logger.ELog),
            };
            var engine = new Engine(options =>
            {
                options.LimitMemory(4_000_000);
                options.MaxStatements(500);
            })
            .SetValue("Logger", args.Logger)
            .SetValue("Variables", args.Variables)
            .SetValue("Flow", args);
            foreach (var arg in execArgs.AdditionalArguments)
                engine.SetValue(arg.Key, arg.Value);

            var result = int.Parse(engine.Evaluate(tcode).ToObject().ToString());
            return result;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed executing function: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}


/// <summary>
/// The arguments to pass to the javascript executor
/// </summary>
public class JavascriptExecutionArgs
{
    /// <summary>
    /// Gets or sets the code to execute
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets teh NodeParameters
    /// </summary>
    public NodeParameters Args { get; set; }

    /// <summary>
    /// Gets a collection of additional arguments to be passed to the javascript executor
    /// </summary>
    public Dictionary<string, object> AdditionalArguments { get; private set; } = new Dictionary<string, object>();
}