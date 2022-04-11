namespace FileFlows.MusicNodes
{
    using System.ComponentModel.DataAnnotations;
    using FileFlows.Plugin.Attributes;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        public string Name => "Music Nodes";
        public string MinimumVersion => "0.5.0.679";

        public void Init()
        {
        }
    }
}