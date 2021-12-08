namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class Executor : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "fas fa-terminal";

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

        private NodeParameters args;

        public override async Task Cancel()
        {
            args?.Process?.Cancel();
        }

        public override int Execute(NodeParameters args)
        {
            this.args = args;
            string pArgs = args.ReplaceVariables(Arguments ?? string.Empty);
            var task = args.Process.ExecuteShellCommand(new ExecuteArgs
            {
                Command = FileName,
                Arguments = pArgs,
                Timeout = Timeout,
                WorkingDirectory = WorkingDirectory
            });

            task.Wait();

            if(task.Result.Completed == false)
            {
                args.Logger?.ELog("Process failed to complete");
                return -1;
            }
            bool success = task.Result.ExitCode == this.SuccessCode;
            if (success)
                return 1;
            else
            {
                args.Logger?.ILog("Unsuccesful exit code returned: " + task.Result.ExitCode);
                return 2;
            }
        }
    }
}