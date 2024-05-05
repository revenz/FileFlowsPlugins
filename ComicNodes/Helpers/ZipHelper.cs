using System;
using System.IO.Compression;

namespace FileFlows.ComicNodes.Helpers;

internal class ZipHelper
{
    /// <summary>
    /// Zips a folder to a file
    /// </summary>
    /// <param name="args">the NodeParameters</param>
    /// <param name="directory">the directory to zip</param>
    /// <param name="output">the output file of the zip</param>
    /// <param name="pattern">the file pattern to include in the zip</param>
    /// <param name="allDirectories">If all directories should be included or just the top most</param>
    /// <param name="halfProgress">if the NodePArameter.PartPercentageUpdate should start at 50%</param>
    internal static void Compress(NodeParameters args, string directory, string output, string pattern = "*.*", bool allDirectories = false, bool halfProgress = true)
    {

        var dir = new DirectoryInfo(directory);
        var files = dir.GetFiles(pattern, allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        using FileStream fs = new FileStream(output, FileMode.Create);
        using ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create);
        if(args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 0);
        float current = 0;
        float count = files.Length;
        foreach (var file in files)
        {
            ++count;
            string relative = file.FullName.Substring(dir.FullName.Length + 1);
            try
            {
                arch.CreateEntryFromFile(file.FullName, relative, CompressionLevel.SmallestSize);
            }
            catch (Exception ex)
            {
                args.Logger?.WLog("Failed to add file to zip: " + file.FullName + " => " + ex.Message);
            }
            if (args?.PartPercentageUpdate != null)
            {
                float percent = (current / count) * 100f;
                if (halfProgress)
                    percent = 50 + (percent / 2);
                args?.PartPercentageUpdate(percent);
            }
        }
        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(100);
    }

    internal static void Extract(NodeParameters args, string workingFile, string destinationPath, bool halfProgress = true)
    {
        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 0);

        ZipFile.ExtractToDirectory(workingFile, destinationPath);
        PageNameHelper.FixPageNames(destinationPath);

        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 100);
    }

    /// <summary>
    /// Checks if a file exist in the archive
    /// </summary>
    /// <param name="archivePath">the path to the archive</param>
    /// <param name="file">the file to look for</param>
    /// <returns>true if exists, otherwise false</returns>
    public static Result<bool> FileExists(string archivePath, string file)
    {
        // Check if the archive file exists
        if (File.Exists(archivePath) == false)
            return Result<bool>.Fail("Archive file not found: " + archivePath);

        try
        {
            // Open the zip archive
            using ZipArchive archive = ZipFile.OpenRead(archivePath);
            return archive.Entries.Any(x => x.FullName.ToLowerInvariant().Equals(file.ToLowerInvariant()));
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Adds a file to the archive
    /// </summary>
    /// <param name="archivePath">the path to the archive</param>
    /// <param name="file">the file to add</param>
    /// <returns>true if successfully added</returns>
    public static Result<bool> AddToArchive(string archivePath, string file)
    {
        // Check if the archive file exists
        if (File.Exists(archivePath) == false)
            return Result<bool>.Fail("Archive file not found: " + archivePath);
        
        try
        {
            // Open the zip archive
            using ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Update);
            
            // Create a new entry for the file
            archive.CreateEntryFromFile(file, Path.GetFileName(file));

            // Successfully added the file
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }
}
