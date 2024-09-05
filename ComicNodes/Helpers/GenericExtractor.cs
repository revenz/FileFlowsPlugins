// using SharpCompress.Archives;
// using System;
// using System.IO.Compression;
// using System.Text.RegularExpressions;
//
// namespace FileFlows.ComicNodes.Helpers;
//
// internal class GenericExtractor
// {
//     /// <summary>
//     /// Uncompresses a folder
//     /// </summary>
//     /// <param name="args">the node parameters</param>
//     /// <param name="workingFile">the file to extract</param>
//     /// <param name="destinationPath">the location to extract to</param>
//     /// <param name="halfProgress">if the NodeParameter.PartPercentageUpdate should end at 50%</param>
//     internal static void Extract(NodeParameters args, string workingFile, string destinationPath, bool halfProgress = true)
//     {
//         if (args?.PartPercentageUpdate != null)
//             args?.PartPercentageUpdate(halfProgress ? 50 : 0);
//
//         bool isRar = workingFile.ToLowerInvariant().EndsWith(".cbr");
//         try
//         {
//             ArchiveFactory.WriteToDirectory(workingFile, destinationPath);
//             PageNameHelper.FixPageNames(destinationPath);
//         }
//         catch (Exception ex) when (isRar && ex.Message.Contains("Unknown Rar Header"))
//         {
//             UnrarCommandLine.Extract(args, workingFile, destinationPath, halfProgress: halfProgress);
//         }
//
//         if (args?.PartPercentageUpdate != null)
//             args?.PartPercentageUpdate(halfProgress ? 50 : 100);
//     }
//
//     internal static int GetImageCount(NodeParameters args, string workingFile)
//     {
//         bool isRar = workingFile.ToLowerInvariant().EndsWith(".cbr");
//         try
//         {
//             var rgxImages = new Regex(@"\.(jpeg|jpg|jpe|png|bmp|tiff|webp|gif)$", RegexOptions.IgnoreCase);
//             using var archive = ArchiveFactory.Open(workingFile);
//             var files = archive.Entries.Where(entry => !entry.IsDirectory).ToArray();
//             return files.Count(x => x.Key != null && rgxImages.IsMatch(x.Key));
//         }
//         catch(Exception ex) when (isRar && ex.Message.Contains("Unknown Rar Header"))
//         {
//             return UnrarCommandLine.GetImageCount(args, workingFile); 
//         }
//     }
// }
