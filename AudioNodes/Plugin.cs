namespace FileFlows.AudioNodes;

using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin.Attributes;

public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("d951a39e-4296-4801-ab41-4070b0789465");
    public string Name => "Audio";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:audio";

    /// <inheritdoc />
    public void Init()
    {
    }
}
