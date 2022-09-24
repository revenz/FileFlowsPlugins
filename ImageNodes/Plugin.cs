namespace FileFlows.ImageNodes;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("a6ddeee5-4c5a-46c5-80d5-e48552dd6a9b");
    public string Name => "Image Nodes";
    public string MinimumVersion => "1.0.4.2019";

    public void Init()
    {
    }
}
