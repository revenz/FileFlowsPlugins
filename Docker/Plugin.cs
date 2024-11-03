namespace FileFlows.Docker;

/// <summary>
/// Plugin Info
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("1df9239a-3ce5-44b1-9113-3cdcae980a69");
    /// <inheritdoc />
    public string Name => "Docker";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";

    /// <inheritdoc />
    public string Icon => "fab fa-docker";

    /// <inheritdoc />
    public void Init() { }
}