namespace FileFlows.Comic;

public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("3664da0a-b531-47b9-bdc8-e8368d9746ce");
    /// <inheritdoc />
    public string Name => "Comic";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:comic";

    /// <inheritdoc />
    public void Init()
    {
    }
}
