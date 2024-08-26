#if(DEBUG)

using FileFlows.DiscordNodes.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PluginTestLibrary;

namespace FileFlows.DiscordNodes.Tests;

[TestClass]
public class DiscordTests : TestBase
{ 
    private Mock<IDiscordApi> mockApi = new();
    private NodeParameters Args = null!;
    
    protected override void TestStarting()
    {
        Args = new NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        Args.RenderTemplate = (arg) => arg;
    }
    
    [TestMethod]
    public void Discord_Simple_Message()
    {
        var node = new Discord();
        node.Api = mockApi.Object;
        node.Message = "a message\nwith\nsome\nnewlines";
        node.MessageType = "Information";
        bool sent = false;
        mockApi.Setup(x =>
                x.SendAdvanced(It.IsAny<ILogger>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() =>
            {
                sent = true;
            })
            .Returns(true);
        var result = node.Execute(Args);
        Assert.AreEqual(1, result);
        Assert.IsTrue(sent);
    }

    [TestMethod]
    public void Discord_Basic_Message()
    {
        var node = new Discord();
        node.Api = mockApi.Object;

        node.Message = "a message";
        node.MessageType = "Basic";
        bool sent = false;
        mockApi.Setup(x =>
                x.SendBasic(It.IsAny<ILogger>(), It.IsAny<string>()))
            .Callback(() =>
            {
                sent = true;
            })
            .Returns(true);
        Assert.AreEqual(1, node.Execute(Args));
        Assert.IsTrue(sent);
    }
}

#endif