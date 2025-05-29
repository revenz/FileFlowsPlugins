using System.Text.Json;
using FileFlows.Plugin.Attributes;
using System.Text.RegularExpressions;
using FileFlows.Common;

namespace FileFlows.Emby.MediaManagement;

/// <summary>
/// Represents a node that updates the Emby media server about changes to media files or directories.
/// </summary>
public class EmbyUpdater : Node
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

    /// Gets or sets the URL of the Emby server used for updating media metadata.
    /// This property is required for connecting to the Emby server and performing operations.
    /// The URL should include the protocol (e.g., http or https) and optionally
    /// the port. It is used throughout the process to build API requests.
    [Text(1)]
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token used for authenticating requests to the Emby server.
    /// </summary>
    [Text(2)]
    public string AccessToken { get; set; } = string.Empty;

    internal HttpClient? Http { get; set; } // ← inject for testing

    private HttpClient CreateClient()
    {
        return Http ?? new HttpClient();
    }

    /// <summary>
    /// Gets or sets the mapping structure where each key-value pair represents a path transformation.
    /// Keys represent source paths, and values represent their corresponding destination paths.
    /// This mapping is utilized to transform file or folder paths according to configured mappings.
    /// </summary>
    [KeyValue(3, null)]
    public List<KeyValuePair<string, string>> Mapping { get; set; } = new();

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var settings = args.GetPluginSettings<PluginSettings>();
        var serverUrl = GetServerUrl(args, settings);
        if (serverUrl == null)
            return -1;

        var accessToken = GetAccessToken(args, settings);
        if (accessToken == null)
            return -1;
        
        Http ??= CreateClient();
        Http!.DefaultRequestHeaders.Add("X-MediaBrowser-Token", accessToken);

        var path = GetMappedPath(args, settings);

        if (args.IsDirectory == false)
            path = TrimToDirectory(path);

        string? itemId = FindItemIdByPath(serverUrl, accessToken, path, args.Logger);
        if (itemId == null)
        {
            args.Logger?.WLog("Failed to find item ID in Emby for path: " + path);
            return 2;
        }

        bool success = RefreshEmbyItem(serverUrl, accessToken, itemId, args.Logger, out var responseBody);
        if (success == false)
        {
            if (!string.IsNullOrWhiteSpace(responseBody))
                args.Logger?.WLog("Failed to update Emby: " + responseBody);
            return 2;
        }

        args.Logger?.ILog("Successfully updated Emby. Response: " + (responseBody ?? string.Empty));
        return 1;
    }

    /// <summary>
    /// Retrieves the server URL for the Emby Server as configured in the Node or plugin settings.
    /// </summary>
    /// <param name="args">The node parameters containing execution context and logging.</param>
    /// <param name="settings">The plugin settings containing fallback configurations if not set directly in the Node.</param>
    /// <returns>The server URL as a string if configured, null otherwise.</returns>
    private string? GetServerUrl(NodeParameters args, PluginSettings? settings)
    {
        var serverUrl = ServerUrl?.EmptyAsNull() ?? settings?.ServerUrl;
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            args.FailureReason = "No Emby server configured";
            args.Logger?.ELog(args.FailureReason);
            return null;
        }

        if (serverUrl.EndsWith('/') == false)
            serverUrl += "/";
        return serverUrl;
    }

    /// <summary>
    /// Retrieves the access token for authentication with the Emby server.
    /// </summary>
    /// <param name="args">The NodeParameters context, containing execution state and logging functionality.</param>
    /// <param name="settings">The plugin settings containing configuration values.</param>
    /// <returns>The access token as a string, or null if not configured or invalid.</returns>
    private string? GetAccessToken(NodeParameters args, PluginSettings? settings)
    {
        var accessToken = AccessToken?.EmptyAsNull() ?? settings?.AccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            args.FailureReason = "No Emby access token configured";
            args.Logger?.ELog(args.FailureReason);
            return null;
        }

        return accessToken;
    }

    /// <summary>
    /// Retrieves the mapped path based on the provided node parameters and plugin settings.
    /// </summary>
    /// <param name="args">The node parameters containing relevant execution context and file information.</param>
    /// <param name="settings">The plugin settings containing configuration values such as mappings.</param>
    /// <returns>The resolved mapped path as a string.</returns>
    private string GetMappedPath(NodeParameters args, PluginSettings? settings)
    {
        var mapping = (string.IsNullOrWhiteSpace(ServerUrl) ? settings?.Mapping : Mapping)
                      ?? new List<KeyValuePair<string, string>>();

        string path = args.WorkingFile;
        path = args.UnMapPath(path);

        foreach (var map in mapping)
        {
            if (string.IsNullOrEmpty(map.Key))
                continue;
            path = path.Replace(map.Key, map.Value ?? string.Empty);
        }

        return path;
    }

    /// <summary>
    /// Trims the given path to its parent directory by removing the last segment of the path.
    /// </summary>
    /// <param name="path">The file path to be trimmed to its directory.</param>
    /// <returns>The trimmed path, representing the parent directory of the input path.</returns>
    private string TrimToDirectory(string path)
    {
        bool windows = path.StartsWith("\\") || Regex.IsMatch(path, @"^[a-zA-Z]:\\");
        string pathSeparator = windows ? "\\" : "/";
        int index = path.LastIndexOf(pathSeparator, StringComparison.Ordinal);
        if (index > 0)
            path = path.Substring(0, index);
        return path;
    }

    /// <summary>
    /// Finds the Emby item ID for a given path on the server.
    /// </summary>
    /// <param name="serverUrl">The base URL of the Emby server.</param>
    /// <param name="accessToken">The access token used to authorize API requests.</param>
    /// <param name="path">The file or directory path to find in the Emby server.</param>
    /// <param name="logger">An optional logger for logging errors or information during the operation.</param>
    /// <returns>
    /// Returns the item ID as a string if found, or null if the item is not found on the server.
    /// </returns>
    private string? FindItemIdByPath(string serverUrl, string accessToken, string path, ILogger? logger)
    {
        try
        {
            var client = Http!;
            // First, try to get items under the parent folder
            // This API call tries to get children of the folder, filtered by name
            string url = $"{serverUrl}Items?Recursive=true&Fields=Path&Path={Uri.EscapeDataString(path)}";

            var response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode)
                return null;

            var json = response.Content.ReadAsStringAsync().Result;

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("Path", out var itemPath) && 
                        string.Equals(itemPath.GetString()?.Replace("\\", "/"), path.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase))
                    {
                        if (item.TryGetProperty("Id", out var id))
                            return id.GetString();
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            logger?.ELog("Failed to find Emby item by path: " + ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Sends a request to refresh an Emby item by its ID.
    /// </summary>
    /// <param name="serverUrl">The URL of the Emby server.</param>
    /// <param name="accessToken">The access token required for authentication with the Emby server.</param>
    /// <param name="itemId">The ID of the item to be refreshed on the Emby server.</param>
    /// <param name="logger">The logger instance for logging messages.</param>
    /// <param name="responseBody">The response body returned by the Emby server.</param>
    /// <returns>
    /// True if the refresh operation is successful; otherwise, false.
    /// </returns>
    private bool RefreshEmbyItem(string serverUrl, string accessToken, string itemId, ILogger? logger,
        out string responseBody)
    {
        string url = $"{serverUrl}Items/{itemId}/Refresh?MetadataRefreshMode=FullRefresh";
        logger?.ILog("Refreshing item at URL: " + url);
        try
        {
            var response = Http!.PostAsync(url, null).Result;
            responseBody = response.Content.ReadAsStringAsync().Result;
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            responseBody = ex.Message;
            return false;
        }
    }

}
