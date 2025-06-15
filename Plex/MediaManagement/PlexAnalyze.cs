using System.Diagnostics.CodeAnalysis;
using FileFlows.Plex.Models;

namespace FileFlows.Plex.MediaManagement;

public class PlexAnalyze : PlexNode
{

    protected override int ExecuteActual(NodeParameters args, PlexDirectory directory, string baseUrl, string mappedPath, string accessToken)
    {
        string filename = new FileInfo(args.WorkingFile).Name;
        string mappedFile = mappedPath + (mappedPath.IndexOf('/') >= 0 ? "/" : @"\") + filename;

        string itemId = GetItemId(args, baseUrl, $"library/sections/{directory.Key}/all?includeCollections=1&includeAdvanced=1", accessToken, mappedFile);
        
        if (string.IsNullOrEmpty(itemId))
            return 2;
        args.Logger?.ILog("Found Plex Media: " + itemId);

        string url = $"{baseUrl}library/metadata/{itemId}/analyze?X-Plex-Token={accessToken}";
        var analyzeReponse = PutWebRequest(url);

        if (analyzeReponse.success == false)
        {
            if (string.IsNullOrWhiteSpace(analyzeReponse.body) == false)
                args.Logger?.WLog("Failed to analyze item in Plex:" + analyzeReponse.body);
            return 2;
        }
        args.Logger?.ILog("Successfully sent analyze request to Plex.");
        return 1;
    }

    internal string GetItemId(NodeParameters args, string baseUrl, string urlPath, string token, string file)
    {
        var media = GetPlexMedia(args, baseUrl, urlPath, token);
        if(media?.Any() != true)
        {
            args.Logger?.ILog("No media found in Plex");
            return string.Empty;
        }

        var item = media.Where(x => x.Part?.Any(y => 
            string.Equals(y.File?.Replace("\\", "/"), file.Replace("\\", "/"), StringComparison.InvariantCultureIgnoreCase)) == true)?.FirstOrDefault();
        if(item == null)
        {
            args.Logger?.ILog($"No item matching '{file}' found in Plex.");
            return string.Empty;
        }    
        return item?.RatingKey ?? string.Empty;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal PlexMedia[] GetPlexMedia(NodeParameters args, string baseUrl, string urlPath, string token, int depth = 0)
    {
        if (depth > 10)
            return [];
        if (urlPath.StartsWith('/') && baseUrl.EndsWith('/'))
            urlPath = urlPath[1..];

        string fullUrl = baseUrl + urlPath;
        fullUrl += (fullUrl.IndexOf('?', StringComparison.Ordinal) > 0 ? "&" : "?") + "X-Plex-Token=";
        args.Logger?.ILog("Requesting URL: " + fullUrl);
        fullUrl += token;
        var updateResponse = GetWebRequest(fullUrl);
        if (updateResponse.success == false)
        {
            if (string.IsNullOrWhiteSpace(updateResponse.body) == false)
                args.Logger?.WLog("Failed to get files from Plex:" + updateResponse.body);
            return [];
        }
    

        List<PlexMetadata> metadata;
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            metadata = System.Text.Json.JsonSerializer.Deserialize<PlexSections>(updateResponse.body, options)
                           ?.MediaContainer?.Metadata?.ToList()
                       ?? new();
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed deserializing sections json: " + ex.Message);
            return [];
        }

        var list = metadata ?? new();
        var results = new List<PlexMedia>();

        foreach(var item in list)
        {
            if(item?.Media?.Any() == true)
            {
                foreach (var media in item.Media)
                    media.RatingKey = item.RatingKey;

                results.AddRange(item.Media);
            }
            else if(string.IsNullOrEmpty(item?.Key) == false && depth < 10)
            {
                var children = GetPlexMedia(args, baseUrl, item.Key, token, depth + 1);
                if (children?.Any() == true)
                    results.AddRange(children);
            }
        }
        return results.ToArray();
    }
}
