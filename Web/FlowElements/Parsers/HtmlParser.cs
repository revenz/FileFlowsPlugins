using System.Net;
using System.Text.RegularExpressions;

namespace FileFlows.Web.FlowElements;

/// <summary>
/// Parses the text of an HTML file
/// </summary>
public abstract class HtmlParser : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;

    /// <inheritdoc />
    public override string Group => "Web";
    
    /// <summary>
    /// Gets or sets a file to process
    /// </summary>
    [TextVariable(1)]
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets the pattern of the links to match
    /// </summary>
    [TextVariable(2)]
    public string Pattern { get; set; } = null!;
    
    /// <summary>
    /// Gets the name of the variable to store the list in
    /// </summary>
    protected abstract string VariableName { get; } 

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var result = GetFileContent(args);
        if(result.Failed(out var error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }

        var html = result.Value;

        var list = ParseHtml(args.Logger, html);

        var pattern = args.ReplaceVariables(Pattern ?? string.Empty, stripMissing: true);
        if (string.IsNullOrWhiteSpace(pattern) == false)
        {
            Regex? rgxPattern = null;
            try
            {
                rgxPattern = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                // Ignored
            }

            list = list.Where(x =>
            {
                if (rgxPattern != null && rgxPattern.IsMatch(x))
                    return true;
                return pattern?.ToLowerInvariant().Contains(x, StringComparison.InvariantCultureIgnoreCase) == true;
            }).ToList();
        }
        
        args.Logger?.ILog("Items Found: " + list.Count);

        if (list.Count == 0)
        {
            args.Logger?.ILog("No items found");
            return 2;
        }

        // Log the found items
        foreach (var item in list)
        {
            args.Logger?.ILog("Found item: " + item);
        }

        args.Variables[VariableName] = list;
        // current list is the default current list FileFLows will use in a list flow element if no list is specified
        args.Variables["CurrentList"] = list;
        
        return 1;
    }

    /// <summary>
    /// Parses the HTML
    /// </summary>
    /// <param name="logger">the logger to use</param>
    /// <param name="html">the HTML to parse</param>
    /// <returns>the items found while pasrsing</returns>
    protected abstract List<string> ParseHtml(ILogger? logger, string html);

    /// <summary>
    /// Gets the file content
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the file content or a failure if cannot get the content</returns>
    private Result<string> GetFileContent(NodeParameters args)
    {
        var file = args.ReplaceVariables(Path ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile;
        if (args.FileService.FileExists(file) == false)
        {
            if (file.IndexOf("\n", StringComparison.Ordinal) > 0 || file.Length > 2000)
            {
                // treat this as file content
                return file;
            }
            return Result<string>.Fail("File does not exist: " + file);
        }
        
        var localFileResult = args.FileService.GetLocalPath(file);
        if (localFileResult.Failed(out var error))
        {
            return Result<string>.Fail(error);
        }

        if (File.Exists(localFileResult.Value) == false)
        {
            return Result<string>.Fail("File does not exist: " + localFileResult.Value);
        }

        if (new FileInfo(localFileResult.Value).Length > 10_000_000)
        {
            return Result<string>.Fail("File is to large to parse, limited to 10MB");
        }

        return File.ReadAllText(localFileResult.Value);
    }
}