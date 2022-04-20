namespace FileFlows.BasicNodes.Tools;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System;
using System.Text;

public class WebRequest : Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Communication;
    public override bool FailureNode => true;
    public override string Icon => "fas fa-globe";

    [TextVariable(1)]
    public string Url { get; set; }

    [Select(nameof(MethodOptions), 2)]
    public string Method { get; set; }

    private static List<ListOption> _MethodOptions;
    public static List<ListOption> MethodOptions
    {
        get
        {
            if (_MethodOptions == null)
            {
                _MethodOptions = new List<ListOption>
                {
                    new ListOption { Label = "GET", Value = "GET"},
                    new ListOption { Label = "POST", Value = "POST"},
                    new ListOption { Label = "PUT", Value = "PUT"},
                    new ListOption { Label = "DELETE", Value = "DELETE"},
                };
            }
            return _MethodOptions;
        }
    }

    [Select(nameof(ContentTypeOptions), 3)]
    public string ContentType { get; set; }

    private static List<ListOption> _ContentTypeOptions;
    public static List<ListOption> ContentTypeOptions
    {
        get
        {
            if (_ContentTypeOptions == null)
            {
                _ContentTypeOptions = new List<ListOption>
                {
                    new ListOption { Label = "None", Value = ""},
                    new ListOption { Label = "JSON", Value = "application/json"},
                    new ListOption { Label = "Form Data", Value = "application/x-www-form-urlencoded"},
                };
            }
            return _ContentTypeOptions;
        }
    }


    [KeyValue(4)]
    public List<KeyValuePair<string, string>> Headers { get; set; }

    [TextArea(5)]
    public string Body { get; set; }


    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    public WebRequest()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "web.StatusCode", 200 },
            { "web.Body", "this is a sample body" }
        };
    }

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
