using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Models;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Flow element that compares if a file is older than the given period
/// </summary>
public class FileDateCompare : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;

    /// <inheritdoc />
    public override string Icon => "fas fa-calendar";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/file-date-compare";

    /// <summary>
    /// Gets or sets the name of the file to check
    /// Leave blank to test the working file
    /// </summary>
    [TextVariable(1)]
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the Date
    /// </summary>
    [Select(nameof(DateOptions), 2)]
    public int Date { get; set; }

    private static List<ListOption> _DateOptions;

    /// <summary>
    /// Gets the date options to show in the UI
    /// </summary>
    public static List<ListOption> DateOptions
    {
        get
        {
            if (_DateOptions == null)
            {
                _DateOptions = new List<ListOption>
                {
                    new() { Label = "Date Created", Value = 0 },
                    new() { Label = "Date Modified", Value = 1 }
                };
            }

            return _DateOptions;
        }
    }

    /// <summary>
    /// Gets or sets the date comparison mode 
    /// </summary>
    [DateCompare(3)]
    public DateCompareModel DateComparision { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string file = args.ReplaceVariables(FileName ?? string.Empty, true)?.EmptyAsNull() ?? args.WorkingFile;
        if (string.IsNullOrWhiteSpace(file))
        {
            args.FailureReason = "FileName not set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var result = Date == 0
            ? args.FileService.FileCreationTimeUtc(file)
            : args.FileService.FileLastWriteTimeUtc(file);

        if (result.Failed(out var error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var date = result.Value;
        args.Logger?.ILog($"Mode: {(Date == 1 ? "Last Write Time" : "Date Created")}");
        args.Logger?.ILog($"Date: {date:yyyy-MM-ddTHH:mm:ss}Z");

        if (DateComparision == null)
        {
            args.FailureReason = "Comparison is null";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        args.Logger?.ILog($"Comparison: {DateComparision.Comparison}");
        if (DateComparision.Comparison == DateCompareMode.Any)
        {
            return 1;
        }

        var now = DateTime.UtcNow;
        var date1 = DateComparision.Comparison is DateCompareMode.After or DateCompareMode.Before
            ? DateComparision.DateValue
            : now.AddMinutes(-DateComparision.Value1);

        if (DateComparision.Comparison is DateCompareMode.Before or DateCompareMode.LessThan)
        {
            if (date <= date1)
            {
                args.Logger?.ILog($"Date is before: {date1:yyyy-MM-ddTHH:mm:ss}Z");
                return 1;
            }

            args.Logger?.ILog($"Date is not before: {date1:yyyy-MM-ddTHH:mm:ss}Z");
            return 2;
        }

        if (DateComparision.Comparison is DateCompareMode.After or DateCompareMode.GreaterThan)
        {
            if (date > date1)
            {
                args.Logger?.ILog($"Date is after: {date1:yyyy-MM-ddTHH:mm:ss}Z");
                return 1;
            }

            args.Logger?.ILog($"Date is not after: {date1:yyyy-MM-ddTHH:mm:ss}Z");
            return 2;
        }

        // then it must be a between or not between
        int low = Math.Min(DateComparision.Value1, DateComparision.Value2);
        int high = Math.Max(DateComparision.Value1, DateComparision.Value2);

        DateTime lowDate = now.AddMinutes(-low);
        DateTime highDate = now.AddMinutes(-high);
        
        bool isBetween = now >= lowDate && now <= highDate;
        args.Logger?.ILog(
            $"Date is {(isBetween ? "" : "not ")}between {lowDate:yyyy-MM-ddTHH:mm:ss}Z and {highDate:yyyy-MM-ddTHH:mm:ss}Z");

        if (DateComparision.Comparison is DateCompareMode.Between)
        {
            return isBetween ? 1 : 2;
        }

        // not between
        return isBetween ? 2 : 1;
    }

}