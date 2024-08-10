using System.Net;
using System.Text.RegularExpressions;

namespace FileFlows.Web.FlowElements;

/// <summary>
/// Parses the text of an HTML file for links
/// </summary>
public class HtmlLinkParser : HtmlParser
{
    /// <inheritdoc />
    public override string Icon => "fas fa-link";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/web/html-link-parser";
    
    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    
    /// <summary>
    /// Initialised a new instance of the HTML Link Parser
    /// </summary>
    public HtmlLinkParser()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "Links", "A list of the Links found" },
            { "CurrentList", "The common current list of strings" }
        };
    }

    /// <inheritdoc />
    protected override string VariableName => "Links";

    /// <inheritdoc />
    protected override List<string> ParseHtml(NodeParameters args, string html)
        => ParseHtmlForUrls(args, html, ["a"], ["href"]);

    private List<string> ParseHtmlOld(ILogger? logger, string html)
    {
        var urls = new List<string>();
        var regex = new Regex("<a[^>]+href=(\"([^\"]*)\"|'([^']*)'|([^\\s>]+))", RegexOptions.IgnoreCase);
        var matches = regex.Matches(html);

        string? baseUrl = null;
        if (Variables.TryGetValue("Url", out var oUrl) && oUrl is string sBaseUrl)
        {
            try
            {
                var uri = new Uri(sBaseUrl);

                // Get the absolute path without the query parameters
                baseUrl = uri.GetLeftPart(UriPartial.Path);

                // Ensure the path ends with a slash
                if (baseUrl.EndsWith("/") == false)
                    baseUrl += "/";

                // Use the folderPath as needed
                logger?.ILog("Base URL: " + baseUrl);
            }
            catch (Exception)
            {
                // Ignored
            }
        }
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var url = match.Groups[1].Value.TrimStart('"', '\'').TrimEnd('"', '\'');
                url = WebUtility.HtmlDecode(url);
                if (baseUrl != null && Regex.IsMatch(url, "^http(s)://", RegexOptions.IgnoreCase) == false)
                {
                    logger?.ILog("Relative URL: " + url);
                    if (url.StartsWith("/"))
                        url = url[1..];
                    url = baseUrl + url;
                    logger?.ILog("Absolute URL: " + url);
                }
                urls.Add(url);
            }
        }
        
        return urls.Distinct().ToList();
    }
}