// #if(DEBUG)
//
// using System.Text.Json;
// using FileFlows.Plex.MediaManagement;
// using FileFlows.Plex.Models;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using PluginTestLibrary;
//
// namespace FileFlows.Plex.Tests;
//
// [TestClass]
// public class PlexAnayzeTests : TestBase
// {
//     [TestMethod]
//     public void PlexAnayze_Basic()
//     {
//         var args = GetNodeParameters(TempFile);
//         args.GetPluginSettingsJson = _ => """{"AccessToken": "access-token", "ServerUrl": "http://plex.test" }""";
//
//         var element = new PlexAnalyze();
//         element.GetWebRequest = (_, url) =>
//         {
//             if (url.Contains("library/sections"))
//                 return (true, JsonSerializer.Serialize(new PlexSections()
//                 {
//                     MediaContainer = new()
//                     {
//                         Metadata = [
//                             new ()
//                             {
//                                 Key = "metatdata",
//                                 Media = [
//                                     new ()
//                                     {
//                                         Id = 123,
//                                         RatingKey = "rating-key"
//                                     }
//                                 ]
//                             }
//                         ],
//                         Directory =
//                         [
//                             new()
//                             {
//                                 Location =
//                                 [
//                                     new()
//                                     {
//                                         Id = 123,
//                                         Path = TempPath
//                                     }
//                                 ]
//                             }
//                         ]
//                     },
//                     
//                 }));
//             return (true, "");
//         };
//         
//         Assert.AreEqual(1, element.Execute(args));
//     }
//
//     [TestMethod]
//     public void PlexAnayze_Fail()
//     {
//         var args = new NodeParameters(@"/media/tv/Outrageous Fortune/Season 3/Outrageous Fotune - 3x02a.mkv",
//             Logger, false, string.Empty, new LocalFileService());
//         args.GetPluginSettingsJson = (string input) =>
//         {
//             return File.ReadAllText("../../../settings.json");
//         };
//
//         var node = new PlexAnalyze();
//         Assert.AreEqual(2, node.Execute(args));
//     }
//
//     [TestMethod]
//     public void PlexAnayze_Mapping()
//     {
//         var args = new NodeParameters(@"/mnt/movies/The Batman (2022)/The Batman (2022).mkv", 
//             Logger, false, string.Empty, new LocalFileService());
//         var settings = new PluginSettings();
//         settings.Mapping = new List<KeyValuePair<string, string>>();
//         settings.Mapping.Add(new KeyValuePair<string, string>("/mnt/movies", "/media/movies"));
//
//         args.GetPluginSettingsJson = (string input) =>
//         {
//             return File.ReadAllText("../../../settings.json");
//         };
//
//         var node = new PlexAnalyze();
//         Assert.AreEqual(1, node.Execute(args));
//     }
//
// }
//
// #endif