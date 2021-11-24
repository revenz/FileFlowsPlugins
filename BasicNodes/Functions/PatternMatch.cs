namespace FileFlows.BasicNodes.Functions
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    public class PatternMatch : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string Icon => "fas fa-equals";

        [DefaultValue("")]
        [Text(1)]
        [Required]
        public string Pattern { get; set; }

        public override int Execute(NodeParameters args)
        {
            if (string.IsNullOrEmpty(Pattern))
                return 1; // no pattern, matches everything

            try
            {
                var rgx = new Regex(Pattern);
                if (rgx.IsMatch(args.WorkingFile) || rgx.IsMatch(args.FileName))
                    return 1;
                return 0;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Pattern error: " + ex.Message);
                return -1;
            }
        }
    }
}