using System.Net;
using System.Text.RegularExpressions;

namespace FileFlows.Web.FlowElements;

/// <summary>
/// Parses the text of an HTML file for images
/// </summary>
public class HtmlImageParser : HtmlParser
{
    /// <inheritdoc />
    public override string Icon => "fas fa-image";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/web/html-image-parser";
    
    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    
    /// <summary>
    /// Initialised a new instance of the HTML Image Parser
    /// </summary>
    public HtmlImageParser()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "ImageUrls", "A list of the Image URLs found" },
            { "CurrentList", "The common current list of strings" }
        };
    }

    /// <inheritdoc />
    protected override string VariableName => "ImageUrls";

    /// <inheritdoc />
    protected override List<string> ParseHtml(ILogger? logger, string html)
    {
        var imageUrls = new List<string>();
        var regex = new Regex("<img[^>]+src=(\"([^\"]*)\"|'([^']*)'|([^\\s>]+))", RegexOptions.IgnoreCase);
        var matches = regex.Matches(html);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var url = match.Groups[1].Value.TrimStart('"', '\'').TrimEnd('"', '\'');
                imageUrls.Add(WebUtility.HtmlDecode(url));
            }
        }
        regex = new Regex("<img[^>]+content=(\"([^\"]*)\"|'([^']*)'|([^\\s>]+))", RegexOptions.IgnoreCase);
        matches = regex.Matches(html);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var url = match.Groups[1].Value.TrimStart('"', '\'').TrimEnd('"', '\'');
                
                imageUrls.Add(WebUtility.HtmlDecode(url));
            }
        }

        return imageUrls.Distinct().ToList();
    }
}