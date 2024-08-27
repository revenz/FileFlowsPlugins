using FileFlows.Plugin.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace FileFlows.Emby.MediaManagement;

public class EmbyUpdater: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process; 
    /// <inheritdoc />
    public override string Icon => "svg:emby";
    /// <inheritdoc />
    public override bool NoEditorOnAdd => true;

    [Text(1)]
    public string ServerUrl { get; set; } = string.Empty;

    [Text(2)]
    public string AccessToken { get; set; } = string.Empty;

    [KeyValue(3, null)] public List<KeyValuePair<string, string>> Mapping { get; set; } = new();

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public override int Execute(NodeParameters args)
    {
        var settings = args.GetPluginSettings<PluginSettings>();
        var serverUrl = ServerUrl?.EmptyAsNull() ?? settings?.ServerUrl;
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            args.FailureReason = "No Emby server configured";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        var accessToken = AccessToken?.EmptyAsNull() ?? settings?.AccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            args.FailureReason = "No Emby access token configured";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        var mapping = (string.IsNullOrWhiteSpace(ServerUrl) ? settings?.Mapping : Mapping) 
                      ?? new List<KeyValuePair<string, string>>();

        // get the path
        string path = args.WorkingFile;
        path = args.UnMapPath(path);

        foreach (var map in mapping)
        {
            if (string.IsNullOrEmpty(map.Key))
                continue;
            path = path.Replace(map.Key, map.Value ?? string.Empty);
        }

        if (args.IsDirectory == false)
        {
            bool windows = path.StartsWith("\\") || Regex.IsMatch(path, @"^[a-zA-Z]:\\");
            string pathSeparator = windows ? "\\" : "/";
            path = path.Substring(0, path.LastIndexOf(pathSeparator, StringComparison.Ordinal));
        }


        string url = serverUrl;
        if (url.EndsWith('/') == false)
            url += "/";
        url += "Library/Media/Updated";
        
        args.Logger?.ILog("Url to call: " + url);
        args.Logger?.ILog("Path to update: " + path);

        var body = System.Text.Json.JsonSerializer.Serialize(new {
            Updates = new object [] { new { Path = path } }
        });

        using var httpClient = new HttpClient();

        var updateResponse = GetWebRequest(httpClient, url, accessToken, body);
        if (updateResponse.success == false)
        {
            if(string.IsNullOrWhiteSpace(updateResponse.body) == false)
                args.Logger?.WLog("Failed to update Emby:" + updateResponse.body);
            return 2;
        }
        args.Logger?.ILog("Body response: " + (updateResponse.body ?? string.Empty));
        return 1;
    }

    /// <summary>
    /// The method used to send the request
    /// </summary>
    private Func<HttpClient, string, string, string, (bool success, string body)>? _GetWebRequest;
    
    /// <summary>
    /// Gets the method used to send a request
    /// </summary>
    internal Func<HttpClient, string, string, string, (bool success, string body)> GetWebRequest
    {
        get
        {
            if(_GetWebRequest == null)
            {
                _GetWebRequest = (HttpClient client, string url, string accessToken, string json) =>
                {
                    try
                    {
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Add("X-MediaBrowser-Token", accessToken);
                        var response = client.PostAsync(url, content).Result;
                        var respnoseBody = response.Content.ReadAsStringAsync().Result;
                        if(response.IsSuccessStatusCode)
                            return (response.IsSuccessStatusCode, respnoseBody);
                        return (response.IsSuccessStatusCode, respnoseBody?.EmptyAsNull() ?? response.ReasonPhrase ?? string.Empty);
                    }
                    catch(Exception ex)
                    {
                        return (false, ex.Message);
                    }
                };
            }
            return _GetWebRequest;
        }
        #if(DEBUG)
        set
        {
            _GetWebRequest = value;
        }
        #endif
    }
}
