namespace FileFlows.BasicNodes.Tools;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System;
using System.Text;

/// <summary>
/// Flow element that makes a web request
/// </summary>
public class WebRequest : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Communication;
    /// <inheritdoc />
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override string Icon => "fas fa-globe";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/web-request";

    /// <inheritdoc />
    public override bool Obsolete => true;

    /// <inheritdoc />
    public override string ObsoleteMessage =>
        "This flow element has been replaced by the Web Request flow element in the Web plugin.  This flow element will be removed in a future update.";

    /// <summary>
    /// Gets or sets the URL
    /// </summary>
    [TextVariable(1)]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the method
    /// </summary>
    [Select(nameof(MethodOptions), 2)]
    public string Method { get; set; }

    private static List<ListOption>? _MethodOptions;
    
    /// <summary>
    /// Gets the method options
    /// </summary>
    public static List<ListOption> MethodOptions
    {
        get
        {
            if (_MethodOptions == null)
            {
                _MethodOptions = new List<ListOption>
                {
                    new () { Label = "GET", Value = "GET"},
                    new () { Label = "POST", Value = "POST"},
                    new () { Label = "PUT", Value = "PUT"},
                    new () { Label = "DELETE", Value = "DELETE"},
                };
            }
            return _MethodOptions;
        }
    }

    /// <summary>
    /// Gets or sets the content type
    /// </summary>
    [Select(nameof(ContentTypeOptions), 3)]
    public string ContentType { get; set; }

    private static List<ListOption>? _ContentTypeOptions;
    /// <summary>
    /// Gets the content type options
    /// </summary>
    public static List<ListOption> ContentTypeOptions
    {
        get
        {
            if (_ContentTypeOptions == null)
            {
                _ContentTypeOptions = new List<ListOption>
                {
                    new () { Label = "None", Value = ""},
                    new () { Label = "JSON", Value = "application/json"},
                    new () { Label = "Form Data", Value = "application/x-www-form-urlencoded"},
                };
            }
            return _ContentTypeOptions;
        }
    }


    /// <summary>
    /// Gest or sets any header
    /// </summary>
    [KeyValue(4, null)]
    public List<KeyValuePair<string, string>>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the body of the request
    /// </summary>
    [TextArea(5, variables: true)]
    public string Body { get; set; }


    private Dictionary<string, object>? _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    
    /// <summary>
    /// Initialises a new instace of the web request
    /// </summary>
    public WebRequest()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "web.StatusCode", 200 },
            { "web.Body", "this is a sample body" }
        };
    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        try
        {
            using var client = new HttpClient();


            string url = VariablesHelper.ReplaceVariables(this.Url, args.Variables, true, false, encoder: (string input) =>
            {
                if (string.IsNullOrEmpty(input))
                    return string.Empty;
                return Uri.EscapeDataString(input);
            });

            HttpMethod method = this.Method switch
            {
                "POST" => HttpMethod.Post,
                "PUT" => HttpMethod.Put,
                "DELETE" => HttpMethod.Delete,
                _ => HttpMethod.Get
            };
            args.Logger.ILog("Requesting: [" + method + "] " + url);
            HttpRequestMessage message = new HttpRequestMessage(method, url);

            if(this.Headers?.Any() == true)
            {
                foreach(var header in this.Headers)
                {
                    if (string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value))
                        continue;

                    message.Headers.Add(header.Key, header.Value);
                }
            }

            if (string.IsNullOrEmpty(this.ContentType) == false && method != HttpMethod.Get && string.IsNullOrWhiteSpace(this.Body) == false)
            {
                string body = args.ReplaceVariables(this.Body, stripMissing: false);
                message.Content = new StringContent(body, Encoding.UTF8, this.ContentType?.EmptyAsNull() ?? "application/json");
            }

            var result = client.Send(message);

            string stringBody = result.Content.ReadAsStringAsync().Result ?? string.Empty;

            args.UpdateVariables(new Dictionary<string, object>{
                { "web.StatusCode", (int)result.StatusCode },
                { "web.Body", stringBody }
            });

            if (result.IsSuccessStatusCode == false)
            {
                args.Logger.WLog("Non successfully status code returned: " + result.StatusCode);
                return 2;
            }
            args.Logger?.ILog("Successful status code returned: " + result.StatusCode);


            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed sending web request: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}
