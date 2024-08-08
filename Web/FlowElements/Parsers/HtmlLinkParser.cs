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
    protected override List<string> ParseHtml(ILogger? logger, string html)
    {
        var urls = new List<string>();
        var regex = new Regex("<a[^>]+href=(\"([^\"]*)\"|'([^']*)'|([^\\s>]+))", RegexOptions.IgnoreCase);
        var matches = regex.Matches(html);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var url = match.Groups[1].Value.TrimStart('"', '\'').TrimEnd('"', '\'');
                urls.Add(WebUtility.HtmlDecode(url));
            }
        }
        
        return urls.Distinct().ToList();
    }
}