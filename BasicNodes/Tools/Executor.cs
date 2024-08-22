using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Executes a command
/// </summary>
public class Executor : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-terminal";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/executor";

    internal const string VariablePattern = @"(^[\s]*$)|(^([a-zA-Z_]+)[a-zA-Z_0-9]*$)";

    [Required]
    [File(1)]
    public string FileName { get; set; }

    [Required]
    [TextVariable(2)]
    public string Arguments { get; set; }

    [Folder(3)]
    public string WorkingDirectory { get; set; }

    [Required]
    [NumberInt(4)]
    public int SuccessCode { get; set; }

    [NumberInt(5)]
    public int Timeout { get; set; }

    [Text(6)]
    [System.ComponentModel.DataAnnotations.RegularExpression(VariablePattern)]
    public string OutputVariable { get; set; }

    [Text(7)]
    [System.ComponentModel.DataAnnotations.RegularExpression(VariablePattern)]
    public string OutputErrorVariable { get; set; }

    private NodeParameters args;

    /// <inheritdoc />
    public override Task Cancel()
    {
        args?.Process?.Cancel();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        this.args = args;
        string pArgs = args.ReplaceVariables(Arguments ?? string.Empty);
        string filename = args.ReplaceVariables(FileName ?? string.Empty, stripMissing: true);
        string workingDirectory = args.ReplaceVariables(WorkingDirectory ?? string.Empty, stripMissing: true);
        var task = args.Process.ExecuteShellCommand(new ExecuteArgs
        {
            Command = filename,
            Arguments = pArgs,
            Timeout = Timeout,
            WorkingDirectory = workingDirectory
        });

        task.Wait();

        if(task.Result.Completed == false)
        {
            args.Logger?.ELog("Process failed to complete");
            return -1;
        }
        bool success = task.Result.ExitCode == this.SuccessCode;
        if(string.IsNullOrWhiteSpace(OutputVariable) == false && Regex.IsMatch(OutputVariable, VariablePattern))
        {
            args.UpdateVariables(new Dictionary<string, object>
            {
                { OutputVariable, task.Result.StandardOutput }
            });
        }
        if (string.IsNullOrWhiteSpace(OutputErrorVariable) == false && Regex.IsMatch(OutputErrorVariable ?? string.Empty, VariablePattern))
        {
            args.UpdateVariables(new Dictionary<string, object>
            {
                { OutputErrorVariable, task.Result.StandardError }
            });
        }
        if (success)
            return 1;
        else
        {
            args.Logger?.ILog("Unsuccesful exit code returned: " + task.Result.ExitCode);
            return 2;
        }
    }


}