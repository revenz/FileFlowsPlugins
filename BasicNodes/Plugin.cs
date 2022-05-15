namespace FileFlows.BasicNodes
{
    using System.ComponentModel.DataAnnotations;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        public string Name => "Basic Nodes";
        public string MinimumVersion => "0.6.1.900";

        public void Init() { }
    }
}