namespace FileFlows.BasicNodes.Functions
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using Jint.Runtime;
    using Jint.Native.Object;
    using Jint;
    using System.Text;

    public class Function : Node
    {
        public override int Inputs => 1;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string Icon => "fas fa-code";

        [DefaultValue(1)]
        [NumberIntAttribute(1)]
        public new int Outputs { get; set; }

        [DefaultValue("// VideoFile object contains info about the video file\n\n// return 0 to complete the flow.\n// return -1 to signal an error in the flow\n// return 1+ to indicate which output to process next\n\n return 0;")]
        [Code(2)]
        public string Code { get; set; }

        delegate void LogDelegate(params object[] values);
        public override int Execute(NodeParameters args)
        {
            args.Logger.DLog("Code: ", Environment.NewLine + new string('=', 40) + Environment.NewLine + Code + Environment.NewLine + new string('=', 40));
            if (string.IsNullOrEmpty(Code))
                return base.Execute(args); // no code, means will run fine... i think... maybe...  depends what i do

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
                options.MaxStatements(100);
            })
            .SetValue("Logger", args.Logger)
            .SetValue("FileSize", new FileInfo(args.WorkingFile).Length)
            //.SetValue("ILog", log.ILog)
            ;

            var result = engine.Evaluate(Code).ToObject();
            if (result as bool? != true)
                args.Result = NodeResult.Failure;

            return base.Execute(args);
        }
    }
}