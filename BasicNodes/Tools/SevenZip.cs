using System.ComponentModel;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Helpers;

using System.IO;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// A flow element that 7zips files or directories.
/// </summary>
public class SevenZip : Node
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 1;
    /// <summary>
    /// Gets the element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;

    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "svg:7zip";
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/seven-zip";
    
    private string _DestinationPath = string.Empty;
    private string _DestinationFile = string.Empty;

    /// <summary>
    /// Gets or sets the destination path for zipping.
    /// </summary>
    [Folder(2)]
    public string DestinationPath
    {
        get => _DestinationPath;
        set { _DestinationPath = value ?? ""; }
    }

    /// <summary>
    /// Gets or sets the destination file name for zipping.
    /// </summary>
    [TextVariable(3)]
    public string DestinationFile
    {
        get => _DestinationFile;
        set { _DestinationFile = value ?? ""; }
    }
    
    /// <summary>
    /// Gets or sets the level of compression
    /// </summary>
    [DefaultValue("lzma2")]
    [Select(nameof(CompressionMethods), 0)]
    public string CompressionMethod { get; set; }
    
    private static List<ListOption> _CompressionMethods;

    public static List<ListOption> CompressionMethods
    {
        get
        {
            if (_CompressionMethods == null)
            {
                _CompressionMethods = new List<ListOption>
                {
                    new () { Value = "lzma2", Label = "LZMA2 - Improved LZMA Compression (Slower)" },
                    new () { Value = "lzma", Label = "LZMA - Better Compression (Slower)" },
                    new () { Value = "ppmd", Label = "PPMd - Balanced Compression (Slower)" },
                    new () { Value = "bzip2", Label = "BZip2 - Good Compression (Moderate)" },
                    new () { Value = "deflate", Label = "Deflate - Standard ZIP Compression (Moderate)" },
                    new () { Value = "copy", Label = "Copy - No Compression (Fastest)" }
                };
            }
            return _CompressionMethods;
        }
    }

    /// <summary>
    /// Gets or sets the level of compression
    /// </summary>
    [Select(nameof(CompressionLevels), 1)]
    public int CompressionLevel { get; set; }
    
    private static List<ListOption> _CompressionLevels;
    
    public static List<ListOption> CompressionLevels
    {
        get
        {
            if (_CompressionLevels == null)
            {
                _CompressionLevels = new List<ListOption>
                {
                    // we are 1 based here, even though 0 is the actually 0
                    // we use 1 so we can detect if its not set and use a sensible default
                    new () { Label = "0 - No compression", Value = 1 },
                    new () { Label = "1 - Fastest compression", Value = 2 },
                    new () { Label = "2 - Less compression", Value = 3 },
                    new () { Label = "3 - Fast compression", Value = 4 },
                    new () { Label = "4 - Normal compression", Value = 5 },
                    new () { Label = "5 - Balanced compression", Value = 6 },
                    new () { Label = "6 - Maximum speed compression", Value = 7 },
                    new () { Label = "7 - Maximum compression", Value = 8 },
                    new () { Label = "8 - Ultra compression", Value = 9 },
                    new () { Label = "9 - Insane compression", Value = 10 }
                };
            }
            return _CompressionLevels;
        }
    }

    private Result<string> GetItemToCompress(NodeParameters args)
    {
        var localResult = args.FileService.GetLocalPath(args.WorkingFile);
        if (localResult.IsFailed)
            return Result<string>.Fail("Failed to ensure file is local: " + localResult.Error);

        string itemToCompress = localResult.Value;
        if (Directory.Exists(itemToCompress) == false)
            return itemToCompress;
        
        return FileHelper.Combine(itemToCompress, "*");
    }

    private Result<string> GetDestinationPath(NodeParameters args, bool isDir)
    {
        string destDir = DestinationPath;
        if (string.IsNullOrEmpty(destDir))
        {
            args.Logger?.ILog("No destination path set");
            if (isDir)
                destDir = new DirectoryInfo(args.LibraryPath).FullName;
            else
                destDir = FileHelper.GetDirectory(args.FileName);
                
            if (string.IsNullOrEmpty(destDir))
                return Result<string>.Fail("Failed to get destination directory");
        }
        else
        {
            // in case they set a linux path on windows or vice versa
            destDir = destDir.Replace('\\', Path.DirectorySeparatorChar);
            destDir = destDir.Replace('/', Path.DirectorySeparatorChar);

            destDir = args.ReplaceVariables(destDir, stripMissing: true);

            destDir = destDir.TrimEnd(Path.PathSeparator);

            // this converts it to the actual OS path
            var result = args.FileService.DirectoryCreate(destDir);
            if (result.IsFailed)
                return Result<string>.Fail("Failed creating directory: " + result.Error);
        }
        args.Logger?.ILog("Destination Folder: " + destDir);
        return destDir;
    }

    private Result<string> GetSevenZipLocation(NodeParameters args)
    {
        var sevenZip = args.GetToolPath("7zip")?.EmptyAsNull() ?? "7z";
        var result = args.Execute(new()
        {
            Command = sevenZip
        });
        if (result.ExitCode == 0)
            return sevenZip;

        if (OperatingSystem.IsWindows())
        {
            // try default installation of 7zip
            args.Logger?.ILog("7zip not found, trying default Windows install path");
            string programFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string sevenZipPath = Path.Combine(programFilesDir, "7-Zip", "7z.exe");
            if (System.IO.File.Exists(sevenZipPath))
            {
                args.Logger?.ILog("7zip found at: " + sevenZipPath);
                return sevenZipPath;
            }

            args.Logger?.ILog("Failed to locate 7zip in default Windows installation path: " + sevenZipPath);
        }
        return Result<string>.Fail("7zip not found on processing node.");
    }


    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        try
        {
            var sevenZipResult = GetSevenZipLocation(args);
            if (sevenZipResult.IsFailed)
            {
                args.FailureReason = sevenZipResult.Error;
                args.Logger?.ELog(sevenZipResult.Error);
                return -1;
            }

            string sevenZip = sevenZipResult.Value;

            var itemToCompressResult = GetItemToCompress(args);
            if (itemToCompressResult.IsFailed)
            {
                args.FailureReason = itemToCompressResult.Error;
                args.Logger?.ELog(itemToCompressResult.Error);
                return -1;
            }
            string itemToCompress = itemToCompressResult.Value;
            bool isDir = itemToCompress.EndsWith("*"); 

            var destDirResult = GetDestinationPath(args, isDir);
            if (destDirResult.IsFailed)
            {
                args.FailureReason = destDirResult.Error;
                args.Logger?.ELog(destDirResult.Error);
                return -1;
            }

            var destDir = destDirResult.Value;
            
            
            string destFile = args.ReplaceVariables(DestinationFile ?? string.Empty, true);
            if (string.IsNullOrEmpty(destFile))
            {
                if (isDir)
                    destFile = new DirectoryInfo(args.FileName).Name + ".7z";
                else
                    destFile = FileHelper.GetShortFileName(args.FileName) + ".7z";
            }
            if (destFile.ToLower().EndsWith(".7z") == false)
                destFile += ".7z";
            destFile = destDir + args.FileService.PathSeparator + destFile;

            args.Logger?.ILog($"Compressing '{args.WorkingFile}' to '{destFile}'");

            int compression = CompressionLevel < 1 ? 5 : CompressionLevel;

            if (args.FileService.FileExists(destFile).Is(true))
            {
                args.Logger?.ILog("Destination file already exists, deleting: " + destFile);
                var result = args.FileService.FileDelete(destFile);
                if (result.IsFailed)
                {
                    args.FailureReason = "Failed to delete existing file: " + result.Error;
                    args.Logger?.ELog(args.FailureReason);
                    return -1;
                }
            }

            string compressionMethod = CompressionMethod?.EmptyAsNull() ?? "lzma2";

            string targetFile = args.IsRemote ? FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".7zip") : 
                args.FileService.GetLocalPath(destFile); // local but maybe mapped

            args.Execute(new()
            {
                Command = sevenZip,
                ArgumentList = new[]
                {
                    "a", "-t7z", $"-m0={compressionMethod}", 
                    $"-mx{compression}", 
                    targetFile, itemToCompress
                }
            });

            if (System.IO.File.Exists(targetFile) == false)
            {
                args.FailureReason = "Failed to create 7z: " + destFile; 
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }

            if (targetFile != destFile)
            {
                // need to move the file 
                var result = args.FileService.FileMove(targetFile, destFile, true);
                if (result.IsFailed)
                {
                    args.FailureReason = "Failed to move 7z file: " + result.Error;
                    args.Logger?.ELog(args.FailureReason);
                    return -1;
                }
            }
            
            args.SetWorkingFile(destFile);
            args.Logger?.ILog("7z created at: " + destFile);
            return 1;
        }
        catch (Exception ex)
        {
            args.FailureReason = "Failed creating 7z: " + ex.Message;
            args.Logger?.ELog(args.FailureReason + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}
