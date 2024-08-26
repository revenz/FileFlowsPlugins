#if(DEBUG)

using FileFlows.Pushover.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Pushover.Tests;

[TestClass]
public class PushoverTests : TestBase
{
    [TestMethod]
    public void Pushover_Basic_Message()
    {
        var args = new NodeParameters("test.file", Logger, false, string.Empty, new LocalFileService());
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