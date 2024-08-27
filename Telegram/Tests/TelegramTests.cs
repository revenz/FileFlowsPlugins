#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Telegram.Tests;

[TestClass]
public class TelegramTests : TestBase
{
    /// <summary>
    /// Tests a basic success
    /// </summary>
    [TestMethod]
    public void Success()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"BotToken": "bot-token", "ChatId": "chat-id" }""";
        args.RenderTemplate = template => template;

        var element = new Communication.Telegram();
        element.SendMessage = (botToken, chatId,message)
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
        args.GetPluginSettingsJson = _ => """{"BotToken": "bot-token", "ChatId": "chat-id" }""";
        args.RenderTemplate = template => template;

        var element = new Communication.Telegram();
        element.SendMessage = (botToken, chatId,message)
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

        var element = new Communication.Telegram();
        element.SendMessage = (botToken, chatId,message)
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
        args.GetPluginSettingsJson = _ => """{ "ChatId": "chat-id" }""";
        args.RenderTemplate = template => template;

        var element = new Communication.Telegram();
        element.SendMessage = (botToken, chatId,message)
            => (true, "sent");
        element.Message = "a message";
        Assert.AreEqual(2, element.Execute(args));
    }
    
    /// <summary>
    /// Tests a no chat id fails success
    /// </summary>
    [TestMethod]
    public void NoChatId()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"BotToken": "bot-token" }""";
        args.RenderTemplate = template => template;

        var element = new Communication.Telegram();
        element.SendMessage = (botToken, chatId,message)
            => (true, "sent");
        element.Message = "a message";
        Assert.AreEqual(2, element.Execute(args));
    }
}

#endif