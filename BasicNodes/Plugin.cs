namespace FileFlows.BasicNodes
{
    using System.ComponentModel.DataAnnotations;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        public string Name => "Basic Nodes";
        public string MinimumVersion => "0.3.1.391";

        public void Init() { }
    }
}