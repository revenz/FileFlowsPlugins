#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Telegram.Tests;

[TestClass]
public class TelegramTests
{
    [TestMethod]
    public void SendMessage()
    {
        string botToken = "sometoken";
        string chatId = "somechat";
        
        var result =
            Communication.Telegram.SendMessage(botToken, chatId, "this is a test");
        
        Assert.IsTrue(result);
    }
}

#endif