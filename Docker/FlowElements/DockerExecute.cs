using System.ComponentModel.DataAnnotations;
using System.Text;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.Docker.FlowElements;

/// <summary>
/// Flow element that executes a Docker command
/// </summary>
public class DockerExecute: Node
{
    /// <inheritdoc />
    public override string Group => "Docker";

    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => AdditionalOutputs?.Any() == true ? (1 + AdditionalOutputs.Count) : 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fab fa-docker";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/docker-execute";

    /// <summary>
    /// Gets or sets the name of the docker image
    /// </summary>
    [Required]
    [TextVariable(1)]
    public string Image { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets volumes
    /// </summary>
    [KeyValue(2, null)]
    public List<KeyValuePair<string, string>> Volumes { get; set; } = [];
    
    /// <summary>
    /// Gets or sets additional outputs
    /// </summary>
    [StringArray(3)]
    public List<string> AdditionalOutputs { get; set; } = [];

    /// <summary>
    /// Gets or sets the docker command
    /// </summary>
    [Required]
    [TextArea(3, true)]
    public string Command { get; set; } = string.Empty;

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var command = args.ReplaceVariables(Command, stripMissing: true);
        command = command.Replace(args.TempPath, "/temp");

        string image = args.ReplaceVariables(Image, stripMissing: true);
        args.Logger?.ILog("Image: " + image);
        args.Logger?.ILog("Command: " + command);
        List<string> arguments =
        [
            "run",
            "--rm"
        ];
        foreach (var vol in Volumes ?? [])
        {
            arguments.AddRange(["-v", vol.Key.Trim() + ":" + vol.Value.Trim() ]);
        }

        arguments.AddRange(["-v", $"{args.TempPathHost}:/temp"]);
        arguments.Add(image);
        
        // Split the command string into individual arguments, considering quotes
        var commandArguments = SplitCommand(command)?.ToArray() ?? [];
        if (commandArguments.Length > 0)
        {
            arguments.AddRange(commandArguments);
            foreach (var arg in commandArguments)
            {
                args.Logger?.ILog("Arg: " + arg);
            }
        }

        var result = args.Execute(new()
        {
            Command = command,
            ArgumentList = arguments.ToArray()
        });

        if (AdditionalOutputs?.Any() == true)
        {
            for (int i = 0; i < AdditionalOutputs.Count;i++)
            {
                var text = args.ReplaceVariables(AdditionalOutputs[i], stripMissing: true);
                if (args.StringHelper.Matches(text, result.StandardOutput)
                    || args.StringHelper.Matches(text, result.StandardError))
                {
                    // + 2 since outputs are 1 based, and these are additional so above the standard 1 output
                    args.Logger?.ILog($"Additional output[{i + 2}] detected: {text}");
                    return i + 2;
                }
            }
        }
        args.Logger?.ILog("Exit Code: " + result.ExitCode);
        if (result.ExitCode != 0)
        {
            args.FailureReason = "Invalid exit code received: " + result.ExitCode;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        args.Logger?.ILog("Success exit code received");
        return 1;
    }
    

    /// <summary>
    /// Splits a command string into a list of arguments, handling quoted segments correctly.
    /// </summary>
    private static IEnumerable<string> SplitCommand(string command)
    {
        var args = new List<string>();
        var currentArg = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in command)
        {
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (currentArg.Length > 0)
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
            }
            else
            {
                currentArg.Append(c);
            }
        }

        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString());
        }

        return args;
    }
}