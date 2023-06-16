namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// The FFmpeg Builder PreExecutor
/// </summary>
public class PreExecutor
{
    private readonly string code;
    private readonly NodeParameters args;
    /// <summary>
    /// Gets or sets the FFmpeg Arguments
    /// </summary>
    public List<string> Args { get; set; }
    
    /// <summary>
    /// Constructs an instance of the pre-executor
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="code">the code to run</param>
    /// <param name="ffArgs">the current FFmpeg arguments</param>
    public PreExecutor(NodeParameters args, string code, List<string> ffArgs)
    {
        this.args = args;
        this.code = code;
        this.Args = ffArgs;
    }

    /// <summary>
    /// Runs the pre-executor
    /// </summary>
    /// <returns>if the execution was successful</returns>
    public bool Run()
    {
        try
        {
            int exitCode = args.ScriptExecutor.Execute(new FileFlows.Plugin.Models.ScriptExecutionArgs
            {
                Args = args,
                Code = code + "\n\n// automatically added return code\nreturn 1;",
                AdditionalArguments = new ()
                {
                    { "FFmpeg", this }
                }
            });
            args.Logger.ILog("PreExecute Exit Code: " + exitCode);
            return exitCode >= 0;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed executing pre-executor: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return false;
        }
    }
}