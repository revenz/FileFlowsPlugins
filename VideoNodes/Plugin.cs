namespace FileFlows.VideoNodes;

using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin.Attributes;

public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("881b486b-4b38-4e66-b39e-fbc0fc9deee1");
    /// <inheritdoc />
    public string Name => "Video Nodes";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:video";

    /// <inheritdoc />
    public void Init()
    {
    }
}
