using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlows.ComicNodes.Helpers;

internal class ComicExtractor
{
    /// <summary>
    /// Extracts a comic book to a given folder
    /// </summary>
    /// <param name="args">the Node Parameters</param>
    /// <param name="file">the file to extract</param>
    /// <param name="destinationPath">the destination to extract files into, this should be an empty path</param>
    /// <param name="halfProgress">if the progress should be halfed when reporting</param>
    /// <param name="cancellation">the cancelation token</param>
    /// <returns>true is successfully extracted the comic, otherwise false</returns>
    internal static Result<bool> Extract(NodeParameters args, string file, string destinationPath, bool halfProgress, CancellationToken cancellation)
    {
        string currentFormat = new FileInfo(file).Extension;
        if (string.IsNullOrEmpty(currentFormat))
            return Result<bool>.Fail("Could not detect format for: " + file);
        
        if (currentFormat[0] == '.')
            currentFormat = currentFormat[1..]; // remove the dot
        currentFormat = currentFormat.ToUpper();

        Directory.CreateDirectory(destinationPath);
        args.Logger?.ILog("Extracting comic pages to: " + destinationPath);

        if (currentFormat == "PDF")
        {
            args.ImageHelper.ExtractPdfImages(file, destinationPath);
            // PdfHelper.Extract(args, file, destinationPath, "page", halfProgress: halfProgress,
            //     cancellation: cancellation);
        }
        else if (currentFormat is "CBZ" or "CB7" or "CBR" or "GZ" or "BZ2")
            return args.ArchiveHelper.Extract(file, destinationPath, (percent) =>
            {
                if (halfProgress)
                    percent /= 2;
                args.PartPercentageUpdate?.Invoke(percent);
            });
        else
            return Result<bool>.Fail("Unknown format:" + currentFormat);

        return true;
    }
}
