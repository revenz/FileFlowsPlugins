namespace ChecksumNodes;

public class Plugin : IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("5ce1524c-5e7b-40ee-9fc1-2152181490f1");
    /// <inheritdoc />
    public string Name => "Checksum Nodes";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "fas fa-file-contract";

    /// <inheritdoc />
    public void Init()
    {
    }
}
