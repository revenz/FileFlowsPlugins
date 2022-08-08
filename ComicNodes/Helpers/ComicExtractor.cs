using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlows.ComicNodes.Helpers;

internal class ComicExtractor
{
    internal static bool Extract(NodeParameters args, string file, string destinationPath, bool halfProgress, CancellationToken cancellation)
    {

        string currentFormat = new FileInfo(args.WorkingFile).Extension;
        if (string.IsNullOrEmpty(currentFormat))
        {
            args.Logger?.ELog("Could not detect format for: " + args.WorkingFile);
            return false;
        }
        if (currentFormat[0] == '.')
            currentFormat = currentFormat[1..]; // remove the dot
        currentFormat = currentFormat.ToUpper();

        Directory.CreateDirectory(destinationPath);
        args.Logger?.ILog("Extracting comic pages to: " + destinationPath);

        if (currentFormat == "PDF")
            PdfHelper.Extract(args, args.WorkingFile, destinationPath, "page", halfProgress: halfProgress, cancellation: cancellation);
        else if (currentFormat == "CBZ")
            ZipHelper.Extract(args, args.WorkingFile, destinationPath, halfProgress: halfProgress);
        else if (currentFormat == "CB7" || currentFormat == "CBR" || currentFormat == "GZ" || currentFormat == "BZ2")
            GenericExtractor.Extract(args, args.WorkingFile, destinationPath, halfProgress: halfProgress);
        else
            throw new Exception("Unknown format:" + currentFormat);

        return true;
    }
}
