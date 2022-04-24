namespace FileFlows.Apprise
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class PluginSettings:IPluginSettings
    {
        [Text(1)]
        [Required]
        public string ServerUrl { get; set; }

        [Text(2)]
        [Required]
        public string Endpoint { get; set; }
    }
}
