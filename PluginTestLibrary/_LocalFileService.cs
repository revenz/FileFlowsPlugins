using FileFlows.Plugin;
using FileFlows.Plugin.Models;
using FileFlows.Plugin.Services;
using System.IO;
using FileHelper = FileFlows.Plugin.Helpers.FileHelper;

namespace PluginTestLibrary;

/// <summary>
/// Local file service
/// </summary>
public class LocalFileService : IFileService
{
    /// <summary>
    /// Gets or sets the path separator for the file system
    /// </summary>
    public char PathSeparator { get; init; } = Path.DirectorySeparatorChar;

    public ReplaceVariablesDelegate? ReplaceVariables { get; set; }

    /// <summary>
    /// Gets or sets the allowed paths the file service can access
    /// </summary>
    public string[] AllowedPaths { get; init; } = null!;
    
    
    /// <summary>
    /// Gets or sets the permissions to use for files
    /// </summary>
    public int? Permissions { get; set; }
    
    /// <summary>
    /// Gets or sets the owner:group to use for files
    /// </summary>
    public string OwnerGroup { get; set; } = null!;

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

    /// <inheritdoc />
    public Result<bool> DirectoryEmpty(string path, string[]? includePatterns = null)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);

        try
        {
            if (!Directory.Exists(path))
                return Result<bool>.Success(true); // Path doesn't exist, considered empty

            // Get all files in the directory
            var files = Directory.GetFiles(path);

            // If there are patterns, only count matching files
            if (includePatterns != null && includePatterns.Length > 0)
            {
                foreach (var file in files)
                {
                    foreach (var pattern in includePatterns)
                    {
                        try
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(file, pattern.Trim(),
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                return Result<bool>.Success(false); // File matches, directory is not empty
                        }
                        catch (Exception)
                        {
                            // Handle regex exceptions silently, as per your example
                        }
                    }
                }
            }
            else if (files.Length > 0)
            {
                // No patterns provided, directory is not empty if any file exists
                return Result<bool>.Success(false);
            }

            // Check for directories (subdirectories are not affected by includePatterns)
            var dirs = Directory.GetDirectories(path);
            if (dirs.Length > 0)
                return Result<bool>.Success(false); // Directory contains subdirectories, not empty

            return Result<bool>.Success(true); // Directory is empty
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<bool>.Fail("Unauthorized access to path: " + path + " - " + ex.Message);
        }
        catch (IOException ex)
        {
            return Result<bool>.Fail("IO error while accessing path: " + path + " - " + ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail("Error while accessing path: " + path + " - " + ex.Message);
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
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            return System.IO.File.Exists(path);
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
            System.IO.File.AppendAllText(path, text);
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
            if (System.IO.File.Exists(path))
                System.IO.File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
            else
            {
                System.IO.File.Create(path);
                SetPermissions(path);
            }

            return true;
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Failed to touch file: '{path}' => {ex.Message}");
        }
    }

    public Result<long> DirectorySize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return 0;
        
        if (System.IO.File.Exists(path))
            path = new FileInfo(path).Directory?.FullName ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(path))
            return 0;
        
        if (Directory.Exists(path) == false)
            return 0;
        
        try
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            return dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(x => x.Length);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public Result<bool> SetCreationTimeUtc(string path, DateTime date)
    {
        if (IsProtectedPath(ref path))
            return Result<bool>.Fail("Cannot access protected path: " + path);
        try
        {
            if (!System.IO.File.Exists(path))
                return Result<bool>.Fail("File not found.");

            System.IO.File.SetCreationTimeUtc(path, date);
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
            if (!System.IO.File.Exists(path))
                return Result<bool>.Fail("File not found.");

            System.IO.File.SetLastWriteTimeUtc(path, date);
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
        

        if ((System.IO.File.Exists(path) == false && Directory.Exists(path) == false))
        {
            logMethod("SetPermissions: File doesnt existing, skipping");
            return;
        }
        
        //StringLogger stringLogger = new StringLogger();
        var logger = new TestLogger();

        bool isFile = new FileInfo(path).Exists;

        FileHelper.SetPermissions(logger, path, file: isFile, permissions: permissions);
        
        FileHelper.ChangeOwner(logger, path, file: isFile, ownerGroup: OwnerGroup);
        
        logMethod(logger.ToString());
    }
}