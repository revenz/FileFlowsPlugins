namespace FileFlows.Comic;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("3664da0a-b531-47b9-bdc8-e8368d9746ce");
    public string Name => "Comic Nodes";
    public string MinimumVersion => "1.0.0.1864";

    public void Init()
    {
    }
}
