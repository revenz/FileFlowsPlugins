namespace MetaNodes;

using System.ComponentModel.DataAnnotations;

public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("ed1e2547-6f92-4bc8-ae49-fcd7c74e7e9c");
    /// <inheritdoc />
    public string Name => "Meta Nodes";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:database";

    /// <inheritdoc />
    public void Init() { }
}
