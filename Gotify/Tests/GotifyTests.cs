#if(DEBUG)

using FileFlows.Gotify.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Gotify.Tests;

/// <summary>
/// Goitfy Tests
/// </summary>
[TestClass]
public class GotifyTests : TestBase
{
    /// <summary>
    /// Performs the Gotify test
    /// </summary>
    /// <param name="message">the message</param>
    /// <param name="priority">the priority</param>
    /// <param name="title">the title</param>
    /// <param name="expectedPriority">the expected priority</param>
    private void Test(string message, int priority = 0, string? title = null, int? expectedPriority = null)
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"AccessToken": "access-token", "ServerUrl": "http://gotify.test" }""";
        args.RenderTemplate = (template) => template;

        var element = new FileFlows.Gotify.Communication.Gotify();
        element.Message = message;
        element.Priority = priority;
        element.Title = title ?? string.Empty;

        var expectedTitle = title ?? "FileFlows";
        expectedPriority ??= priority < 0 ? 2 : priority;
        element.GetWebRequest = (client, url, accessToken, json) =>
        {
            var request = System.Text.Json.JsonSerializer.Deserialize<GotifyMessage>(json)!;
            if (request.title != expectedTitle)
                return (false, "Unexpected title");
            if (request.message != "a message")
                return (false, "Unexpected message");
            if (request.priority != expectedPriority.Value)
                return (false, "Unexpected priority");
            return (true, "All Good");
        };
        Assert.AreEqual(1, element.Execute(args));
    }

    /// <summary>
    /// Tests the defaults
    /// </summary>
    [TestMethod]
    public void Gotify_Defaults()
        => Test("a message");

    /// <summary>
    /// Tests a custom priority
    /// </summary>
    [TestMethod]
    public void Gotify_CustomPriority()
        => Test("a message", priority: 10);

    /// <summary>
    /// Tests a custom title
    /// </summary>
    [TestMethod]
    public void Gotify_CustomTitle()
        => Test("a message", title: "My Custom Title");

    /// <summary>
    /// Tests a negative priority
    /// </summary>
    [TestMethod]
    public void Gotify_NegativePriority()
        => Test("a message", priority: -1, expectedPriority: 2);
    
    /// <summary>
    /// Gotify message
    /// </summary>
    /// <param name="title">the title of the message</param>
    /// <param name="message">the message</param>
    /// <param name="priority">the priority</param>
    private record GotifyMessage(string title, string message, int priority);
}

#endif