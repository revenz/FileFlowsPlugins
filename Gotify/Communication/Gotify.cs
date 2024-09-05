using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
    public string Title { get; set; } = string.Empty;
    

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
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
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
                title,
                message,
                priority = this.Priority < 0 ? 2 : this.Priority,
            };

            string url = settings.ServerUrl;
            if (url.EndsWith('/') == false)
                url += "/";
            url += "message";

            using var httpClient = new HttpClient();

            var updateResponse = GetWebRequest(httpClient, url, settings.AccessToken, JsonSerializer.Serialize(data));
            if (updateResponse.success == false)
            {
                if(string.IsNullOrWhiteSpace(updateResponse.body) == false)
                    args.Logger?.WLog("Error for Gotify:" + updateResponse.body);
                return 2;
            }
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Error sending message: " + ex.Message);
            return 2;
        } 
    }
    
    

    /// <summary>
    /// The method used to send the request
    /// </summary>
    private Func<HttpClient, string, string, string, (bool success, string body)>? _GetWebRequest;
    
    /// <summary>
    /// Gets the method used to send a request
    /// </summary>
    internal Func<HttpClient, string, string, string, (bool success, string body)> GetWebRequest
    {
        get
        {
            if(_GetWebRequest == null)
            {
                _GetWebRequest = (HttpClient client, string url, string accessToken, string json) =>
                {
                    try
                    {
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Add("X-Gotify-Key", accessToken);
                        var response = client.PostAsync(url, content).Result;
                        var respnoseBody = response.Content.ReadAsStringAsync().Result;
                        if(response.IsSuccessStatusCode)
                            return (response.IsSuccessStatusCode, respnoseBody);
                        return (response.IsSuccessStatusCode, respnoseBody?.EmptyAsNull() ?? response.ReasonPhrase ?? string.Empty);
                    }
                    catch(Exception ex)
                    {
                        return (false, ex.Message);
                    }
                };
            }
            return _GetWebRequest;
        }
#if(DEBUG)
        set
        {
            _GetWebRequest = value;
        }
#endif
    }
}
