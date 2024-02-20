#if(DEBUG)

using FileFlows.Pushover.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Pushover.Tests;

[TestClass]
public class PushoverTests
{
    [TestMethod]
    public void Pushover_Basic_Message()
    {
        var args = new NodeParameters("test.file", new TestLogger(), false, string.Empty, null);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../../../pushover.json");
        };

        var node = new FileFlows.Pushover.Communication.Pushover();
        node.Message = "a message";
        Assert.AreEqual(1, node.Execute(args));
    }
}

#endif