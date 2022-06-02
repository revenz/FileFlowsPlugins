namespace FileFlows.BasicNodes
{
    using System.ComponentModel.DataAnnotations;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        public Guid Uid => new Guid("789b5213-4ca5-42da-816e-f2117f00cd16");
        public string Name => "Basic Nodes";
        public string MinimumVersion => "0.6.3.1000";

        public void Init() { }
    }
}