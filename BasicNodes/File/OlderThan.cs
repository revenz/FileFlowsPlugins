namespace FileFlows.BasicNodes.File;

using System.ComponentModel;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

/// <summary>
/// Node that checks if a file is older than
/// </summary>
public class OlderThan : Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Logic;
    public override string Icon => "fas fa-clock";
    public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/older-than";


    [NumberInt(1)]
    public int Number { get; set; }

    [NumberInt(2)]
    [Select(nameof(UnitOptions), 1)]
    public int Unit { get; set; }
    
    [NumberInt(3)]
    [Select(nameof(DateOptions), 1)]
    public int Date { get; set; }
    
    
    private static List<ListOption> _UnitOptions;
    /// <summary>
    /// Gets or sets the unit options
    /// </summary>
    public static List<ListOption> UnitOptions
    {
        get
        {
            if (_UnitOptions == null)
            {
                _UnitOptions = new List<ListOption>
                {
                    new () { Label = "Minutes", Value = 1 },
                    new () { Label = "Days", Value = 1440 },
                    new () { Label = "Weeks", Value = 10080 }
                };
            }
            return _UnitOptions;
        }
    }
    
    
    private static List<ListOption> _DateOptions;
    /// <summary>
    /// Gets or sets the date options
    /// </summary>
    public static List<ListOption> DateOptions
    {
        get
        {
            if (_DateOptions == null)
            {
                _DateOptions = new List<ListOption>
                {
                    new () { Label = "Date Created", Value = 0 },
                    new () { Label = "Date Modified", Value = 1 }
                };
            }
            return _DateOptions;
        }
    }

    public override int Execute(NodeParameters args)
    {
        var fi = new FileInfo(args.WorkingFile);
        
        if (fi.Exists == false) 
        {
            args.Logger.ELog("File Does not exist: " + args.WorkingFile);
            return -1;
        }

        var date = Date == 1 ? fi.LastWriteTime : fi.CreationTime;

        // now, 1 week ago, == 7 days
        var minutes = DateTime.Now.Subtract(date).TotalMinutes;
        // 3 * 1 days, == 3 days
        var cutoff = this.Number * Math.Max(this.Unit, 1);
        // 7 > 3 == true, 1
        return minutes > cutoff ? 1 : 2;
    }
}