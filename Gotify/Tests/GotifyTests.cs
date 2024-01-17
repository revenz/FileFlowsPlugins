#if(DEBUG)

using FileFlows.Gotify.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Gotify.Tests;

[TestClass]
public class GotifyTests
{
    [TestMethod]
    public void Gotify_Basic_Message()
    {
        var args = new NodeParameters("test.file", new TestLogger(), false, string.Empty, null);;
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new FileFlows.Gotify.Communication.Gotify();
        node.Message = "a message";
        Assert.AreEqual(1, node.Execute(args));
    }
}

#endif