#if(DEBUG)
using FileFlows.Plugin;
using FileFlows.Plugin.Models;
using FileFlows.Plugin.Services;
using System.IO;
using System.Linq;

namespace FileFlows.ImageNodes.Tests;

public class LocalFileService : IFileService
{
    /// <summary>
    /// Gets or sets the path separator for the file system
    /// </summary>
    public char PathSeparator { get; init; } = Path.DirectorySeparatorChar;
    
    /// <summary>
    /// Gets or sets the allowed paths the file service can access
    /// </summary>
    public string[]? AllowedPaths { get; init; }

    /// <summary>
    /// Gets or sets a function for replacing variables in a string.
    /// </summary>
    /// <remarks>
    /// The function takes a string input, a boolean indicating whether to strip missing variables,
    /// and a boolean indicating whether to clean special characters.
    /// </remarks>
    public ReplaceVariablesDelegate? ReplaceVariables { get; set; }
    
    /// <summary>
    /// Gets or sets the permissions to use for files
    /// </summary>
    public int? Permissions { get; set; }
    
    /// <summary>
    /// Gets or sets the owner:group to use for files
    /// </summary>
    public string? OwnerGroup { get; set; }

    /// <summary>
    /// Gets or sets the logger used for logging
    /// </summary>
    public ILogger? Logger { get; set; }

    public Result<string[]> GetFiles(string path, string searchPattern = "", bool recursive = false)
    {
        if (IsProtectedPath(ref path))
            return Result<string[]>.Fail("Cannot access protected path: " + path);
        try
        {
            return Directory.GetFiles(path, searchPattern ?? string.Empty,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
        catch (Exception)
        {
            return new string[] { };
        }
    }

    public Result<string[]> GetDirectories(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<string[]>.Fail("Cannot access protected path: " + path);
        try
        {
            return Directory.GetDirectories(path);
        }
        catch (Exception)
        {
            return new string[] { };
        }
    }

    public Result<bool> DirectoryExists(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            return Directory.Exists(path);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Result<bool> DirectoryDelete(string path, bool recursive = false)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            Directory.Delete(path, recursive);
            return true;
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public Result<bool> DirectoryMove(string path, string destination)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        if (IsProtectedPath(ref destination))
            return Result<bool>.Fail("Cannot access protected path: " + destination);
        try
        {
            Directory.Move(path, destination);
            SetPermissions(destination);
            return true;
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public Result<bool> DirectoryCreate(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            var dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists == false)
                dirInfo.Create();
            SetPermissions(path);
            return true;
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public Result<bool> FileExists(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            return File.Exists(path);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Result<FileInformation> FileInfo(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<FileInformation>.Fail("Cannot access protected path: " + path);
        try
        {
            FileInfo fileInfo = new FileInfo(path);

            return new FileInformation
            {
                CreationTime = fileInfo.CreationTime,
                CreationTimeUtc = fileInfo.CreationTimeUtc,
                LastWriteTime = fileInfo.LastWriteTime,
                LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
                Extension = fileInfo.Extension.TrimStart('.'),
                Name = fileInfo.Name,
                FullName = fileInfo.FullName,
                Length = fileInfo.Length,
                Directory = fileInfo.DirectoryName!
            };
        }
        catch (Exception ex)
        {
            return Result<FileInformation>.Fail(ex.Message);
        }
    }

    public Result<bool> FileDelete(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            var fileInfo = new FileInfo(path);
            if(fileInfo.Exists)
                fileInfo.Delete();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Result<long> FileSize(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<long>.Fail("Cannot access protected path: " + path);
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists == false)
                return Result<long>.Fail("File does not exist");
            return fileInfo.Length;
        }
        catch (Exception ex)
        {
            return Result<long>.Fail(ex.Message);
        }
    }

    public Result<DateTime> FileCreationTimeUtc(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<DateTime>.Fail("Cannot access protected path: " + path);
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists == false)
                return Result<DateTime>.Fail("File does not exist");
            return fileInfo.CreationTimeUtc;
        }
        catch (Exception ex)
        {
            return Result<DateTime>.Fail(ex.Message);
        }
    }

    public Result<DateTime> FileLastWriteTimeUtc(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<DateTime>.Fail("Cannot access protected path: " + path);
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists == false)
                return Result<DateTime>.Fail("File does not exist");
            return fileInfo.LastWriteTimeUtc;
        }
        catch (Exception ex)
        {
            return Result<DateTime>.Fail(ex.Message);
        }
    }

    public Result<bool> FileMove(string path, string destination, bool overwrite = true)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        if (IsProtectedPath(ref destination))
            return Result<bool>.Fail("Cannot access protected path: " + destination);
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists == false)
                return Result<bool>.Fail("File does not exist");
            var destDir = new FileInfo(destination).Directory!;
            if (destDir.Exists == false)
            {
                destDir.Create();
                SetPermissions(destDir.FullName);
            }

            fileInfo.MoveTo(destination, overwrite);
            SetPermissions(destination);
            return true;
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public Result<bool> FileCopy(string path, string destination, bool overwrite = true)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        if (IsProtectedPath(ref destination))
            return Result<bool>.Fail("Cannot access protected path: " + destination);
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists == false)
                return Result<bool>.Fail("File does not exist");
            
            var destDir = new FileInfo(destination).Directory!;
            if (destDir.Exists == false)
            {
                destDir.Create();
                SetPermissions(destDir.FullName);
            }

            fileInfo.CopyTo(destination, overwrite);
            SetPermissions(destination);
            return true;
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public Result<bool> FileAppendAllText(string path, string text)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            File.AppendAllText(path, text);
            SetPermissions(path);
            return true;
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public bool FileIsLocal(string path) => true;

    /// <summary>
    /// Gets the local path
    /// </summary>
    /// <param name="path">the path</param>
    /// <returns>the local path to the file</returns>
    public Result<string> GetLocalPath(string path)
        => Result<string>.Success(path);

    public Result<bool> Touch(string path)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        
        if (DirectoryExists(path).Is(true))
        {
            try
            {
                Directory.SetLastWriteTimeUtc(path, DateTime.UtcNow);
                return true;
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail("Failed to touch directory: " + ex.Message);
            }
        }
        
        try
        {
            if (File.Exists(path))
                File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
            else
            {
                File.Create(path);
                SetPermissions(path);
            }

            return true;
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Failed to touch file: '{path}' => {ex.Message}");
        }
    }

    public Result<bool> SetCreationTimeUtc(string path, DateTime date)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            if (!File.Exists(path))
                return Result<bool>.Fail("File not found.");

            File.SetCreationTimeUtc(path, date);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error setting creation time: {ex.Message}");
        }
    }

    public Result<bool> SetLastWriteTimeUtc(string path, DateTime date)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            if (!File.Exists(path))
                return Result<bool>.Fail("File not found.");

            File.SetLastWriteTimeUtc(path, date);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error setting last write time: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a path is accessible by the file server
    /// </summary>
    /// <param name="path">the path to check</param>
    /// <returns>true if accessible, otherwise false</returns>
    private bool IsProtectedPath(ref string path)
    {
        if (OperatingSystem.IsWindows())
            path = path.Replace("/", "\\");
        else
            path = path.Replace("\\", "/");
        
        if(ReplaceVariables != null)
            path = ReplaceVariables(path, true);
        
        if (FileHelper.IsSystemDirectory(path))
            return true; // a system directory, no access

        if (AllowedPaths?.Any() != true)
            return false; // no allowed paths configured, allow all

        if (OperatingSystem.IsWindows())
            path = path.ToLowerInvariant();
        
        for(int i=0;i<AllowedPaths.Length;i++)
        {
            string p = OperatingSystem.IsWindows() ? AllowedPaths[i].ToLowerInvariant().TrimEnd('\\') : AllowedPaths[i].TrimEnd('/');
            if (path.StartsWith(p))
                return false;
        }

        return true;
    }

    public void SetPermissions(string path, int? permissions = null, Action<string>? logMethod = null)
    {
        logMethod ??= (string message) => Logger?.ILog(message);
        
        permissions = permissions != null && permissions > 0 ? permissions : Permissions;
        if (permissions == null || permissions < 1)
            permissions = 777;
        

        if ((File.Exists(path) == false && Directory.Exists(path) == false))
        {
            logMethod("SetPermissions: File doesnt existing, skipping");
            return;
        }
        
        //StringLogger stringLogger = new StringLogger();
        var logger = new TestLogger();

        bool isFile = new FileInfo(path).Exists;

        FileHelper.SetPermissions(logger, path, file: isFile, permissions: permissions);
        
        FileHelper.ChangeOwner(logger, path, file: isFile, ownerGroup: OwnerGroup ?? string.Empty);
        
        logMethod(logger.ToString());
    }
}
#endif