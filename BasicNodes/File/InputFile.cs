namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class InputFile : Node
    {
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Input;
        public override string Icon => "far fa-file";
    }
}