#if(DEBUG)

using FileFlows.Plugin;
using FileFlows.Plugin.Models;
using FileFlows.Plugin.Services;

namespace BasicNodes.Tests;

public class TestFileService : IFileService
{
    public List<string> Files { get; set; }
    
    public Result<string[]> GetFiles(string path, string searchPattern = "", bool recursive = false)
    {
        if (Files?.Any() != true)
            return new string[] { };

        if (searchPattern.StartsWith("*"))
            searchPattern = searchPattern[1..];
        return Files.Where(x => x.EndsWith(searchPattern)).ToArray();
    }

    public Result<string[]> GetDirectories(string path)
    {
        throw new NotImplementedException();
    }

    public Result<bool> DirectoryExists(string path)
    {
        throw new NotImplementedException();
    }

    public Result<bool> DirectoryDelete(string path, bool recursive = false)
    {
        throw new NotImplementedException();
    }

    public Result<bool> DirectoryMove(string path, string destination)
    {
        throw new NotImplementedException();
    }

    public Result<bool> DirectoryCreate(string path)
    {
        throw new NotImplementedException();
    }

    public Result<DateTime> DirectoryCreationTimeUtc(string path)
    {
        throw new NotImplementedException();
    }

    public Result<DateTime> DirectoryLastWriteTimeUtc(string path)
    {
        throw new NotImplementedException();
    }

    public Result<bool> FileExists(string path)
    {
        throw new NotImplementedException();
    }

    public Result<FileInformation> FileInfo(string path)
    {
        throw new NotImplementedException();
    }

    public Result<bool> FileDelete(string path)
    {
        throw new NotImplementedException();
    }

    public Result<long> FileSize(string path)
    {
        throw new NotImplementedException();
    }

    public Result<DateTime> FileCreationTimeUtc(string path)
    {
        throw new NotImplementedException();
    }

    public Result<DateTime> FileLastWriteTimeUtc(string path)
    {
        throw new NotImplementedException();
    }

    public Result<bool> FileMove(string path, string destination, bool overwrite = true)
    {
        throw new NotImplementedException();
    }

    public Result<bool> FileCopy(string path, string destination, bool overwrite = true)
    {
        throw new NotImplementedException();
    }

    public Result<bool> FileAppendAllText(string path, string text)
    {
        throw new NotImplementedException();
    }

    public bool FileIsLocal(string path)
    {
        throw new NotImplementedException();
    }

    public Result<string> GetLocalPath(string path)
    {
        throw new NotImplementedException();
    }

    public Result<bool> Touch(string path)
    {
        throw new NotImplementedException();
    }

    public Result<long> DirectorySize(string path)
    {
        throw new NotImplementedException();
    }

    public Result<bool> SetCreationTimeUtc(string path, DateTime date)
    {
        throw new NotImplementedException();
    }

    public Result<bool> SetLastWriteTimeUtc(string path, DateTime date)
    {
        throw new NotImplementedException();
    }

    public char PathSeparator { get; init; }
    public ReplaceVariablesDelegate ReplaceVariables { get; set; }
    public ILogger? Logger { get; set; }
}

#endif