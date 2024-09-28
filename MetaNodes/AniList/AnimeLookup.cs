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
    public class TVShowLookup : Node
    {
        private const string AniListGraphQLUrl = "https://graphql.anilist.co";

        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string HelpUrl => "https://fileflows.com/docs/plugins/meta-nodes/tv-show-lookup";
        public override string Icon => "fas fa-tv";

        private Dictionary<string, object> _Variables;

        public override Dictionary<string, object> Variables => _Variables;

        public TVShowLookup()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "tvshow.Title", "Naruto" },
                { "tvshow.Year", 2002 }
            };
        }

        [Boolean(1)]
        public bool UseFolderName { get; set; }

        public override async Task<int> Execute(NodeParameters args)
        {
            (string lookupName, string year) = GetLookupName(args.LibraryFileName, UseFolderName);

            // Send query to AniList
            var showInfo = await FetchShowInfoFromAniList(lookupName);

            if (showInfo != null)
            {
                _Variables["tvshow.Title"] = showInfo.Title;
                _Variables["tvshow.Year"] = showInfo.Year;
                _Variables["tvshow.Description"] = showInfo.Description;
                _Variables["tvshow.Score"] = showInfo.Score;

                args.Logger?.ILog($"Found TV Show: {showInfo.Title} ({showInfo.Year})");
                return 1; // success output
            }
            else
            {
                args.Logger?.WLog($"TV Show '{lookupName}' not found.");
                return 2; // failure output
            }
        }

        private async Task<ShowInfo> FetchShowInfoFromAniList(string showName)
        {
            var query = @"
            query ($search: String) {
              Media(search: $search, type: ANIME) {
                title {
                  romaji
                }
                description
                startDate {
                  year
                }
                averageScore
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
                            return new ShowInfo
                            {
                                Title = media.Title.Romaji,
                                Description = media.Description,
                                Year = media.StartDate.Year,
                                Score = media.AverageScore
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

        private (string, string) GetLookupName(string libraryFileName, bool useFolderName)
        {
            string lookupName;
            string year = null;

            if (useFolderName)
            {
                // Get folder name if using folder name
                var folderName = Path.GetDirectoryName(libraryFileName);
                lookupName = ExtractShowNameFromPath(folderName);
                year = ExtractYearFromPath(folderName);
            }
            else
            {
                // Extract from file name
                var fileName = Path.GetFileNameWithoutExtension(libraryFileName);
                lookupName = ExtractShowNameFromPath(fileName);
                year = ExtractYearFromPath(fileName);
            }

            return (lookupName, year);
        }

        private string ExtractShowNameFromPath(string path)
        {
            // Regex pattern to extract show name from file or folder
            var showNamePattern = @"^(?<name>[\w\s\.\-\(\)]+?)(\s?[\(\.\-\_]\d{4}[\)\.\-\_])?";  // Matches "ShowName" or "ShowName (2004)"

            var match = Regex.Match(path, showNamePattern);
            if (match.Success)
            {
                return match.Groups["name"].Value.Trim(new[] { '.', ' ', '-', '_', '(', ')' });
            }

            // If no match, fallback to entire path as name (unlikely)
            return path;
        }

        private string ExtractYearFromPath(string path)
        {
            // Regex to extract year in formats like "(2004)", ".2004.", "-2004-", " 2004 "
            var yearPattern = @"(?:[\(\.\-\_\s])(?<year>(19|20)\d{2})(?:[\)\.\-\_\s])";

            var match = Regex.Match(path, yearPattern);
            if (match.Success)
            {
                return match.Groups["year"].Value;
            }

            return null; // If no year found
        }

        private class ShowInfo
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int Year { get; set; }
            public int? Score { get; set; }
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
        }

        private class TitleData
        {
            public string Romaji { get; set; }
        }

        private class DateData
        {
            public int Year { get; set; }
        }
    }
}
