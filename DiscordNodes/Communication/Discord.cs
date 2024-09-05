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
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message type
    /// </summary>
    [DefaultValue("standard")]
    [Select(nameof(MessageTypeOptions), 2)]
    public string MessageType { get; set; } = "standard";

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    [Required]
    [Template(3, nameof(MessageTemplates))]
    public string Message { get; set; } = string.Empty;

    private static List<ListOption>? _MessageTypeOptions;
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
    /// Gets or sets the API instance
    /// </summary>
    internal IDiscordApi? Api { get; set; }

    public override int Execute(NodeParameters args)
    {
        try
        {
            if (Api == null)
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

                Api = new DiscordApi(settings.WebhookId, settings.WebhookToken);
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

            var result = MessageType?.ToLowerInvariant() == "basic"
                ? Api.SendBasic(args.Logger!, message)
                : Api.SendAdvanced(args.Logger!, message, title, MessageType!);
            return result ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Error sending discord message: " + ex.Message);
            return 2;
        }
    }
}
