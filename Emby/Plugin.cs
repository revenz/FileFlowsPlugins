namespace FileFlows.Emby;

public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("51bdd442-6630-4c8c-b3a4-70a2d1c60309");
    /// <inheritdoc />
    public string Name => "Emby";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:emby";

    /// <inheritdoc />
    public void Init()
    {
    }
}
