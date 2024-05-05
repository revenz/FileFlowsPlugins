using System.ComponentModel;
using FileFlows.Plugin.Helpers;

namespace FileFlows.ComicNodes.Comics;

/// <summary>
/// Convert comic books
/// </summary>
public class ComicConverter: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc /> 
    public override string Icon => "fas fa-book";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/comic-nodes/comic-converter";

    CancellationTokenSource cancellation = new CancellationTokenSource();

    /// <summary>
    /// Gets or sets the comic book format
    /// </summary>
    [DefaultValue("CBZ")]
    [Select(nameof(FormatOptions), 1)]
    public string Format { get; set; } = string.Empty;

    private static List<ListOption> _FormatOptions;
    /// <summary>
    /// Gets the format options
    /// </summary>
    public static List<ListOption> FormatOptions
    {
        get
        {
            if (_FormatOptions == null)
            {
                _FormatOptions = new List<ListOption>
                {
                    new() { Label = "CBZ", Value = "CBZ" },
                    //new ListOption { Label = "CB7", Value = "cb7"},
                    new() { Label = "PDF", Value = "PDF" }
                };
            }
            return _FormatOptions;
        }
    }

    /// <summary>
    /// Gets or sets if the archive should only have images in the top directgory
    /// </summary>
    [Boolean(2)]
    [ConditionEquals(nameof(Format), "PDF", inverse:true)]
    public bool EnsureTopDirectory { get; set; }

    /// <summary>
    /// Gets or sets the codec the images will be saved in
    /// </summary>
    [DefaultValue("")]
    [Select(nameof(CodecOptions), 3)]
    public string Codec { get; set; } = string.Empty;

    private static List<ListOption> _CodecOptions;
    /// <summary>
    /// Gets the format options
    /// </summary>
    public static List<ListOption> CodecOptions
    {
        get
        {
            if (_CodecOptions == null)
            {
                _CodecOptions = new List<ListOption>
                {
                    new() { Label = "Same as source", Value = "" },
                    new() { Label = "JPEG", Value = "jpeg" },
                    new() { Label = "WEBP", Value = "webp" }
                };
            }
            return _CodecOptions;
        }
    }
    
    /// <summary>
    /// Gets or sets the quality
    /// </summary>
    [Range(0, 100)]
    [Slider(4)]
    [DefaultValue(75)]
    [ConditionEquals(nameof(Codec), "", true)]
    public int Quality { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum width of images
    /// </summary>
    [NumberInt(5)]
    [ConditionEquals(nameof(Codec), "", true)]
    public int MaxWidth { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum height of images
    /// </summary>
    [NumberInt(6)]
    [ConditionEquals(nameof(Codec), "", true)]
    public int MaxHeight { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string currentFormat = new FileInfo(args.WorkingFile).Extension;
        if (string.IsNullOrEmpty(currentFormat))
        {
            args.Logger?.ELog("Could not detect format for: " + args.WorkingFile);
            return -1;
        }
        Format = Format?.ToUpper() ?? string.Empty;

        if (currentFormat[0] == '.')
            currentFormat = currentFormat[1..]; // remove the dot
        currentFormat = currentFormat.ToUpper();


        var metadata = new Dictionary<string, object>();
        int pageCount = GetPageCount(args, currentFormat, args.WorkingFile);
        metadata.Add("Format", currentFormat);
        metadata.Add("Pages", pageCount);
        args.RecordStatisticRunningTotals("COMIC_FORMAT", currentFormat);
        args.RecordStatisticAverage("COMIC_PAGES", pageCount);
        args.SetMetadata(metadata);
        args.Logger?.ILog("Setting metadata: " + currentFormat);

        if (currentFormat == Format && string.IsNullOrWhiteSpace(Codec) &&
            (currentFormat.ToLowerInvariant() == "pdf" || EnsureTopDirectory == false))
        {
            args.Logger?.ILog($"Already in the target format of '{Format}'");
            return 2;
        }

        string destinationPath = Path.Combine(args.TempPath, Guid.NewGuid().ToString());
        
        Directory.CreateDirectory(destinationPath);
        if (Helpers.ComicExtractor.Extract(args, args.WorkingFile, destinationPath, halfProgress: true, cancellation: cancellation.Token) == false)
            return -1;

        if (EnsureTopDirectory)
        {
            args.Logger?.ILog("Ensuring top directory");
            MoveSubFilesToRootDirectory(destinationPath);
        }

        if (Codec.ToLowerInvariant() is "webp" or "jpeg")
        {
            args.Logger?.ILog("Converting images to: " + Codec);
            var rgxImages = new Regex(@"\.(jpeg|jpg|jpe|png|bmp|tiff|webp|gif)$", RegexOptions.IgnoreCase);
            var files = Directory.GetFiles(destinationPath, "*.*", SearchOption.AllDirectories);
            ImageOptions imageOptions = new()
            {
                Quality = Quality,
                MaxWidth = MaxWidth,
                MaxHeight = MaxHeight
            };
            args.Logger?.ILog("Quality: " + Quality);
            args.Logger?.ILog("MaxWidth: " + MaxWidth);
            args.Logger?.ILog("MaxHeight: " + MaxHeight);
            args.PartPercentageUpdate?.Invoke(0);
            int count = 0;
            foreach (string file in files)
            {
                if (cancellation.IsCancellationRequested)
                    break;
                try
                {
                    if (File.Exists(file) == false)
                        continue; // may have been replaced

                    if (file.ToLowerInvariant().EndsWith(".pdf"))
                    {
                        args.Logger?.ILog("Deleting: " + file);
                        File.Delete(file);
                        continue;
                    }

                    if (rgxImages.IsMatch(file))
                    {
                        args.Logger?.ILog("Converting image: " + file);
                        string dest;
                        if (Codec.ToLowerInvariant() == "webp")
                        {
                            dest = Path.ChangeExtension(file, "webp");
                            args.ImageHelper.ConvertToWebp(file, dest, imageOptions);
                        }
                        else
                        {
                            dest = Path.ChangeExtension(file, "jpg");
                            args.ImageHelper.ConvertToJpeg(file, dest, imageOptions);
                        }

                        if (File.Exists(dest) && File.Exists(file) && file != dest)
                            File.Delete(file);
                    }
                }
                finally
                {
                    float total = files.Length;
                    args.PartPercentageUpdate?.Invoke((count++ / total) * 100);
                }
            }
        }

        if (cancellation.IsCancellationRequested)
            return -1;
        
        string newFile = CreateComic(args, destinationPath, this.Format);

        args.SetWorkingFile(newFile);   

        return 1;
    }

    /// <summary>
    /// Cancels the conversion
    /// </summary>
    /// <returns>the task to await</returns>
    public override Task Cancel()
    {
        cancellation.Cancel();
        return base.Cancel();
    }

    /// <summary>
    /// Gets the total number of pages
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="format">the format</param>
    /// <param name="workingFile">the working file to get the page count for</param>
    /// <returns>the number of pages</returns>
    private int GetPageCount(NodeParameters args, string format, string workingFile)
    {
        if (format == null)
            return 0;
        format = format.ToUpper().Trim();
        switch (format)
        {
            case "PDF":
                return Helpers.PdfHelper.GetPageCount(workingFile);
            default:
                return Helpers.GenericExtractor.GetImageCount(args, workingFile);
        }
    }

    /// <summary>
    /// Creates the comic
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="directory">the directory to create the comic out of</param>
    /// <param name="format">the format to create the comic</param>
    /// <returns>the path to the newly created comic</returns>
    /// <exception cref="Exception">if the format is not supported</exception>
    private string CreateComic(NodeParameters args, string directory, string format)
    {
        string file = Path.Combine(args.TempPath, Guid.NewGuid() + "." + format.ToLower());
        args.Logger?.ILog("Creating comic: " + file);
        if (format == "CBZ")
            Helpers.ZipHelper.Compress(args, directory, file);
        //else if (format == "CB7")
        //    Helpers.SevenZipHelper.Compress(args, directory, file + ".7z");
        else if (format == "PDF")
            Helpers.PdfHelper.Create(args, directory, file);
        else
            throw new Exception("Unknown format:" + format);
        Directory.Delete(directory, true);
        args.Logger?.ILog("Created comic: " + file);
        args.Logger?.ILog("Deleted temporary extraction directory: " + directory);


        var metadata = new Dictionary<string, object>();
        metadata.Add("Format", format);
        metadata.Add("Pages", GetPageCount(args, format, file));
        args.SetMetadata(metadata);
        args.Logger?.ILog("Setting metadata: " + format);

        return file;
    }
    
    /// <summary>
    /// Moves all files from subdirectories of the specified directory to the root directory,
    /// overwriting duplicates, and deletes empty subdirectories.
    /// </summary>
    /// <param name="path">The directory containing subdirectories with files to be moved.</param>
    static void MoveSubFilesToRootDirectory(string path)
    {
        if (Directory.Exists(path) == false)
            return;

        foreach (var subDirectory in Directory.GetDirectories(path))
        {
            foreach (var filePath in Directory.GetFiles(subDirectory))
            {
                var fileName = Path.GetFileName(filePath);
                var destinationFilePath = Path.Combine(path, fileName);
                File.Copy(filePath, destinationFilePath, true);
            }
        }

        foreach (string subDirectory in Directory.GetDirectories(path))
        {
            Directory.Delete(subDirectory, true);
        }

    }
}
