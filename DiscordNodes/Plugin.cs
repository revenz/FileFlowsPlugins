namespace FileFlows.DiscordNodes;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("ebaea108-8783-46b2-a889-be0d79bc8ad6");
    public string Name => "Discord";
    public string MinimumVersion => "1.0.4.2019";

    public void Init()
    {
    }
}
