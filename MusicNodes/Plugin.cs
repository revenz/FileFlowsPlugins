namespace FileFlows.MusicNodes;

using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin.Attributes;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("d84fbd06-f0e3-4827-8de0-6b0ef20dd883");
    public string Name => "Music Nodes (Obsolete)";
    public string MinimumVersion => "1.0.4.2019";

    public void Init()
    {
    }
}
