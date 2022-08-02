namespace FileFlows.AudioNodes;

using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin.Attributes;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("d951a39e-4296-4801-ab41-4070b0789465");
    public string Name => "Audio Nodes";
    public string MinimumVersion => "1.0.0.1864";

    public void Init()
    {
    }
}
