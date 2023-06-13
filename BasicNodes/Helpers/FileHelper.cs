using System.Diagnostics;

namespace FileFlows.BasicNodes.Helpers;

/// <summary>
/// File Helper 
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Sets the last write time of a file
    /// </summary>
    /// <param name="filePath">the file path</param>
    /// <param name="utcDate">the UTC to set</param>
    public static void SetLastWriteTime(string filePath, DateTime utcDate)
        => System.IO.File.SetLastWriteTimeUtc(filePath, utcDate);

    /// <summary>
    /// Sets the last write time of a file
    /// </summary>
    /// <param name="filePath">the file path</param>
    /// <param name="utcDate">the UTC to set</param>
    public static void SetCreationTime(string filePath, DateTime utcDate)
        => System.IO.File.SetCreationTimeUtc(filePath, utcDate);

}