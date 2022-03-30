namespace MetaNodes
{
    using System.ComponentModel.DataAnnotations;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        public string Name => "Meta Nodes";
        public string MinimumVersion => "0.4.3.660";

        public void Init() { }
    }
}