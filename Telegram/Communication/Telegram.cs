namespace FileFlows.Telegram.Communication;

/// <summary>
/// A Telegram flow element that sends a message
/// </summary>
public class Telegram: Node
{
    /// <summary>
    /// Gets the number of inputs to this flow element
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs to this flow element
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Communication;
    /// <summary>
    /// Gets the icon for this flow element
    /// </summary>
    public override string Icon => "fab fa-telegram-plane";
    /// <summary>
    /// Gets if this can be used in a failure flow
    /// </summary>
    public override bool FailureNode => true;

    /// <inheritdoc />
    public override string CustomColor => "#0088CC";
    
    /// <summary>
    /// Gets the Help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/telegram";

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    [Required]
    [Template(1, nameof(MessageTemplates))]
    public string Message { get; set; } = string.Empty;

    private static List<ListOption>? _MessageTemplates;
    public static List<ListOption> MessageTemplates
    {
        get
        {
            if (_MessageTemplates == null)
            {
                _MessageTemplates = new List<ListOption>
                {
                    new () { Label = "Basic", Value = @"File: {{ file.Orig.FullName }}
Size: {{ file.Size }}" },
                    new () { Label = "File Size Changes", Value = @"
{{ difference = file.Size - file.Orig.Size }}
{{ percent = (difference / file.Orig.Size) * 100 | math.round 2 }}

Input File: {{ file.Orig.FullName }}
Output File: {{ file.FullName }}
Original Size: {{ file.Orig.Size | file_size }}
Final Size: {{ file.Size | file_size }}

{{- if difference > 0 }}
File grew in size: {{ difference | math.abs | file_size }}
{{ else }}
File shrunk in size by: {{ difference | file_size }} / {{ percent }}%
{{ end }}"}
                };
            }
            return _MessageTemplates;
        }
    }

    /// <summary>
    /// Sends a telegram message
    /// </summary>
    /// <param name="botToken">the bot token</param>
    /// <param name="chatId">the chat id</param>
    /// <param name="message">the message to send</param>
    /// <returns>true if successful, otherwise false</returns>
    internal static bool SendMessage(string botToken, string chatId, string message)
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = $"https://api.telegram.org/bot{botToken}/sendMessage";
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("chat_id", chatId),
                new KeyValuePair<string, string>("text", message)
            });

            var response = client.PostAsync(apiUrl, content).Result;
            return response.IsSuccessStatusCode;
        }
    }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        try
        {
            var settings = args.GetPluginSettings<PluginSettings>();

            if (settings == null)
            {
                args.Logger?.ILog("Failed to load plugin settings");
                return 2;
            }

            if (string.IsNullOrWhiteSpace(settings?.BotToken))
            {
                args.Logger?.WLog("No Bot Token set");
                return 2;
            }

            if (string.IsNullOrWhiteSpace(settings?.ChatId))
            {
                args.Logger?.WLog("No Chat ID set");
                return 2;
            }

            var message = args.RenderTemplate!(Message);

            var result = SendMessage(settings.BotToken, settings.ChatId, message);

            return result ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Error sending message: " + ex.Message);
            return 2;
        } 
    }
}
