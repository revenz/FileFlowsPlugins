#if(DEBUG)

using FileFlows.Emby.MediaManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Emby.Tests;

[TestClass]
public class EmbyTests : TestBase
{
    private NodeParameters Args = null!;
    private EmbyUpdater element = null!;
    
    protected override void TestStarting()
    {
        Args = GetNodeParameters(TempFile);
        Args.GetPluginSettingsJson = _ => string.Empty;
        element = new EmbyUpdater();
        element.ServerUrl = "http://emby.test/";
        element.AccessToken = "access-token";
        element.GetWebRequest = (client, url, accessToken, json) 
            => (true, "test");
    }

    [TestMethod]
    public void Emby_Basic()
    {
        Assert.AreEqual(1, element.Execute(Args));
    }

    [TestMethod]
    public void Emby_Fail()
    {
        element.GetWebRequest = (client, url, accessToken, json) 
            => (false, "cant reach");

        Assert.AreEqual(2, element.Execute(Args));
    }

    [TestMethod]
    public void Emby_Mapped()
    {
        element.Mapping = new List<KeyValuePair<string, string>>()
        {
            new(TempPath, "/mapped")
        };

        element.GetWebRequest = (client, url, accessToken, json) =>
        {
            var payload = System.Text.Json.JsonSerializer.Deserialize<EmbyPayload>(json);
            bool success = payload!.Updates[0].Path.StartsWith("/mapped");
            return (success, "mapped");
        };
        
        Assert.AreEqual(1, element.Execute(Args));
    }

    /// <summary>
    /// The emby payload
    /// </summary>
    internal class EmbyPayload
    {
        /// <summary>
        /// Gets or sets the emby payload updates
        /// </summary>
        public EmbyUpdate[] Updates { get; set; } = null!;
    }

    /// <summary>
    /// A emby update
    /// </summary>
    internal class EmbyUpdate
    {
        /// <summary>
        /// Gets or stees the path that is updated
        /// </summary>
        public string Path { get; set; } = null!;
    }
}

#endif