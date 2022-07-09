using System.ComponentModel;
using System.Text.Json;

namespace FileFlows.DiscordNodes.Communication;

public class Discord: Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Communication; 
    public override string Icon => "fab fa-discord";
    public override bool FailureNode => true;
    public override string HelpUrl => "https://docs.fileflows.com/plugins/discord/discord";

    [Required]
    [TextVariable(1)]
    public string Message { get; set; }

    [TextVariable(2)]
    public string Title { get; set; }

    [DefaultValue("standard")]
    [Select(nameof(MessageTypeOptions), 3)]
    public string MessageType { get; set; }

    private static List<ListOption> _MessageTypeOptions;
    public static List<ListOption> MessageTypeOptions
    {
        get
        {
            if (_MessageTypeOptions == null)
            {
                _MessageTypeOptions = new List<ListOption>
                    {
                        new ListOption { Label = "Information", Value = "Information"},
                        new ListOption { Label = "Success", Value = "Success"},
                        new ListOption { Label = "Warning", Value = "Warning" },
                        new ListOption { Label = "Error", Value = "Error"},
                        new ListOption { Label = "Failure", Value = "Failure"},
                        new ListOption { Label = "Basic", Value = "Basic"},
                    };
            }
            return _MessageTypeOptions;
        }
    }

    const int colorInfo = 0x1F61E6;
    const int colorSuccess= 0x80E61F;
    const int colorError = 0xE7421F;
    const int colorFailure = 0xC61FE6;
    const int colorWarning = 0xE6C71F;

    public override int Execute(NodeParameters args)
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

        string message = args.ReplaceVariables(this.Message);
        if (string.IsNullOrWhiteSpace(message))
        {
            args.Logger?.WLog("No message to send");
            return 2;
        }
        string title = args.ReplaceVariables(this.Title)?.EmptyAsNull() ?? this.MessageType?.EmptyAsNull() ?? "Information";

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
                    new {
                        description = message,
                        title = title,
                        color = this.MessageType switch
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
}
