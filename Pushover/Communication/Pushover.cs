using System.ComponentModel;
using System.Text.Json;

namespace FileFlows.Pushover.Communication;

/// <summary>
/// A Pushover flow element that sends a message
/// </summary>
public class Pushover: Node
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
    public override string Icon => "svg:pushover";
    /// <summary>
    /// Gets if this can be used in a failure flow
    /// </summary>
    public override bool FailureNode => true;

    /// <inheritdoc />
    public override string CustomColor => "#40a6eb";
    
    /// <summary>
    /// Gets the Help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/pushover";
    

    /// <summary>
    /// Gets or sets the priority of the message
    /// </summary>
    [Select(nameof(Priorities), 1)]
    [DefaultValue(0)]
    public int Priority { get; set; } = 2;
    
    /// <summary>
    /// Gets or sets the expiry in seconds for the message
    /// </summary>
    [ConditionEquals(nameof(Priority), 2)]
    [NumberInt(2)]
    [DefaultValue(600)]
    [Range(1, 86400)]
    public int Expire { get; set; } = 2;
    /// <summary>
    /// Gets or sets the retry time in seconds
    /// </summary>
    [ConditionEquals(nameof(Priority), 2)]
    [NumberInt(2)]
    [DefaultValue(600)]
    [Range(30, 86400)]
    public int Retry { get; set; } = 2;

    private static List<ListOption>? _Priorities;
    /// <summary>
    /// Gets a list of message templates
    /// </summary>
    public static List<ListOption> Priorities
    {
        get
        {
            if (_Priorities == null)
            {
                _Priorities = new List<ListOption>
                {
                    new () { Label = "Lowest", Value = -1 },
                    new () { Label = "Normal", Value = 0 },
                    new () { Label = "High", Value = 1 },
                    new () { Label = "Emergency", Value = 2 }
                };
            }
            return _Priorities;
        }
    }


    /// <summary>
    /// Gets or sets the message
    /// </summary>
    [Required]
    [Template(3, nameof(MessageTemplates))]
    public string Message { get; set; } = string.Empty;

    private static List<ListOption>? _MessageTemplates;
    /// <summary>
    /// Gets a list of message templates
    /// </summary>
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
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        try
        {
            var settings = args.GetPluginSettings<PluginSettings>();

            if (string.IsNullOrWhiteSpace(settings?.UserKey))
            {
                args.Logger?.WLog("No user set");
                return 2;
            }

            if (string.IsNullOrWhiteSpace(settings?.ApiToken))
            {
                args.Logger?.WLog("No API Token set");
                return 2;
            }

            string message = args.RenderTemplate!(this.Message);
            if (string.IsNullOrWhiteSpace(message))
            {
                args.Logger?.WLog("No message to send");
                return 2;
            }

            List<KeyValuePair<string, string>> parameters = new ()
            {
                new ("token", settings.ApiToken),
                new ("user", settings.UserKey),
                new ("message", message),
                new ("priority", Priority.ToString())
            };

            if (Priority == 2)
            {
                int expire = Expire < 1 ? 1 : (Expire > 86400 ? 86400 : Expire);
                
                parameters.Add(new("expire", expire.ToString()));
                int retry = Retry < 30 ? 30 : (Retry > 86400 ? 86400 : Retry);
                
                parameters.Add(new("retry", retry.ToString()));
            }

            var content = new FormUrlEncodedContent(parameters);

            using var httpClient = new HttpClient();
            
            var response = httpClient.PostAsync("https://api.pushover.net/1/messages.json", content).Result;

            if (response.IsSuccessStatusCode)
                return 1;

            string error = response.Content.ReadAsStringAsync().Result;
            args.Logger?.WLog("Error from Pushover: " + error);
            return 2;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Error sending message: " + ex.Message);
            return 2;
        } 
    }
}
