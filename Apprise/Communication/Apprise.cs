using System.ComponentModel;
using System.Text.Json;

namespace FileFlows.Apprise.Communication;

public class Apprise: Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Communication; 
    public override string Icon => "fas fa-bell";
    public override bool FailureNode => true;

    [Required]
    [TextVariable(1)]
    public string Message { get; set; } = string.Empty;

    [StringArray(2)]
    public string[] Tag { get; set; } = new string[] { };

    [DefaultValue("info")]
    [Select(nameof(MessageTypeOptions), 3)]
    public string MessageType { get; set; } = string.Empty;

    private static List<ListOption> _MessageTypeOptions;
    public static List<ListOption> MessageTypeOptions
    {
        get
        {
            if (_MessageTypeOptions == null)
            {
                _MessageTypeOptions = new List<ListOption>
                    {
                        new ListOption { Label = "Information", Value = "info"},
                        new ListOption { Label = "Success", Value = "success"},
                        new ListOption { Label = "Warning", Value = "warning" },
                        new ListOption { Label = "Failure", Value = "failure"}
                    };
            }
            return _MessageTypeOptions;
        }
    }

    public override int Execute(NodeParameters args)
    {
        var settings = args.GetPluginSettings<PluginSettings>();

        if (string.IsNullOrWhiteSpace(settings?.Endpoint))
        {
            args.Logger?.WLog("No endpoint set");
            return 2;
        }
        if (string.IsNullOrWhiteSpace(settings?.ServerUrl))
        {
            args.Logger?.WLog("No server URL set");
            return 2;
        }

        string url = settings.ServerUrl;
        if (url.EndsWith("/") == false)
            url += "/";
        if (settings.Endpoint.EndsWith("/"))
            url += settings.Endpoint[1..];
        else
            url += settings.Endpoint;

        string message = args.ReplaceVariables(this.Message);
        if (string.IsNullOrWhiteSpace(message))
        {
            args.Logger?.WLog("No message to send");
            return 2;
        }

        object data = new
        {
            body = message,
            tag= Tag?.Any() != true ? "all" : String.Join(";", this.Tag),
            type = this.MessageType?.EmptyAsNull() ?? "info"
        };

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        var json = JsonSerializer.Serialize(data);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpClient = new HttpClient();    
        var response = httpClient.PostAsync(url, content).Result;
        if (response.IsSuccessStatusCode)
            return 1;

        string error = response.Content.ReadAsStringAsync().Result;
        args.Logger?.WLog("Error from Apprise: " + error);
        return 2;
    }
}
