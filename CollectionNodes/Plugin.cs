using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollectionNodes;

public class Plugin : IPlugin
{
    public Guid Uid => new Guid("e62e3b2e-5147-4732-92df-f6fbbdb3bb08");
    public string Name => "Collection Nodes";
    public string MinimumVersion => "0.6.3.1000";

    [Folder(1)]
    [Required]
    public string DataDirectory { get; set; }

    public void Init()
    {
    }
}
