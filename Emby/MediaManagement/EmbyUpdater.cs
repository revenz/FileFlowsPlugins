using FileFlows.Plugin.Attributes;
using System.Text.RegularExpressions;

namespace FileFlows.Emby.MediaManagement;

public class EmbyUpdater: Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-paper-plane";

    public override bool NoEditorOnAdd => true;

    [Text(1)]
    public string ServerUrl { get; set; }

    [Text(2)]
    public string AccessToken { get; set; }

    [KeyValue(3)]
    public List<KeyValuePair<string, string>> Mapping { get; set; }

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
            path = path.Substring(0, path.LastIndexOf(pathSeparator));
        }


        string url = serverUrl;
        if (url.EndsWith("/") == false)
            url += "/";
        url += "Library/Media/Updated";

        string body = System.Text.Json.JsonSerializer.Serialize(new {
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
        return 1;
    }

    private Func<HttpClient, string, string, string, (bool success, string body)> _GetWebRequest;
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
                        string respnoseBody = response.Content.ReadAsStringAsync().Result;
                        if(response.IsSuccessStatusCode)
                            return (response.IsSuccessStatusCode, respnoseBody);
                        return (response.IsSuccessStatusCode, respnoseBody?.EmptyAsNull() ?? response.ReasonPhrase);
                    }
                    catch(Exception ex)
                    {
                        return (false, ex.Message);
                    }
                };
            }
            return _GetWebRequest;
        }
    }
}
