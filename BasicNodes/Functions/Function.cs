namespace FileFlows.BasicNodes.Functions
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;
    using System.Text.Json;

    public class Function : Node
    {
        public override int Inputs => 1;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string Icon => "fas fa-code";
        public override bool FailureNode => true;

        public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/function"; 

        [DefaultValue(1)]
        [NumberInt(1)]
        public new int Outputs { get; set; }

        [Required]
        [DefaultValue("// Custom javascript code that you can run against the flow file.\n// Flow contains helper functions for the Flow.\n// Variables contain variables available to this node from previous nodes.\n// Logger lets you log messages to the flow output.\n\n// return 0 to complete the flow.\n// return -1 to signal an error in the flow\n// return 1+ to select which output node will be processed next\n\nif(Variables.file.Size === 0)\n\treturn -1;\n\nreturn 0;")]
        [Code(2)]
        public string Code { get; set; }

        delegate void LogDelegate(params object[] values);
        public override int Execute(NodeParameters args)
        {
            if (string.IsNullOrEmpty(Code))
                return -1; // no code, flow cannot continue doesnt know what to do

            try
            {

                return args.ScriptExecutor.Execute(new FileFlows.Plugin.Models.ScriptExecutionArgs
                {
                    Args = args,
                    Code = Code
                });
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed executing function: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return -1;
            }
        }
    }
}