#if(DEBUG)

using System.Text.Json;
using FileFlows.Plex.MediaManagement;
using FileFlows.Plex.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace FileFlows.Plex.Tests;

[TestClass]
public class PlexUpdaterTests : TestBase
{
    [TestMethod]
    public void Plex_Basic()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"AccessToken": "access-token", "ServerUrl": "http://plex.test" }""";
        
        var element = new PlexUpdater();
        element.GetWebRequest = (_, url) =>
        {
            if (url.Contains("library/sections"))
                return (true, JsonSerializer.Serialize(new PlexSections()
                {
                    MediaContainer = new()
                    {
                        Directory =
                        [
                            new()
                            {
                                Location =
                                [
                                    new()
                                    {
                                        Id = 123,
                                        Path = TempPath
                                    }
                                ]
                            }
                        ]
                    }
                }));
            return (true, "");
        };
        Assert.AreEqual(1, element.Execute(args));
    }

    [TestMethod]
    public void Plex_Fail()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => """{"AccessToken": "access-token", "ServerUrl": "http://plex.test" }""";

        var node = new PlexUpdater();
        Assert.AreEqual(2, node.Execute(args));
    }

    [TestMethod]
    public void Plex_Mapping()
    {
        var args = GetNodeParameters(TempFile);
        args.GetPluginSettingsJson = _ => JsonSerializer.Serialize(new PluginSettings
        {
            AccessToken = "access-token",
            ServerUrl = "http://plex.test",
            Mapping = new List<KeyValuePair<string, string>>()
            {
                new(TempPath, "/media/movies")
            }
        });
        
        var element = new PlexUpdater();
        element.GetWebRequest = (_, url) =>
        {
            if (url.Contains("library/sections"))
                return (true, JsonSerializer.Serialize(new PlexSections()
                {
                    MediaContainer = new()
                    {
                        Directory =
                        [
                            new()
                            {
                                Location =
                                [
                                    new()
                                    {
                                        Id = 123,
                                        Path = "/media/movies"
                                    }
                                ]
                            }
                        ]
                    }
                }));
            return (true, "");
        };
        Assert.AreEqual(1, element.Execute(args));
    }

}

#endif