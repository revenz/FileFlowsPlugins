namespace FileFlows.Pdf;

/// <summary>
/// Plugin Information
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("af8fa0a9-53ad-457d-9729-14f1f996d477");
    /// <inheritdoc />
    public string Name => "PDF";
    /// <inheritdoc />
    public string MinimumVersion => "25.01.1.4200";
    /// <inheritdoc />
    public string Icon => "fas fa-file-pdf";
    
    /// <inheritdoc />
    public void Init()
    {
    }
}
