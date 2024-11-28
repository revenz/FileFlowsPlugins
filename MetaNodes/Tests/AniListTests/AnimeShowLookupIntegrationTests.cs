#if(DEBUG)

using MetaNodes.AniList;
using MetaNodes.AniListElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace MetaNodes.Tests.AniList;

/// <summary>
/// AniList Anime Show Lookup Integration Tests
/// </summary>
[TestClass]
[TestCategory("Slow")]
public class AnimeShowLookupIntegrationTests : TestBase
{
    /// <summary>
    /// Tests the lookup of the anime show "Attack on Titan".
    /// </summary>
    [TestMethod]
    public void AttackOnTitan()
    {
        var parameters = GetNodeParameters("Attack on Titan (2013).mkv");

        var _animeShowLookup = new AnimeShowLookup();
        int result = _animeShowLookup.Execute(parameters);

        Assert.AreEqual(1, result); // Expect success
        Assert.AreEqual(2013, parameters.Variables["tvshow.Year"]);
        Assert.AreEqual("Attack on Titan", parameters.Variables["tvshow.Title"]);
        Assert.AreEqual("Attack on Titan", parameters.Variables["tvshow.TitleEnglish"]);
        Assert.AreEqual("Shingeki no Kyojin", parameters.Variables["tvshow.TitleRomaji"]);
        Assert.AreEqual("進撃の巨人", parameters.Variables["tvshow.TitleNative"]);
        Assert.IsTrue(parameters.Variables.ContainsKey("VideoMetadata"));
    }
    
    
    /// <summary>
    /// Tests the lookup of the anime show "My Hero Academia".
    /// </summary>
    [TestMethod]
    public void MyHeroAcademia()
    {
        var parameters = GetNodeParameters("My Hero Academia (2016).mkv");

        var _animeShowLookup = new AnimeShowLookup();
        int result = _animeShowLookup.Execute(parameters);

        Assert.AreEqual(1, result); // Expect success
        Assert.AreEqual(2016, parameters.Variables["tvshow.Year"]);
        Assert.AreEqual("My Hero Academia", parameters.Variables["tvshow.Title"]);
        Assert.AreEqual("My Hero Academia", parameters.Variables["tvshow.TitleEnglish"]);
        Assert.AreEqual("Boku no Hero Academia", parameters.Variables["tvshow.TitleRomaji"]);
        Assert.AreEqual("僕のヒーローアカデミア", parameters.Variables["tvshow.TitleNative"]);
        Assert.IsTrue(parameters.Variables.ContainsKey("VideoMetadata"));
    }

    /// <summary>
    /// Tests the lookup of the anime show "One Piece".
    /// </summary>
    [TestMethod]
    public void OnePiece()
    {
        var parameters = GetNodeParameters("One Piece (1999).mkv");

        var _animeShowLookup = new AnimeShowLookup();
        int result = _animeShowLookup.Execute(parameters);

        Assert.AreEqual(1, result); // Expect success
        Assert.AreEqual(1999, parameters.Variables["tvshow.Year"]);
        Assert.AreEqual("ONE PIECE", parameters.Variables["tvshow.Title"]);
        Assert.AreEqual("ONE PIECE", parameters.Variables["tvshow.TitleEnglish"]);
        Assert.AreEqual("ONE PIECE", parameters.Variables["tvshow.TitleRomaji"]);
        Assert.AreEqual("ONE PIECE", parameters.Variables["tvshow.TitleNative"]);
        Assert.IsTrue(parameters.Variables.ContainsKey("VideoMetadata"));
    }

    /// <summary>
    /// Tests the lookup of the anime show "Naruto".
    /// </summary>
    [TestMethod]
    public void Naruto()
    {
        var parameters = GetNodeParameters("Naruto (2002).mkv");

        var _animeShowLookup = new AnimeShowLookup();
        int result = _animeShowLookup.Execute(parameters);

        Assert.AreEqual(1, result); // Expect success
        Assert.AreEqual(2002, parameters.Variables["tvshow.Year"]);
        Assert.AreEqual("Naruto", parameters.Variables["tvshow.Title"]);
        Assert.AreEqual("Naruto", parameters.Variables["tvshow.TitleEnglish"]);
        Assert.AreEqual("NARUTO", parameters.Variables["tvshow.TitleRomaji"]);
        Assert.AreEqual("NARUTO -ナルト-", parameters.Variables["tvshow.TitleNative"]);
        Assert.IsTrue(parameters.Variables.ContainsKey("VideoMetadata"));
    }

    /// <summary>
    /// Tests the lookup of the anime show "Fullmetal Alchemist: Brotherhood".
    /// </summary>
    [TestMethod]
    public void FullmetalAlchemistBrotherhood()
    {
        var parameters = GetNodeParameters("Fullmetal Alchemist: Brotherhood (2009).mkv");

        var _animeShowLookup = new AnimeShowLookup();
        int result = _animeShowLookup.Execute(parameters);

        Assert.AreEqual(1, result); // Expect success
        Assert.AreEqual(2009, parameters.Variables["tvshow.Year"]);
        Assert.AreEqual("Fullmetal Alchemist: Brotherhood", parameters.Variables["tvshow.Title"]);
        Assert.AreEqual("Fullmetal Alchemist: Brotherhood", parameters.Variables["tvshow.TitleEnglish"]);
        Assert.AreEqual("Hagane no Renkinjutsushi: FULLMETAL ALCHEMIST", parameters.Variables["tvshow.TitleRomaji"]);
        Assert.AreEqual("鋼の錬金術師 FULLMETAL ALCHEMIST", parameters.Variables["tvshow.TitleNative"]);
        Assert.IsTrue(parameters.Variables.ContainsKey("VideoMetadata"));
    }
}

#endif