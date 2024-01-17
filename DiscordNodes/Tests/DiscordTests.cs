#if(DEBUG)

using FileFlows.DiscordNodes.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.DiscordNodes.Tests;

[TestClass]
public class DiscordTests
{
    [TestMethod]
    public void Discord_Simple_Message()
    {
        var args = new NodeParameters("test.file", new TestLogger(), false, string.Empty, null);;
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new Discord();
        node.Message = "a message\nwith\nsome\nnewlines";
        var result = node.Execute(args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Discord_Basic_Message()
    {
        var args = new NodeParameters("test.file", new TestLogger(), false, string.Empty, null);;
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new Discord();
        node.Message = "a message";
        node.MessageType = "Basic";
        Assert.AreEqual(1, node.Execute(args));
    }
}

#endif