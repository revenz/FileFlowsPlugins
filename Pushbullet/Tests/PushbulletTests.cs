#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Pushbullet.Tests;

[TestClass]
public class PushbulletTests : TestBase
{
    [TestMethod]
    public void Pushbullet_Basic_Message()
    {
        var args = new NodeParameters("test.file", Logger, false, string.Empty, new LocalFileService());
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