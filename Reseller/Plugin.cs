namespace FileFlows.ResellerPlugin;

/// <summary>
/// Plugin Information
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("ba8cfaa3-4ac0-4a39-9e1b-a48def94eb3d");
    /// <inheritdoc />
    public string Name => "Reseller";
    /// <inheritdoc />
    public string MinimumVersion => "24.12.4.4168";
    /// <inheritdoc />
    public string Icon => "fas fa-people-carry";
    
    /// <inheritdoc />
    public void Init()
    {
    }
}
