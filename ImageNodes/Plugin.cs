namespace FileFlows.ImageNodes;

public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("a6ddeee5-4c5a-46c5-80d5-e48552dd6a9b");
    /// <inheritdoc />
    public string Name => "Image";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:image";

    /// <inheritdoc />
    public void Init()
    {
    }
}
