using System.ComponentModel;
using System.Text.Json;

namespace FileFlows.Pushbullet.Communication;

/// <summary>
/// A Pushbullet flow element that sends a message
/// </summary>
public class Pushbullet: Node
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
    public override string Icon => "svg:pushbullet";
    /// <summary>
    /// Gets if this can be used in a failure flow
    /// </summary>
    public override bool FailureNode => true;

    /// <inheritdoc />
    public override string CustomColor => "#3b8f52";
    
    /// <summary>
    /// Gets the Help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/pushbullet";
    
    /// <summary>
    /// Gets or sets the title of the message
    /// </summary>
    [TextVariable(1)]
    [Required]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    [Required]
    [Template(2, nameof(MessageTemplates))]
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

            if (string.IsNullOrWhiteSpace(settings?.ApiToken))
            {
                args.Logger?.WLog("No API Token set");
                return 2;
            }

            string body = args.RenderTemplate!(this.Message);
            if (string.IsNullOrWhiteSpace(body))
            {
                args.Logger?.WLog("No message to send");
                return 2;
            }

            using var httpClient = new HttpClient();

            string title = args.ReplaceVariables(this.Title, stripMissing: true);
            
            // Set the authorization header with the Access Token
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.ApiToken);

            // Create the request content
#pragma warning disable IL2026
            var content = new StringContent(JsonSerializer.Serialize(
            new {
                type = "note",
                title,
                body 
            }), Encoding.UTF8, "application/json");
#pragma warning restore IL2026

            
            var response = httpClient.PostAsync("https://api.pushbullet.com/v2/pushes", content).Result;

            if (response.IsSuccessStatusCode)
                return 1;

            string error = response.Content.ReadAsStringAsync().Result;
            args.Logger?.WLog("Error from Pushbullet: " + error);
            return 2;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Error sending message: " + ex.Message);
            return 2;
        } 
    }
}
