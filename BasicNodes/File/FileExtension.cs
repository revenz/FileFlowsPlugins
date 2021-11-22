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

        public override int Execute(NodeParameters args)
        {
            if (Extensions?.Any() != true)
            {
                args.Logger.ELog("No extensions specified");
                return -1;
            }

            foreach (var extension in Extensions)
            {
                if (string.IsNullOrEmpty(extension))
                    continue;
                if (args.WorkingFile.ToLower().EndsWith(extension.ToLower()))
                    return 1;
                if (args.FileName.ToLower().EndsWith(extension.ToLower()))
                    return 1;
            }
            return 2;

        
        }
    }
}