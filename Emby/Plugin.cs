namespace FileFlows.Emby;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("51bdd442-6630-4c8c-b3a4-70a2d1c60309");
    public string Name => "Emby";
    public string MinimumVersion => "0.6.3.1000";

    public void Init()
    {
    }
}
