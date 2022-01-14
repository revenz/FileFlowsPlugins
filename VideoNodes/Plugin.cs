namespace FileFlows.VideoNodes
{
    using System.ComponentModel.DataAnnotations;
    using FileFlows.Plugin.Attributes;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        public string Name => "Video Nodes";
        public string MinimumVersion => "0.3.2.394";

        public void Init()
        {
        }
    }
}