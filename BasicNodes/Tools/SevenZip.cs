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
    public override string Icon => "fas fa-file-archive";
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


    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        bool isDir = false;

        try
        {
            var localResult = args.FileService.GetLocalPath(args.WorkingFile);
            if (localResult.IsFailed)
            {
                args.Logger?.ELog("Failed to ensure file is local: " + localResult.Error);
                return -1;
            }

            string itemToCompress = localResult.Value;
            if (Directory.Exists(itemToCompress))
            {
                if (args.IsRemote)
                {
                    args.Logger?.ELog("Cannot 7-zip a remote folder");
                    return -1;
                }
                isDir = true;
                itemToCompress = FileHelper.Combine(itemToCompress, "*");
            }
            
            string sevenZip = args.GetToolPath("7zip")?.EmptyAsNull() ?? "7z";

            string destDir = DestinationPath;
            if (string.IsNullOrEmpty(destDir))
            {
                args.Logger?.ILog("No destination path set");
                if (isDir)
                    destDir = new DirectoryInfo(args.LibraryPath).FullName;
                else
                    destDir = FileHelper.GetDirectory(args.FileName);
                
                if (string.IsNullOrEmpty(destDir))
                {
                    args.Logger?.ELog("Failed to get destination directory");
                    return -1;
                }
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
                {
                    args.Logger?.ELog("Failed creating directory: " + result.Error);
                    return -1;
                }
            }
            args.Logger?.ILog("Destination Folder: " + destDir);

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
                    args.Logger?.ILog("Failed to delete existing file: " + result.Error);
                    return -1;
                }
            }

            string compressionMethod = CompressionMethod?.EmptyAsNull() ?? "lzma2";

            string targetFile = args.IsRemote ? FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".7zip") : destFile;

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
                args.Logger?.ELog("Failed to create 7z: " + destFile);
                return -1;
            }

            if (targetFile != destFile)
            {
                // need to move the file 
                var result = args.FileService.FileMove(targetFile, destFile, true);
                if (result.IsFailed)
                {
                    args.Logger?.ELog("Failed to move 7z file: " + result.Error);
                    return -1;
                }
            }
            
            args.SetWorkingFile(destFile);
            args.Logger?.ILog("7z created at: " + destFile);
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed creating 7z: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}
