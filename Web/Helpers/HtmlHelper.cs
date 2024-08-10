using HtmlAgilityPack;

namespace FileFlows.Web.Helpers;

/// <summary>
/// HTML Helper
/// </summary>
public class HtmlHelper
{
    
    /// <summary>
    /// Converts all relative URLs in the provided HTML content to absolute URLs based on the given base URL.
    /// This method processes the href attribute of <a> tags and the src and content attributes of <img> tags.
    /// </summary>
    /// <param name="htmlContent">The HTML content containing relative URLs.</param>
    /// <param name="baseUrl">The base URL to convert relative URLs to absolute URLs.</param>
    /// <returns>The HTML content with all relative URLs converted to absolute URLs.</returns>
    /// <exception cref="UriFormatException">Thrown when the base URL is not in a valid format.</exception>
    public static string ConvertRelativeUrlsToAbsolute(string htmlContent, string baseUrl)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var uri = new Uri(baseUrl);

        // Convert relative URLs in <a> tags (href attribute)
        foreach (var link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
        {
            var hrefValue = link.GetAttributeValue("href", string.Empty);
            if (Uri.TryCreate(hrefValue, UriKind.Relative, out var relativeUri))
            {
                var absoluteUri = new Uri(uri, relativeUri);
                link.SetAttributeValue("href", absoluteUri.ToString());
            }
        }

        // Convert relative URLs in <img> tags (src and content attributes)
        foreach (var img in htmlDoc.DocumentNode.SelectNodes("//img[@src or @content]"))
        {
            var srcValue = img.GetAttributeValue("src", string.Empty);
            if (Uri.TryCreate(srcValue, UriKind.Relative, out var relativeSrcUri))
            {
                var absoluteSrcUri = new Uri(uri, relativeSrcUri);
                img.SetAttributeValue("src", absoluteSrcUri.ToString());
            }

            var contentValue = img.GetAttributeValue("content", string.Empty);
            if (Uri.TryCreate(contentValue, UriKind.Relative, out var relativeContentUri))
            {
                var absoluteContentUri = new Uri(uri, relativeContentUri);
                img.SetAttributeValue("content", absoluteContentUri.ToString());
            }
        }

        return htmlDoc.DocumentNode.OuterHtml;
    }
}