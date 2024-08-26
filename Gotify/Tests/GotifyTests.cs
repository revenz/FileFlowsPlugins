#if(DEBUG)

using FileFlows.Gotify.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Gotify.Tests;

[TestClass]
public class GotifyTests : TestBase
{
    [TestMethod]
    public void Gotify_Basic_Message()
    {
        var args = new NodeParameters("test.file", Logger, false, string.Empty, new LocalFileService());
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