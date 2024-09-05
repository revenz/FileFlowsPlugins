#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Pushbullet.Tests;

/// <summary>
/// Push bullet tests
/// </summary>
[TestClass]
public class PushbulletTests : TestBase
{
    /// <summary>
    /// Tests a basic success
    /// </summary>
    [TestMethod]
    public void Success()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"ApiToken": "api-token" }""";
        args.RenderTemplate = template => template;

        var element = new FileFlows.Pushbullet.Communication.Pushbullet();
        element.GetWebRequest = (accessToken, message, title)
            => (true, "sent");
        element.Message = "a message";
        Assert.AreEqual(1, element.Execute(args));
    }
    
    /// <summary>
    /// Tests a basic success
    /// </summary>
    [TestMethod]
    public void Fail()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"ApiToken": "api-token" }""";
        args.RenderTemplate = template => template;

        var element = new FileFlows.Pushbullet.Communication.Pushbullet();
        element.GetWebRequest = (accessToken, message, title)
            => (false, "sent");
        element.Message = "a message";
        Assert.AreEqual(2, element.Execute(args));
    }
    
    /// <summary>
    /// Tests a basic success
    /// </summary>
    [TestMethod]
    public void NoSettings()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => string.Empty;
        args.RenderTemplate = template => template;

        var element = new FileFlows.Pushbullet.Communication.Pushbullet();
        element.GetWebRequest = (accessToken, message, title)
            => (true, "sent");
        element.Message = "a message";
        Assert.AreEqual(2, element.Execute(args));
    }
}

#endif