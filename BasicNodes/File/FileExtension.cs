namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class FileExtension : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override string Icon => "far fa-file-excel";

        [StringArray(1)]
        public string[] Extensions { get; set; }
        public override FlowElementType Type => FlowElementType.Logic;
    }
}