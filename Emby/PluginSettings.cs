﻿namespace FileFlows.Emby
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class PluginSettings:IPluginSettings
    {
        [Text(1)]
        [Required]
        public string ServerUrl { get; set; }

        [Text(2)]
        [Required]
        public string AccessToken { get; set; }

        [KeyValue(3)]
        public List<KeyValuePair<string, string>> Mapping { get; set; }
    }
}
