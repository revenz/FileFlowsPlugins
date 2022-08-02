namespace FileFlows.Apprise;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("32d0e2ad-7617-4b52-bc39-338d2cfe468c");
    public string Name => "Apprise Nodes";
    public string MinimumVersion => "1.0.0.1864";

    public void Init()
    {
    }
}
