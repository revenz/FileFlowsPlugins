﻿using System.Text.RegularExpressions;
using FileFlows.Web.Helpers;
using HttpMethod = System.Net.Http.HttpMethod;

namespace FileFlows.Web.FlowElements;

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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/web/web-request";
    /// <inheritdoc />
    public override string Group => "Web";

    /// <summary>
    /// Gets or sets the URL
    /// </summary>
    [TextVariable(1)]
    public string Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the method
    /// </summary>
    [Select(nameof(MethodOptions), 2)]
    public string Method { get; set; } = null!;

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
    public string ContentType { get; set; } = null!;
    
    const string CONTENT_TYPE_FORMDATA = "application/x-www-form-urlencoded";

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
                    new () { Label = "Form Data", Value = CONTENT_TYPE_FORMDATA},
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
    public string Body { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the variable to save the response object in
    /// </summary>
    [TextVariable(6)]
    public string ResponseVariable { get; set; } = null!;


    private Dictionary<string, object>? _Variables;
    public override Dictionary<string, object> Variables => _Variables!;
    
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
            args.Logger?.ILog("Requesting: [" + method + "] " + url);
            HttpRequestMessage message = new HttpRequestMessage(method, url);

            if(this.Headers?.Any() == true)
            {
                foreach(var header in this.Headers)
                {
                    if (string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value))
                        continue;

                    var headerKey = args.ReplaceVariables(header.Key, stripMissing: true);
                    var headerValue = args.ReplaceVariables(header.Value, stripMissing:true);
                    
                    if (string.IsNullOrEmpty(headerKey) || string.IsNullOrEmpty(headerValue))
                        continue;
                    
                    args.Logger?.ILog($"Header: {headerKey} = {headerValue}");
                    message.Headers.Add(headerKey, headerValue);
                }
            }

            if (string.IsNullOrEmpty(this.ContentType) == false && method != HttpMethod.Get && string.IsNullOrWhiteSpace(this.Body) == false)
            {
                string body = args.ReplaceVariables(this.Body, stripMissing: false);
                if (this.ContentType.Equals(CONTENT_TYPE_FORMDATA, StringComparison.OrdinalIgnoreCase))
                {
                    // Create a MultipartFormDataContent for form-data
                    var multipartContent = new MultipartFormDataContent();

                    // Add content to the form-data. Parse `Body` if it contains multiple fields.
                    // Example: Body = "field1=value1&field2=value2"
                    var keyValuePairs = body.Split('&')
                        .Select(p => p.Split('='))
                        .Where(parts => parts.Length == 2)
                        .Select(parts => new KeyValuePair<string, string>(parts[0], parts[1]));

                    foreach (var kvp in keyValuePairs)
                    {
                        args.Logger?.ILog($"Form Data: {kvp.Key} = {kvp.Value}");
                        multipartContent.Add(new StringContent(kvp.Value), kvp.Key);
                    }

                    message.Content = multipartContent;
                }
                else
                {
                    message.Content = new StringContent(body, Encoding.UTF8, this.ContentType?.EmptyAsNull() ?? "application/json");
                }
            }

            var result = client.Send(message);

            string stringBody = result.Content.ReadAsStringAsync().Result ?? string.Empty;
            
            // Try to parse the JSON response and add it to the dictionary
            var responseVariable = args.ReplaceVariables(ResponseVariable ?? string.Empty, stripMissing: true);
            if (string.IsNullOrEmpty(responseVariable) == false &&
                Regex.IsMatch(responseVariable, @"^[a-zA-Z_][a-zA-Z_0-9]*$"))
            {
                try
                {
                    var jsonResult = JsonUtils.DeserializeToDictionary(stringBody);
                    if (jsonResult != null)
                        args.Variables[responseVariable] = jsonResult;
                }
                catch (System.Text.Json.JsonException ex)
                {
                    args.Logger?.WLog("Failed to parse JSON response: " + ex.Message);
                }
            }

            args.UpdateVariables(new Dictionary<string, object>{
                { "web.StatusCode", (int)result.StatusCode },
                { "web.Body", stringBody }
            });

            if (result.IsSuccessStatusCode == false)
            {
                args.Logger?.WLog("Non successfully status code returned: " + result.StatusCode);
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
