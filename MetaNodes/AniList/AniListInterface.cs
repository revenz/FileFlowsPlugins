using System.Text;
using System.Text.Json;
using FileFlows.Plugin;

namespace MetaNodes.AniList;

/// <summary>
/// Helper class for interacting with the AniList API.
/// </summary>
/// <param name="_args">The node parameters.</param>
public class AniListInterface(NodeParameters _args)
{
    private const string AniListGraphQLUrl = "https://graphql.anilist.co";

    private static HttpClient _HttpClient = new();

    /// <summary>
    /// Fetches show information from AniList based on the provided show name.
    /// </summary>
    /// <param name="showName">The name of the anime show to look up.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AniListShowInfo"/> object.</returns>
    public async Task<AniListShowInfo?> FetchShowInfo(string showName)
    {
        var query = @"
        query ($search: String) {
          Media(search: $search, type: ANIME) {
            title {
              romaji,
              english,
              native
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

        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            var response = await _HttpClient.PostAsync(AniListGraphQLUrl, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode == false)
            {
                _args.Logger?.ELog($"Error fetching data from AniList: {jsonResponse}");
                return null;
            }

            var media = UnwrapResponse<AniListShowInfoResponse>(jsonResponse);
        
            return new AniListShowInfo
            {
                Title = media.Title.English?.EmptyAsNull() ??
                        media.Title.Native?.EmptyAsNull() ?? media.Title.Romaji,
                TitleRomaji = media.Title.Romaji,
                TitleEnglish = media.Title.English,
                TitleNative = media.Title.Native,
                Description = media.Description,
                Year = media.StartDate.Year,
                Score = media.AverageScore
            };
        }
        catch (Exception ex)
        {
            _args.Logger?.ELog($"Exception occurred: {ex.Message}");
        }

        return null;
    }
/// <summary>
/// Fetches episode information from AniList based on the provided show name.
/// </summary>
/// <param name="showName">The name of the anime show.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AniListEpisodeInfo"/> objects.</returns>
public async Task<List<AniListEpisodeInfo>> FetchEpisode(string showName)
{
    var queryAiringSchedule = @"
    query ($search: String) {
      Media(search: $search, type: ANIME) {
        airingSchedule {
          edges {
            node {
              episode
              airingAt
            }
          }
        }
        id
      }
    }";
    
    var variablesAiringSchedule = new { search = showName };
    var jsonRequestAiringSchedule = JsonSerializer.Serialize(new { query = queryAiringSchedule, variables = variablesAiringSchedule });

    var episodeDetails = new List<AniListEpisodeInfo>();

    try
    {
        // Send the request to AniList for the airing schedule
        var content = new StringContent(jsonRequestAiringSchedule, Encoding.UTF8, "application/json");
        var responseAiringSchedule = await _HttpClient.PostAsync(AniListGraphQLUrl, content);
        var jsonResponseAiringSchedule = await responseAiringSchedule.Content.ReadAsStringAsync();

        if (responseAiringSchedule.IsSuccessStatusCode == false)
        {
            _args.Logger?.ELog($"Error fetching data from AniList: {jsonResponseAiringSchedule}");
            return new List<AniListEpisodeInfo>();
        }

        // Use UnwrapResponse to directly extract AniListAiringSchedule from the JSON response
        var airSchedule = UnwrapResponse<AniListAirScheduleResponse>(jsonResponseAiringSchedule);
        

        // Now we can fetch the Media Id
        var mediaId = airSchedule.Id;

        if (mediaId == 0)
        {
            _args.Logger?.ELog("No media ID found for the show.");
            return new List<AniListEpisodeInfo>();
        }
        
        if (airSchedule?.AiringSchedule?.Edges == null)
        {
            _args.Logger?.ELog("Failed to deserialize the airing schedule data.");
            return new List<AniListEpisodeInfo>();
        }

        // Query episode details based on the media ID
        var queryEpisodeDetails = @"
             query ($id: Int) {
               Media(id: $id) {
                 episodes {
                   episode
                   title {
                     romaji
                     english
                     native
                   }
                   description
                 }
               }
             }";

        var variablesEpisodeDetails = new { id = mediaId };
        var jsonRequestEpisodeDetails = JsonSerializer.Serialize(new { query = queryEpisodeDetails, variables = variablesEpisodeDetails });

        var contentEpisodeDetails = new StringContent(jsonRequestEpisodeDetails, Encoding.UTF8, "application/json");
        var responseEpisodeDetails = await _HttpClient.PostAsync(AniListGraphQLUrl, contentEpisodeDetails);
        var jsonResponseEpisodeDetails = await responseEpisodeDetails.Content.ReadAsStringAsync();

        if (responseEpisodeDetails.IsSuccessStatusCode == false)
        {
            _args.Logger?.ELog($"Error fetching episode details from AniList: {jsonResponseEpisodeDetails}");
            return new List<AniListEpisodeInfo>();
        }

        // Deserialize episode details
        // var dataEpisodeDetails = JsonSerializer.Deserialize<AniListEpisodeResponse>(
        //     jsonResponseEpisodeDetails, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // var episodes = dataEpisodeDetails?.Data?.Media?.Episodes;
        //
        // if (episodes != null)
        // {
        //     foreach (var episode in episodes)
        //     {
        //         episodeDetails.Add(new AniListEpisodeInfo
        //         {
        //             EpisodeNumber = episode.Episode,
        //             Title = episode.Title?.Romaji ?? episode.Title?.English ?? episode.Title?.Native,
        //             Description = episode.Description
        //         });
        //     }
        // }
    }
    catch (Exception ex)
    {
        _args.Logger?.ELog($"Exception occurred: {ex.Message}");
    }

    return episodeDetails;
}



    /// <summary>
    /// Unwraps a nested JSON response from AniList API, automatically finding the object under data.Media.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the unwrapped JSON into.</typeparam>
    /// <param name="json">The raw JSON string.</param>
    /// <returns>The unwrapped object of type <typeparamref name="T"/>.</returns>
    public static T UnwrapResponse<T>(string json)
    {
        using var doc = JsonDocument.Parse(json);

        // Navigate to data.Media
        if (!doc.RootElement.TryGetProperty("data", out var dataElement) ||
            !dataElement.TryGetProperty("Media", out var mediaElement))
        {
            throw new InvalidOperationException("JSON does not contain 'data.Media' structure.");
        }

        return mediaElement.Deserialize<T>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
