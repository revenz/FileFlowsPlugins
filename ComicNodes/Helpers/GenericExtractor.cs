using SharpCompress.Archives;
using System;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace FileFlows.ComicNodes.Helpers;

internal class GenericExtractor
{
    /// <summary>
    /// Uncompresses a folder
    /// </summary>
    /// <param name="args">the node paratemers</param>
    /// <param name="workingFile">the file to extract</param>
    /// <param name="destinationPath">the location to extract to</param>
    /// <param name="halfProgress">if the NodeParameter.PartPercentageUpdate should end at 50%</param>
    internal static void Extract(NodeParameters args, string workingFile, string destinationPath, bool halfProgress = true)
    {
        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 0);

        ArchiveFactory.WriteToDirectory(workingFile, destinationPath);
        PageNameHelper.FixPageNames(destinationPath);

        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 100);
    }
}
