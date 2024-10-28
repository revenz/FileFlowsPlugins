// using System.Text;
// using System.Text.Json;
// using System.Text.RegularExpressions;
// using FileFlows.Plugin;
// using FileFlows.Plugin.Attributes;
// using FileFlows.Plugin.Helpers;
// using MetaNodes.Helpers;
//
// namespace MetaNodes.AniListElements;
//
// /// <summary>
// /// Anime Episode Lookup using AniList API
// /// </summary>
// public class AnimeEpisodeLookup : Node
// {
//     /// <inheritdoc />
//     public override int Inputs => 1;
//     /// <inheritdoc />
//     public override int Outputs => 2;
//     /// <inheritdoc />
//     public override FlowElementType Type => FlowElementType.Logic;
//     /// <inheritdoc />
//     public override string HelpUrl => "https://fileflows.com/docs/plugins/meta-nodes/anime-episode-lookup";
//     /// <inheritdoc />
//     public override string Icon => "fas fa-dragon";
//
//     private Dictionary<string, object> _Variables;
//
//     /// <summary>
//     /// Gets the Variables this flow element provides
//     /// </summary>
//     public override Dictionary<string, object> Variables => _Variables;
//
//     /// <summary>
//     /// Constructs a new instance of this flow element
//     /// </summary>
//     public AnimeEpisodeLookup()
//     {
//         _Variables = new Dictionary<string, object>()
//         {
//             { "tvshow.Title", "Attack on Titan" },
//             { "tvshow.TitleRomaji", "Shingeki no Kyojin" },
//             { "tvshow.TitleEnglish", "Attack on Titan" },
//             { "tvshow.TitleNative", "進撃の巨人" },
//             { "tvshow.Year", 2016 },
//             { "tvshow.Description", "Several hundred years ago, humans were nearly exterminated by titans." },
//             { "tvshow.Score", 84 }
//         };
//     }
//
//     /// <summary>
//     /// Gets or sets if the folder name should be used
//     /// </summary>
//     [Boolean(1)]
//     public bool UseFolderName { get; set; }
//
//     /// <summary>
//     /// Executes the flow element
//     /// </summary>
//     /// <param name="args">The node parameters</param>
//     /// <returns>The output to call next</returns>
//     public override int Execute(NodeParameters args)
//     {
//         string filename = FileHelper.GetShortFileNameWithoutExtension(args.LibraryFileName);
//         
//         var helper = new TVShowHelper(args);
//         (string lookupName, string year) = helper.GetLookupName(args.LibraryFileName, UseFolderName);
//
//         (string showName, int? season, int? episode, int? lastEpisode, string year2) = helper.GetTVShowInfo(filename);
//         if (season == null)
//         {
//             args.Logger?.WLog("Season not found in string: " + filename);
//             return 2;
//         }
//         if (episode == null)
//         {
//             args.Logger?.WLog("Episode not found in string: " + filename);
//             return 2;
//         }
//         args.Logger?.ILog($"Found show info from filename '{lookupName}' season '{season}' episode '{(episode + (lastEpisode == null ? "" : "-" + lastEpisode))}'");
//
//
//         // Send query to AniList
//         var aniList = new AniList.AniListInterface(args);
//         var showInfo = aniList.FetchShowInfo(lookupName).Result;
//         var episodes = aniList.FetchEpisode(lookupName);//, season.Value, episode.Value, (lastEpisode ?? episode)!.Value).Result;
//
//         if (showInfo != null)
//         {
//             args.Variables["tvshow.Title"] = showInfo.Title;
//             args.Variables["tvshow.TitleRomaji"] = showInfo.TitleRomaji;
//             args.Variables["tvshow.TitleEnglish"] = showInfo.TitleEnglish;
//             args.Variables["tvshow.TitleNative"] = showInfo.TitleNative;
//             args.Variables["tvshow.Year"] = showInfo.Year;
//             args.Variables["tvshow.Description"] = showInfo.Description;
//             args.Variables["tvshow.Score"] = showInfo.Score;
//             args.Variables["VideoMetadata"] = showInfo;
//
//             args.Logger?.ILog($"Found TV Show: {showInfo.Title} ({showInfo.Year})");
//             return 1; // success output
//         }
//         else
//         {
//             args.Logger?.WLog($"TV Show '{lookupName}' not found.");
//             return 2; // failure output
//         }
//     }
//
//
// }