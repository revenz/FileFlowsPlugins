using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollectionNodes
{
    public class Plugin : IPlugin
    {
        public string Name => "Collection Nodes";
        public string MinimumVersion => "0.5.0.683";

        [Folder(1)]
        [Required]
        public string DataDirectory { get; set; }

        public void Init()
        {
        }
    }
}