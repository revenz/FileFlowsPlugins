namespace FileFlows.Web.FlowElements;

/// <summary>
/// 
/// </summary>
public class UrlToRelativePath : Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override string Icon => 

    /// <summary>
    /// Gets or sets the URL to get a path for
    /// </summary>
    [TextVariable(1)]
    public string Url { get; set; } = null!;
    
    public override int Execute(NodeParameters args)
    {
        
        // Create a Uri object from the URL
        var uri = new Uri(x);

        // Get the path without the query
        var path = uri.AbsolutePath;

        // Get the query part and replace the '=' and '&' with '-'
        var query = uri.Query.TrimStart('?').Replace('=', '-').Replace('&', '/');

        // Combine the path and the modified query
        var fakePath = path.TrimEnd('/') + (string.IsNullOrEmpty(query) ? string.Empty : "/" + query);

        // Remove leading slash
        if (fakePath.StartsWith("/"))
        {
            fakePath = fakePath.Substring(1);
        }
    }
}