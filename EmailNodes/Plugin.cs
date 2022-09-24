namespace FileFlows.EmailNodes;

public class Plugin : IPlugin
{
    public Guid Uid => new Guid("b5077522-4a31-4faa-b9a7-b409ecb9c01e");
    public string Name => "Email";
    public string MinimumVersion => "1.0.4.2019";

    public void Init()
    {
    }
}
