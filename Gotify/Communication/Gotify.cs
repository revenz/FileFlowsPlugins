using System.ComponentModel;
using System.Text.Json;

namespace FileFlows.Gotify.Communication;

/// <summary>
/// A Gotify flow element that sends a message
/// </summary>
public class Gotify: Node
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
    public override string Icon => "svg:gotify";
    /// <summary>
    /// Gets if this can be used in a failure flow
    /// </summary>
    public override bool FailureNode => true;

    /// <inheritdoc />
    public override string CustomColor => "#6fc4e7";
    
    /// <summary>
    /// Gets the Help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/gotify";

    
    /// <summary>
    /// Gets or sets the title of the message
    /// </summary>
    [TextVariable(1)]
    public string Title { get; set; }
    

    /// <summary>
    /// Gets or sets the priority of the message
    /// </summary>
    [NumberInt(2)]
    [Range(1, 100)]
    [DefaultValue(2)]
    public int Priority { get; set; } = 2;
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    [Required]
    [Template(3, nameof(MessageTemplates))]
    public string Message { get; set; }

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

            if (string.IsNullOrWhiteSpace(settings?.AccessToken))
            {
                args.Logger?.WLog("No access token set");
                return 2;
            }

            if (string.IsNullOrWhiteSpace(settings?.ServerUrl))
            {
                args.Logger?.WLog("No server URL set");
                return 2;
            }

            string message = args.RenderTemplate!(this.Message);
            if (string.IsNullOrWhiteSpace(message))
            {
                args.Logger?.WLog("No message to send");
                return 2;
            }

            string title = args.ReplaceVariables(this.Title)?.EmptyAsNull() ?? "FileFlows";

            object data = new
            {
                title = title,
                message = message,
                priority = this.Priority < 0 ? 2 : this.Priority,
            };

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            string url = settings.ServerUrl;
            if (url.EndsWith("/") == false)
                url += "/";
            url += "message";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Gotify-Key", settings.AccessToken);
            var response = httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
                return 1;

            string error = response.Content.ReadAsStringAsync().Result;
            args.Logger?.WLog("Error from Gotify: " + error);
            return 2;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Error sending message: " + ex.Message);
            return 2;
        } 
    }
}
