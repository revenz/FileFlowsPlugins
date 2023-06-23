// namespace FileFlows.BasicNodes.File;
//
// using System.ComponentModel;
// using FileFlows.Plugin;
// using FileFlows.Plugin.Attributes;
//
// /// <summary>
// /// Node that checks if a file is older than
// /// </summary>
// public class OlderThan : Node
// {
//     public override int Inputs => 1;
//     public override int Outputs => 2;
//     public override FlowElementType Type => FlowElementType.Logic;
//     public override string Icon => "fas fa-clock";
//     public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/older-than";
//
//     [Period(1)]
//     public int Minutes { get; set; }
//     
//     [NumberInt(2)]
//     [Select(nameof(DateOptions), 1)]
//     public int Date { get; set; }
//     
//     private static List<ListOption> _DateOptions;
//     /// <summary>
//     /// Gets or sets the date options
//     /// </summary>
//     public static List<ListOption> DateOptions
//     {
//         get
//         {
//             if (_DateOptions == null)
//             {
//                 _DateOptions = new List<ListOption>
//                 {
//                     new () { Label = "Date Created", Value = 0 },
//                     new () { Label = "Date Modified", Value = 1 }
//                 };
//             }
//             return _DateOptions;
//         }
//     }
//
//     public override int Execute(NodeParameters args)
//     {
//         var fi = new FileInfo(args.WorkingFile);
//         
//         if (fi.Exists == false) 
//         {
//             args.Logger.ELog("File Does not exist: " + args.WorkingFile);
//             return -1;
//         }
//
//         var date = Date == 1 ? fi.LastWriteTime : fi.CreationTime;
//
//         var minutes = DateTime.Now.Subtract(date).TotalMinutes;
//         return minutes > this.Minutes ? 1 : 2;
//     }
// }