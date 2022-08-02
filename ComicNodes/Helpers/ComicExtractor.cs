using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlows.ComicNodes.Helpers
{
    internal class ComicExtractor
    {
        internal static bool Extract(NodeParameters args, string file, string destinationPath, bool halfProgress)
        {

            string currentFormat = new FileInfo(args.WorkingFile).Extension;
            if (string.IsNullOrEmpty(currentFormat))
            {
                args.Logger?.ELog("Could not detect format for: " + args.WorkingFile);
                return false;
            }
            if (currentFormat[0] == '.')
                currentFormat = currentFormat[1..]; // remove the dot
            currentFormat = currentFormat.ToLower();

            Directory.CreateDirectory(destinationPath);
            args.Logger?.ILog("Extracting comic pages to: " + destinationPath);

            if (currentFormat == "pdf")
                PdfHelper.Extract(args, args.WorkingFile, destinationPath, "page", halfProgress: halfProgress);
            else if (currentFormat == "cbz")
                ZipHelper.Extract(args, args.WorkingFile, destinationPath, halfProgress: halfProgress);
            else if (currentFormat == "cb7" || currentFormat == "cbr" || currentFormat == "gz" || currentFormat == "bz2")
                GenericExtractor.Extract(args, args.WorkingFile, destinationPath, halfProgress: halfProgress);
            else
                throw new Exception("Unknown format:" + currentFormat);

            return true;
        }
    }
}
