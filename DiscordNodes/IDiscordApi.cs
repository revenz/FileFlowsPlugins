using System.Text.Json;

namespace FileFlows.DiscordNodes;

/// <summary>
/// Interface for Discord API
/// </summary>
public interface IDiscordApi
{
    /// <summary>
    /// Sends a basic message
    /// </summary>
    /// <param name="logger">the logger</param>
    /// <param name="message">the message</param>
    /// <returns>true if successful, otherwise false</returns>
    bool SendBasic(ILogger logger, string message);

    /// <summary>
    /// Sends an advanced message
    /// </summary>
    /// <param name="logger">the logger</param>
    /// <param name="message">the message</param>
    /// <param name="title">the title</param>
    /// <param name="type">the type</param>
    /// <returns>true if successful, otherwise false</returns>
    bool SendAdvanced(ILogger logger, string message, string title, string type);
}

/// <summary>
/// Implementation of the Discord API
/// </summary>
/// <param name="webhookId">the ID of the webhook</param>
/// <param name="webhookToken">the token of the webhook</param>
public class DiscordApi(string webhookId, string webhookToken) : IDiscordApi
{
    
    const int colorInfo = 0x1F61E6;
    const int colorSuccess= 0x80E61F;
    const int colorError = 0xE7421F;
    const int colorFailure = 0xC61FE6;
    const int colorWarning = 0xE6C71F;
    
    /// <inheritdoc />
    public bool SendBasic(ILogger logger, string message)
        => Send(logger, new
        {
            username = "FileFlows",
            content = message,
            avatar_url = "https://fileflows.com/icon.png",
        });

    /// <inheritdoc />
    public bool SendAdvanced(ILogger logger, string message, string title, string type)
        => Send(logger, new
        {
            username = "FileFlows",
            avatar_url = "https://fileflows.com/icon.png",
            embeds = new[]
            {
                new
                {
                    description = message,
                    title,
                    color = type switch
                    {
                        "Success" => colorSuccess,
                        "Warning" => colorWarning,
                        "Error" => colorError,
                        "Failure" => colorFailure,
                        _ => colorInfo,
                    }
                }
            }
        });
    
    /// <summary>
    /// Sends the webhook
    /// </summary>
    /// <param name="logger">the logger</param>
    /// <param name="body">the body</param>
    /// <returns>the result</returns>
    private bool Send(ILogger logger, object body)
    {

    string url = $"https://discordapp.com/api/webhooks/{webhookId}/{webhookToken}";
#pragma warning disable IL2026
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
#pragma warning restore IL2026
        using var httpClient = new HttpClient();
        var response = httpClient.PostAsync(url, content).Result;
        if (response.IsSuccessStatusCode)
            return true;

        string error = response.Content.ReadAsStringAsync().Result;
        logger?.WLog("Error from discord: " + error);
        return false;
    }
}