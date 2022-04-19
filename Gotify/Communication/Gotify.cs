using System.ComponentModel;
using System.Text.Json;

namespace FileFlows.Gotify.Communication;

public class Gotify: Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Communication; 
    public override string Icon => "fas fa-bell";
    public override bool FailureNode => true;

    [Required]
    [TextVariable(1)]
    public string Message { get; set; }

    [TextVariable(2)]
    public string Title { get; set; }

    [NumberInt(3)]
    [Range(1, 100)]
    [DefaultValue(2)]
    public int Priority { get; set; } = 2;

    public override int Execute(NodeParameters args)
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

        string message = args.ReplaceVariables(this.Message);
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
        if(url.EndsWith("/") == false)
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
}
