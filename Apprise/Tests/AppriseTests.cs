#if(DEBUG)

using FileFlows.Apprise.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Apprise.Tests;

[TestClass]
public class AppriseTests
{
    [TestMethod]
    public void Apprise_Basic_All()
    {
        var args = new NodeParameters("test.file", new TestLogger(), false, string.Empty);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new FileFlows.Apprise.Communication.Apprise();
        node.Message = "a message";
        Assert.AreEqual(1, node.Execute(args));
    }

    [TestMethod]
    public void Apprise_Basic_Valid()
    {
        var args = new NodeParameters("test.file", new TestLogger(), false, string.Empty);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new FileFlows.Apprise.Communication.Apprise();
        node.Message = "a message";
        node.Tag = new[] { "test" };
        Assert.AreEqual(1, node.Execute(args));
    }

    [TestMethod]
    public void Apprise_Basic_Invalid()
    {
        var args = new NodeParameters("test.file", new TestLogger(), false, string.Empty);
        args.GetPluginSettingsJson = (string input) =>
        {
            return File.ReadAllText("../../../settings.json");
        };

        var node = new FileFlows.Apprise.Communication.Apprise();
        node.Message = "a message";
        node.Tag = new[] { "invalid" };
        Assert.AreEqual(2, node.Execute(args));
    }
}

#endif