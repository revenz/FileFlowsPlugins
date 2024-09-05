#if(DEBUG)

using FileFlows.Apprise.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PluginTestLibrary;

namespace FileFlows.Apprise.Tests;

[TestClass]
public class AppriseTests : TestBase
{
    private Mock<IAppriseApi> mockApi = new();
    private NodeParameters Args = null!;
    
    protected override void TestStarting()
    {
        Args = new NodeParameters(TempFile, Logger, false, string.Empty, new LocalFileService());
        Args.RenderTemplate = (arg) => arg;
    }


    [TestMethod]
    public void Apprise_Basic_All()
    {
        var node = new FileFlows.Apprise.Communication.Apprise();
        node.Api = mockApi.Object;
        mockApi.Setup(x => x.Send(It.IsAny<ILogger>(), It.IsAny<string?>(), It.IsAny<string[]?>(), It.IsAny<string>()))
            .Returns(true);
        node.Message = "a message";
        Assert.AreEqual(1, node.Execute(Args));
    }

    [TestMethod]
    public void Apprise_Basic_Valid()
    {
        var node = new FileFlows.Apprise.Communication.Apprise();
        node.Api = mockApi.Object;
        mockApi.Setup(x => x.Send(It.IsAny<ILogger>(), It.IsAny<string?>(), It.IsAny<string[]?>(), It.IsAny<string>()))
            .Returns(true);
        node.Message = "a message";
        node.Tag = [ "test" ];
        Assert.AreEqual(1, node.Execute(Args));
    }

    [TestMethod]
    public void Apprise_Basic_Invalid()
    {
        var node = new FileFlows.Apprise.Communication.Apprise();
        node.Api = mockApi.Object;
        mockApi.Setup(x => x.Send(It.IsAny<ILogger>(), It.IsAny<string?>(), It.IsAny<string[]?>(), It.IsAny<string>()))
            .Returns(false);

        node.Message = "a message";
        node.Tag = [ "invalid" ];
        Assert.AreEqual(2, node.Execute(Args));
    }
}

#endif