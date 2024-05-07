#if(DEBUG)

using FileFlows.Pushbullet.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Pushbullet.Tests;

[TestClass]
public class PushbulletTests
{
    [TestMethod]
    public void Pushbullet_Basic_Message()
    {
        var args = new NodeParameters("test.file", new TestLogger(), false, string.Empty, null!);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../../../Pushbullet.json");
        };

        var node = new FileFlows.Pushbullet.Communication.Pushbullet();
        node.Message = "a message";
        Assert.AreEqual(1, node.Execute(args));
    }
}

#endif