using System.Text.Json;

namespace FileFlows.Apprise;

/// <summary>
/// Interface for Apprise
/// </summary>
public interface IAppriseApi
{
    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="logger">the logger to use</param>
    /// <param name="type">the type of message to send</param>
    /// <param name="tags">the tags</param>
    /// <param name="message">the message</param>
    /// <returns>the result</returns>
    bool Send(ILogger logger, string? type, string[]? tags, string message);
}

/// <summary>
/// Apprise URL instance
/// </summary>
/// <param name="url">the URL to send to</param>
public class AppriseApi(string url) : IAppriseApi
{
    /// <inheritdoc />
    public bool Send(ILogger logger, string? type, string[]? tags, string message)
    {

        object data = new
        {
            body = message,
            tag = tags?.Any() != true ? "all" : string.Join(";", tags),
            type = type?.EmptyAsNull() ?? "info"
        };

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpClient = new HttpClient();
        var response = httpClient.PostAsync(url, content).Result;
        if (response.IsSuccessStatusCode)
            return true;

        string error = response.Content.ReadAsStringAsync().Result;
        logger?.WLog("Error from Apprise: " + error);
        return false;
    }
} 