using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollectionNodes
{
    public class Plugin : IPlugin
    {
        public string Name => "Collection Nodes";
        public string MinimumVersion => "0.6.1.900";

        [Folder(1)]
        [Required]
        public string DataDirectory { get; set; }

        public void Init()
        {
        }
    }
}