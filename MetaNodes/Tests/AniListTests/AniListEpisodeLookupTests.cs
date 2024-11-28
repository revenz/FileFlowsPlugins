// #if(DEBUG)
//
// using MetaNodes.AniList;
// using MetaNodes.AniListElements;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using PluginTestLibrary;
//
// namespace MetaNodes.Tests.AniList;
//
//
// /// <summary>
// /// AniList Anime Episode Lookup Integration Tests
// /// </summary>
// [TestClass]
// public class AniListEpisodeLookupTests : TestBase
// {
//     /// <summary>
//     /// Tests the lookup of the anime show "Attack on Titan".
//     /// </summary>
//     [TestMethod]
//     public void AttackOnTitan()
//     {
//         var parameters = GetNodeParameters("My Hero Academia - 1x01-03.mkv");
//
//         var _animeShowLookup = new AnimeEpisodeLookup();
//         int result = _animeShowLookup.Execute(parameters);
//
//         Assert.AreEqual(1, result); // Expect success
//         Assert.AreEqual(2013, parameters.Variables["tvshow.Year"]);
//         Assert.AreEqual("Attack on Titan", parameters.Variables["tvshow.Title"]);
//         Assert.AreEqual("Attack on Titan", parameters.Variables["tvshow.TitleEnglish"]);
//         Assert.AreEqual("Shingeki no Kyojin", parameters.Variables["tvshow.TitleRomaji"]);
//         Assert.AreEqual("進撃の巨人", parameters.Variables["tvshow.TitleNative"]);
//         Assert.IsTrue(parameters.Variables.ContainsKey("VideoMetadata"));
//     }
//     
// }
//
// #endif