namespace MetaNodes
{
    using System.ComponentModel.DataAnnotations;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        public string Name => "Meta Nodes";
        public string MinimumVersion => "0.5.0.679";

        public void Init() { }
    }
}