namespace FileFlows.EmailNodes;

public class Plugin : IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("b5077522-4a31-4faa-b9a7-b409ecb9c01e");
    /// <inheritdoc />
    public string Name => "Email";
    /// <inheritdoc />
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "fas fa-envelope";

    /// <inheritdoc />
    public void Init()
    {
    }
}
