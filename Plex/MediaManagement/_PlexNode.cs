using FileFlows.Plex.Models;

namespace FileFlows.Plex.MediaManagement;

/// <summary>
/// Plex flow element
/// </summary>
public abstract class PlexNode:Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override bool NoEditorOnAdd => true;
    /// <inheritdoc />
    public override string Icon => "svg:plex";

    [Text(1)] public string ServerUrl { get; set; } = string.Empty;

    [Text(2)] public string AccessToken { get; set; } = string.Empty;

    [KeyValue(3, null)]
    public List<KeyValuePair<string, string>>? Mapping { get; set; }

    public override int Execute(NodeParameters args)
    {
        var settings = args.GetPluginSettings<PluginSettings>();
        string serverUrl = ServerUrl?.EmptyAsNull() ?? settings.ServerUrl;
        string accessToken = AccessToken?.EmptyAsNull() ?? settings.AccessToken;
        var mapping = (string.IsNullOrWhiteSpace(ServerUrl) ? settings.Mapping : Mapping) ?? new List<KeyValuePair<string, string>>();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            args.Logger?.WLog("No access token set");
            return 2;
        }
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            args.Logger?.WLog("No server URL set");
            return 2;
        }

        // get the path
        string path = args.WorkingFile;
        args.Logger?.ILog("Working File: " + path);
        path = args.UnMapPath(path);
        args.Logger?.ILog("Working File (Unmapped): " + path);
        if (args.IsDirectory == false)
        {
            // just use /
            path = path.Replace("\\", "/");
            path = path[..path.LastIndexOf("/", StringComparison.Ordinal)];
        }

        if (serverUrl.EndsWith("/") == false)
            serverUrl += "/";
        string url = serverUrl;
        url += "library/sections";

        using var httpClient = new HttpClient();

        var sectionsResponse = GetWebRequest(httpClient, url + "?X-Plex-Token=" + accessToken);
        if (sectionsResponse.success == false)
        {
            args.Logger?.WLog("Failed to retrieve sections" + (string.IsNullOrWhiteSpace(sectionsResponse.body) ? "" : ": " + sectionsResponse.body));
            return 2;
        }

        PlexSections sections;
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            sections = System.Text.Json.JsonSerializer.Deserialize<PlexSections>(sectionsResponse.body, options)!;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed deserializing sections json: " + ex.Message);
            return 2;
        }
        args.Logger?.ILog("Path before plex mapping: " + path);
        foreach (var map in mapping)
        {
            if (string.IsNullOrEmpty(map.Key))
                continue;
            path = path.Replace(map.Key, map.Value ?? string.Empty);
        }
        args.Logger?.ILog("Path after plex mapping: " + path);

        string pathLower = path.Replace("\\", "/").ToLowerInvariant();
        if (pathLower.EndsWith("/"))
            pathLower = pathLower[..^1];
        args.Logger?.ILog("Testing Plex Path: " + pathLower);
        var section = sections?.MediaContainer?.Directory?.Where(x => {
            if (x.Location?.Any() != true)
                return false;
            foreach (var loc in x.Location)
            {
                if (loc.Path == null)
                    continue;
                args.Logger?.ILog("Plex section path: " + loc.Path);
                if (pathLower.StartsWith(loc.Path.Replace("\\", "/").ToLowerInvariant()))
                    return true;
            }
            return false;
        }).FirstOrDefault();
        if (section == null)
        {
            args.Logger?.WLog("Failed to find Plex section for path: " + path);
            return 2;
        }
        args.Logger?.ILog("Found section: " + (section.Key ?? "no-key"));
        return ExecuteActual(args, section, serverUrl, path, accessToken);
    }

    protected abstract int ExecuteActual(NodeParameters args, PlexDirectory directory, string url, string mappedPath, string accessToken);


    private Func<HttpClient, string, (bool success, string body)>? _GetWebRequest;
    internal Func<HttpClient, string, (bool success, string body)> GetWebRequest
    {
        get
        {
            if (_GetWebRequest == null)
            {
                _GetWebRequest = (HttpClient client, string url) =>
                {
                    try
                    {
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        var response = client.GetAsync(url).Result;
                        string body = response.Content.ReadAsStringAsync().Result;
                        return (response.IsSuccessStatusCode, body);
                    }
                    catch (Exception ex)
                    {
                        return (false, ex.Message);
                    }
                };
            }
            return _GetWebRequest;
        }
        #if(DEBUG)
        set => _GetWebRequest = value;
        #endif
    }
    private Func<HttpClient, string, (bool success, string body)>? _PutWebRequest;
    internal Func<HttpClient, string, (bool success, string body)> PutWebRequest
    {
        get
        {
            if (_PutWebRequest == null)
            {
                _PutWebRequest = (HttpClient client, string url) =>
                {
                    try
                    {
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        var response = client.PutAsync(url, null).Result;
                        string body = response.Content.ReadAsStringAsync().Result;
                        return (response.IsSuccessStatusCode, body);
                    }
                    catch (Exception ex)
                    {
                        return (false, ex.Message);
                    }
                };
            }
            return _PutWebRequest;
        }
#if(DEBUG)
        set => _PutWebRequest = value;
#endif
    }
}
