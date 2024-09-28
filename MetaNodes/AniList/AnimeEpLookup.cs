using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace MetaNodes.AniList
{
    public class TVEpisodeLookup : Node
    {
        private const string AniListGraphQLUrl = "https://graphql.anilist.co";

        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string HelpUrl => "https://fileflows.com/docs/plugins/meta-nodes/tv-episode-lookup";
        public override string Icon => "fas fa-tv";

        private Dictionary<string, object> _Variables;

        public override Dictionary<string, object> Variables => _Variables;

        public TVEpisodeLookup()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "tvepisode.Title", "Naruto" },
                { "tvepisode.Season", 1 },
                { "tvepisode.Episode", 1 },
                { "tvepisode.Year", 2002 },
                { "tvepisode.AirDate", new DateTime(2002, 10, 03) },
                { "tvepisode.Overview", "Naruto becomes a ninja." }
            };
        }

        [Boolean(1)]
        public bool UseFolderName { get; set; }

        public override async Task<int> Execute(NodeParameters args)
        {
            (string showName, string year, int season, int episode) = GetEpisodeDetails(args.LibraryFileName, UseFolderName);

            // Send query to AniList
            var showInfo = await FetchEpisodeInfoFromAniList(showName, season, episode);

            if (showInfo != null)
            {
                _Variables["tvepisode.Title"] = showInfo.Title;
                _Variables["tvepisode.Season"] = season;
                _Variables["tvepisode.Episode"] = episode;
                _Variables["tvepisode.Year"] = showInfo.Year;
                _Variables["tvepisode.Overview"] = showInfo.Description;
                _Variables["tvepisode.AirDate"] = showInfo.AirDate;

                args.Logger?.ILog($"Found Episode: {showInfo.Title}, Season {season}, Episode {episode}");
                return 1; // success output
            }
            else
            {
                args.Logger?.WLog($"Episode not found for '{showName}' (Season {season}, Episode {episode})");
                return 2; // failure output
            }
        }

        private async Task<EpisodeInfo> FetchEpisodeInfoFromAniList(string showName, int season, int episode)
        {
            var query = @"
            query ($search: String) {
              Media(search: $search, type: ANIME) {
                title {
                  romaji
                }
                episodes
                description
                startDate {
                  year
                }
                averageScore
                nextAiringEpisode {
                  airingAt
                }
              }
            }";

            var variables = new { search = showName };
            var jsonRequest = JsonSerializer.Serialize(new { query, variables });

            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(AniListGraphQLUrl, content);
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var data = JsonSerializer.Deserialize<AniListResponse>(jsonResponse);
                        var media = data?.Data?.Media;

                        if (media != null)
                        {
                            return new EpisodeInfo
                            {
                                Title = media.Title.Romaji,
                                Description = media.Description,
                                Year = media.StartDate.Year,
                                AirDate = media.NextAiringEpisode != null ? DateTimeOffset.FromUnixTimeSeconds(media.NextAiringEpisode.AiringAt).DateTime : null
                            };
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error fetching data from AniList: {jsonResponse}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                }
            }

            return null;
        }

        private (string, string, int, int) GetEpisodeDetails(string libraryFileName, bool useFolderName)
        {
            string showName;
            string year = null;
            int season = 1, episode = 1;

            if (useFolderName)
            {
                // Extract from folder name
                var folderName = Path.GetDirectoryName(libraryFileName);
                showName = ExtractShowNameFromPath(folderName);
                year = ExtractYearFromPath(folderName);
                season = ExtractSeasonFromPath(folderName);
                episode = ExtractEpisodeFromPath(folderName);
            }
            else
            {
                // Extract from file name
                var fileName = Path.GetFileNameWithoutExtension(libraryFileName);
                showName = ExtractShowNameFromPath(fileName);
                year = ExtractYearFromPath(fileName);
                season = ExtractSeasonFromPath(fileName);
                episode = ExtractEpisodeFromPath(fileName);
            }

            return (showName, year, season, episode);
        }

        private string ExtractShowNameFromPath(string path)
        {
            var showNamePattern = @"^(?<name>[\w\s\.\-\(\)]+?)(\s?[\(\.\-\_]\d{4}[\)\.\-\_])?";
            var match = Regex.Match(path, showNamePattern);
            if (match.Success)
            {
                return match.Groups["name"].Value.Trim(new[] { '.', ' ', '-', '_', '(', ')' });
            }

            return path;
        }

        private string ExtractYearFromPath(string path)
        {
            var yearPattern = @"(?:[\(\.\-\_\s])(?<year>(19|20)\d{2})(?:[\)\.\-\_\s])";
            var match = Regex.Match(path, yearPattern);
            return match.Success ? match.Groups["year"].Value : null;
        }

        private int ExtractSeasonFromPath(string path)
        {
            var seasonPattern = @"[Ss](eason)?[ \-]?(\d+)";
            var match = Regex.Match(path, seasonPattern);
            return match.Success ? int.Parse(match.Groups[2].Value) : 1; // Default to season 1 if not found
        }

        private int ExtractEpisodeFromPath(string path)
        {
            var episodePattern = @"[Ee](pisode)?[ \-]?(\d+)";
            var match = Regex.Match(path, episodePattern);
            return match.Success ? int.Parse(match.Groups[2].Value) : 1; // Default to episode 1 if not found
        }

        private class EpisodeInfo
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int Year { get; set; }
            public DateTime? AirDate { get; set; }
        }

        private class AniListResponse
        {
            public AniListData Data { get; set; }
        }

        private class AniListData
        {
            public MediaData Media { get; set; }
        }

        private class MediaData
        {
            public TitleData Title { get; set; }
            public string Description { get; set; }
            public DateData StartDate { get; set; }
            public int? AverageScore { get; set; }
            public AiringEpisodeData NextAiringEpisode { get; set; }
        }

        private class TitleData
        {
            public string Romaji { get; set; }
        }

        private class DateData
        {
            public int Year { get; set; }
        }

        private class AiringEpisodeData
        {
            public int AiringAt { get; set; }
        }
    }
}
