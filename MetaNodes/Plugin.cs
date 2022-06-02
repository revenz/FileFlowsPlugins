namespace MetaNodes;

using System.ComponentModel.DataAnnotations;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("ed1e2547-6f92-4bc8-ae49-fcd7c74e7e9c");
    public string Name => "Meta Nodes";
    public string MinimumVersion => "0.6.3.1000";

    public void Init() { }
}
