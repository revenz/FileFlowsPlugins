namespace FileFlows.Apprise;

/// <summary>
/// Apprise Plugin
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("32d0e2ad-7617-4b52-bc39-338d2cfe468c");
    /// <inheritdoc />
    public string Name => "Apprise";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:apprise";

    /// <inheritdoc />
    public void Init()
    {
    }
}
