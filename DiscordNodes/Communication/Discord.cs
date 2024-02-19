using System.ComponentModel;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FileFlows.DiscordNodes.Communication;

public class Discord: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Communication; 
    /// <inheritdoc />
    public override string Icon => "fab fa-discord";
    /// <inheritdoc />
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/discord/discord";
    /// <inheritdoc />
    public override string CustomColor => "#5865F2";

    /// <summary>
    /// Gets or sets the title
    /// </summary>
    [TextVariable(1)]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the message type
    /// </summary>
    [DefaultValue("standard")]
    [Select(nameof(MessageTypeOptions), 2)]
    public string MessageType { get; set; }

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    [Required]
    [Template(3, nameof(MessageTemplates))]
    public string Message { get; set; }

    private static List<ListOption> _MessageTypeOptions;
    public static List<ListOption> MessageTypeOptions
    {
        get
        {
            if (_MessageTypeOptions == null)
            {
                _MessageTypeOptions = new List<ListOption>
                    {
                        new () { Label = "Information", Value = "Information"},
                        new () { Label = "Success", Value = "Success"},
                        new () { Label = "Warning", Value = "Warning" },
                        new () { Label = "Error", Value = "Error"},
                        new () { Label = "Failure", Value = "Failure"},
                        new () { Label = "Basic", Value = "Basic"},
                    };
            }
            return _MessageTypeOptions;
        }
    }

    private static List<ListOption> _MessageTemplates;
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

{{- if difference < 0 }}
File grew in size: {{ difference | math.abs | file_size }}
{{ else }}
File shrunk in size by: {{ difference | file_size }} / {{ percent }}%
{{ end }}"}
                };
            }
            return _MessageTemplates;
        }
    }
    
    const int colorInfo = 0x1F61E6;
    const int colorSuccess= 0x80E61F;
    const int colorError = 0xE7421F;
    const int colorFailure = 0xC61FE6;
    const int colorWarning = 0xE6C71F;

    public override int Execute(NodeParameters args)
    {
        try
        {
            var settings = args.GetPluginSettings<PluginSettings>();

            if (string.IsNullOrWhiteSpace(settings?.WebhookId))
            {
                args.Logger?.WLog("No webhook id set");
                return 2;
            }

            if (string.IsNullOrWhiteSpace(settings?.WebhookToken))
            {
                args.Logger?.WLog("No webhook token set");
                return 2;
            }

            var message = args.RenderTemplate!(Message);
            if (string.IsNullOrWhiteSpace(message))
            {
                args.Logger?.WLog("No message to send");
                return 2;
            }

            string title = args.ReplaceVariables(this.Title)?.EmptyAsNull() ??
                           this.MessageType?.EmptyAsNull() ?? "Information";
            

            // replace new lines
            message = message.Replace("\\r\\n", "\r\n");
            message = message.Replace("\\n", "\n");

            object webhook;
            if (this.MessageType == "Basic")
            {
                webhook = new
                {
                    username = "FileFlows",
                    content = message,
                    avatar_url = "https://fileflows.com/icon.png",
                };

            }
            else
            {
                webhook = new
                {
                    username = "FileFlows",
                    avatar_url = "https://fileflows.com/icon.png",
                    embeds = new[]
                    {
                        new
                        {
                            description = message,
                            title,
                            color = MessageType switch
                            {
                                "Success" => colorSuccess,
                                "Warning" => colorWarning,
                                "Error" => colorError,
                                "Failure" => colorFailure,
                                _ => colorInfo,
                            }
                        }
                    }
                };
            }


            string url = $"https://discordapp.com/api/webhooks/{settings.WebhookId}/{settings.WebhookToken}";

            var content = new StringContent(JsonSerializer.Serialize(webhook), Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var response = httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
                return 1;

            string error = response.Content.ReadAsStringAsync().Result;
            args.Logger?.WLog("Error from discord: " + error);
            return 2;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Error sending discord message: " + ex.Message);
            return 2;
        }
    }
}
