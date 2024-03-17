namespace FileFlows.Plex;

public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("5be72267-7574-4ba9-a958-f3dda0d6c2dc");
    /// <inheritdoc />
    public string Name => "Plex";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:plex";
    
    /// <inheritdoc />
    public void Init()
    {
    }
}
