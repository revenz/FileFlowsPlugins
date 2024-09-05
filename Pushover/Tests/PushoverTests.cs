#if(DEBUG)

using FileFlows.Pushover.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Pushover.Tests;

[TestClass]
public class PushoverTests : TestBase
{
    /// <summary>
    /// Tests a basic success
    /// </summary>
    [TestMethod]
    public void Success()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"ApiToken": "api-token", "UserKey":"user" }""";
        args.RenderTemplate = template => template;

        var element = new Communication.Pushover();
        element.GetWebRequest = (token, user,message, priority)
            => (true, "sent");
        element.Message = "a message";
        Assert.AreEqual(1, element.Execute(args));
    }
    
    /// <summary>
    /// Tests a basic failure
    /// </summary>
    [TestMethod]
    public void Fail()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"ApiToken": "api-token", "UserKey":"user" }""";
        args.RenderTemplate = template => template;

        var element = new Communication.Pushover();
        element.GetWebRequest = (token, user,message, priority)
            => (false, "sent");
        element.Message = "a message";
        Assert.AreEqual(2, element.Execute(args));
    }
    
    /// <summary>
    /// Tests a no settings fails
    /// </summary>
    [TestMethod]
    public void NoSettings()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => string.Empty;
        args.RenderTemplate = template => template;

        var element = new Communication.Pushover();
        element.GetWebRequest = (token, user,message, priority)
            => (true, "sent");
        element.Message = "a message";
        Assert.AreEqual(2, element.Execute(args));
    }
    
    /// <summary>
    /// Tests a no token fails
    /// </summary>
    [TestMethod]
    public void NoToken()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{ "UserKey":"user" }""";
        args.RenderTemplate = template => template;

        var element = new Communication.Pushover();
        element.GetWebRequest = (token, user,message, priority)
            => (true, "sent");
        element.Message = "a message";
        Assert.AreEqual(2, element.Execute(args));
    }
    
    /// <summary>
    /// Tests a no user fails success
    /// </summary>
    [TestMethod]
    public void NoUser()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"ApiToken": "api-token" }""";
        args.RenderTemplate = template => template;

        var element = new Communication.Pushover();
        element.GetWebRequest = (token, user,message, priority)
            => (true, "sent");
        element.Message = "a message";
        Assert.AreEqual(2, element.Execute(args));
    }
}

#endif