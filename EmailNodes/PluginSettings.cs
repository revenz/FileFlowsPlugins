namespace FileFlows.EmailNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class PluginSettings:IPluginSettings
    {
        [Required]
        [Text(1)]
        public string SmtpServer { get; set; }

        [Range(1, 6555)]
        [NumberInt(1)]
        public int SmtpPort { get; set; }

        [Text(2)]
        public string SmtpUsername { get; set; }

        [Text(3)]
        public string SmtpPassword { get; set; }

        [Text(4)]
        [Required]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")]
        public string Sender { get; set; }
    }
}
