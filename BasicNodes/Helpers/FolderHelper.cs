using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Helpers;
using FileFlows.Plugin.Services;

namespace FileFlows.BasicNodes.Helpers;

/// <summary>
/// Folder Helper 
/// </summary>
public static class FolderHelper
{
    /// <summary>
    /// Gets additional files matching the criteria
    /// </summary>
    /// <param name="logger">the logger to use</param>
    /// <param name="fileService">the file serverice to use</param>
    /// <param name="replaceVariables">the function to replace variables in the patterns</param>
    /// <param name="shortNameLookup">the shortname of the source file to match against</param>
    /// <param name="directory">the directory to search in</param>
    /// <param name="patterns">the patterns of the additional files</param>
    /// <returns>a list of additional files found</returns>
    public static List<string> GetAdditionalFiles(ILogger logger, IFileService fileService, 
        Func<string, bool, bool, string> replaceVariables, string shortNameLookup, string directory, string[] patterns)
    {
        List<string> results = new();
        if (string.IsNullOrWhiteSpace(directory) || patterns == null || patterns.Length < 1)
            return results;

        logger?.ILog("Additional Files: " + string.Join(", ", patterns));

        try
        {
            logger?.ILog("Looking for additional files in directory: " + directory);
            foreach (var additionalOrig in patterns)
            {
                string additional = replaceVariables(additionalOrig, true, true);
                if (Regex.IsMatch(additionalOrig, @"^\.[a-z0-9A-Z]+$"))
                    additional = "*" + additional; // add the leading start for the search

                logger?.ILog("Looking for additional files: " + additional);
                var srcDirFiles = fileService.GetFiles(directory, additional).ValueOrDefault ?? new string[] { };
                foreach (var addFile in srcDirFiles)
                {
                    try
                    {
                        if (Regex.IsMatch(additional, @"\*\.[a-z0-9A-Z]+$"))
                        {
                            // make sure the file starts with same name
                            var addFileName = FileHelper.GetShortFileName(addFile);
                            if (addFileName.ToLowerInvariant().StartsWith(shortNameLookup.ToLowerInvariant()) ==
                                false)
                                continue;
                        }

                        logger?.ILog("Additional files: " + addFile);
                        results.Add(addFile);
                    }
                    catch (Exception ex)
                    {
                        logger?.ILog("Failed moving file: \"" + addFile + "\": " + ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.WLog("Error moving additional files: " + ex.Message);
        }

        return results;
    }

}