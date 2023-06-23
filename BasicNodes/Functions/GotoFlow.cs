namespace FileFlows.BasicNodes.Functions
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class GotoFlow : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 0;

        public override FlowElementType Type => FlowElementType.Logic;
        public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/goto-flow"; 
        public override string Icon => "fas fa-sitemap";

        [Select("FLOW_LIST", 1)]
        public ObjectReference Flow { get; set; }


        public override int Execute(NodeParameters args)
        {
            args.GotoFlow(Flow);
            return 0;
        }
    }
}