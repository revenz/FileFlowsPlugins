using FileFlows.Plex.Models;

namespace FileFlows.Plex.MediaManagement;

public class PlexUpdater: PlexNode
{
    protected override int ExecuteActual(NodeParameters args, PlexDirectory directory, string url, string mappedPath, string accessToken)
    {
        args.Logger?.ILog("Executing Actual in Plex Updater");
        url += $"library/sections/{directory.Key}/refresh?path={Uri.EscapeDataString(mappedPath)}&X-Plex-Token=" + accessToken;

        using var httpClient = new HttpClient();
        var updateResponse = GetWebRequest(httpClient, url);
        if (updateResponse.success == false)
        {
            if(string.IsNullOrWhiteSpace(updateResponse.body) == false)
                args.Logger?.WLog("Failed to update Plex:" + updateResponse.body);
            return 2;
        }
        return 1;
    }
}
