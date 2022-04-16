namespace FileFlows.DiscordNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class PluginSettings:IPluginSettings
    {
        [Text(1)]
        [Required]
        public string WebhookId { get; set; }

        [Text(2)]
        [Required]
        public string WebhookToken { get; set; }
    }
}
