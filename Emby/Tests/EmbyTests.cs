#if DEBUG

using FileFlows.Emby.MediaManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace FileFlows.Emby.Tests;

[TestClass]
public class EmbyTests : TestBase
{
    private NodeParameters Args = null!;
    private EmbyUpdater element = null!;
    private MockHttpMessageHandler mockHandler = null!;
    private HttpClient client = null!;

    protected override void TestStarting()
    {
        Args = GetNodeParameters("/media/movies/Movie.mp4");
        Args.IsDirectory = false;
        Args.PathUnMapper = path => path;
        Args.GetPluginSettingsJson = _ => string.Empty;
        Args.Logger = new TestLogger();

        mockHandler = new MockHttpMessageHandler();
        
        client = new HttpClient(mockHandler);

        element = new EmbyUpdater
        {
            ServerUrl = "http://emby.test/",
            AccessToken = "access-token",
            Http = client
        };
    }

    [TestMethod]
    public void Emby_Basic()
    {
        var itemId = "abc123";
        
        mockHandler.When(req => req.Method == HttpMethod.Get && 
                                req.RequestUri!.ToString().StartsWith("http://emby.test/Items"))
            .Respond(HttpStatusCode.OK, new
            {
                Items = new[] { new { Id = "abc123", Path = "/media/movies" } }
            });
        
        mockHandler.When(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().StartsWith($"http://emby.test/Items/{itemId}/Refresh"))
            .Respond(HttpStatusCode.OK, "Refreshed");

        mockHandler.When("http://emby.test/Library/Media/Updated")
            .Respond(HttpStatusCode.OK, new { });

        var result = element.Execute(Args);

        Assert.AreEqual(1, result);
        Assert.IsTrue(((TestLogger)Args.Logger!).Contains("Successfully updated Emby"));
    }

    [TestMethod]
    public void Emby_NoItemFound()
    {
        mockHandler.When("http://emby.test/Items*")
            .Respond(HttpStatusCode.OK, new { Items = Array.Empty<object>() });

        var result = element.Execute(Args);

        Assert.AreEqual(2, result);
        Assert.IsTrue(((TestLogger)Args.Logger!).Contains("Failed to find item ID"));
    }

    [TestMethod]
    public void Emby_RefreshFails()
    {
        var itemId = "abc123";

        mockHandler.When(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().StartsWith("http://emby.test/Items"))
            .Respond(HttpStatusCode.OK, new
            {
                Items = new[] { new { Id = itemId, Path = "/media/movies" } }
            });

        mockHandler.When(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().StartsWith($"http://emby.test/Items/{itemId}/Refresh"))
            .Respond(HttpStatusCode.InternalServerError, "Oops");

        var result = element.Execute(Args);

        Assert.AreEqual(2, result);
        Assert.IsTrue(((TestLogger)Args.Logger!).Contains("Failed to update Emby"));
    }
    
    // [TestMethod]
    // public void Emby_LiveTest()
    // {
    //     // Replace these with your real Emby server URL and access token
    //     string realServerUrl = Environment.GetEnvironmentVariable("EMBY_SERVER_URL") ?? "emby.server";
    //     string realAccessToken = Environment.GetEnvironmentVariable("EMBY_ACCESS_TOKEN") ?? "emby.acces.token9";
    //
    //     var liveArgs = GetNodeParameters("/media/tv/Seinfeld/Season 3/Seinfeld - S03E09 - The Nose Job.mkv"); // adjust path accordingly
    //     liveArgs.GetPluginSettingsJson = _ => JsonSerializer.Serialize(new
    //     {
    //         ServerUrl = realServerUrl,
    //         AccessToken = realAccessToken,
    //         Mapping = new List<KeyValuePair<string, string>>()
    //     });
    //
    //     var liveElement = new EmbyUpdater()
    //     {
    //         ServerUrl = realServerUrl,
    //         AccessToken = realAccessToken
    //     };
    //
    //     int result = liveElement.Execute(liveArgs);
    //
    //     Console.WriteLine($"Live Emby update result: {result}");
    //     Assert.IsTrue(result == 1 || result == 2, "Expected success (1) or refresh fail (2) result");
    // }
}
#endif
