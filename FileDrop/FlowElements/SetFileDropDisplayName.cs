// namespace FileFlows.FileDropPlugin.FlowElements;
//
// /// <summary>
// /// Sets the display name ot a file drop friendly name
// /// </summary>
// public class SetFileDropDisplayName : Node
// {
//     /// <inheritdoc />
//     public override int Inputs => 1;
//     /// <inheritdoc />
//     public override int Outputs => 1;
//     /// <inheritdoc />
//     public override string HelpUrl => "https://fileflows.com/docs/plugins/file-drop/set-file-drop-display-name";
//     /// <inheritdoc />
//     public override string Group => "File Drop";
//     /// <inheritdoc />
//     public override FlowElementType Type => FlowElementType.Process;
//     /// <inheritdoc />
//     public override string Icon => "fas fa-file-signature";
//
//     /// <inheritdoc />
//     public override int Execute(NodeParameters args)
//     {
//         var username = Variables["FileDropUser"]?.ToString() ??
//                           Variables["FileDropUserUid"]?.ToString();
//         if (string.IsNullOrWhiteSpace(username))
//         {
//             args.Logger?.WLog("Failed to get file drop username");
//             return 1;
//         }
//
//         var shortname = Variables["ShortName"]?.ToString();
//         if (string.IsNullOrWhiteSpace(shortname))
//         {
//             args.Logger?.WLog("Failed to get shortname");
//             return 1;
//         }
//         
//         args.SetDisplayName($"{username}: {shortname}");
//         
//         return base.Execute(args);
//     }
// }