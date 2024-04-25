namespace FileFlows.AudioNodes;

/// <summary>
/// Flow element that embeds artwork
/// </summary>
public class EmbedArtwork : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-image";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/embed-artwork";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string ffmpeg = args.GetToolPath("FFMpeg");
        if (string.IsNullOrEmpty(ffmpeg))
        {
            args.FailureReason = "FFmpeg tool not found.";
            args.Logger.ELog(args.FailureReason);
            return -1;
        }

        
        var artwork = FindArtwork(args, args.WorkingFile);
        if (string.IsNullOrWhiteSpace(artwork))
            artwork = FindArtwork(args, args.LibraryFileName);

        if (string.IsNullOrWhiteSpace(artwork))
        {
            args.Logger?.ILog("No artwork found");
            return 2;
        }

        if (Regex.IsMatch(artwork, @"\.(jpeg|jpg|jpe)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ==
            false)
        {
            args.Logger?.ILog("Converting artwork to JPG");
            var jpg = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".jpg");
            if (args.ImageHelper.ConvertToJpeg(artwork, jpg).Failed(out string error))
            {
                args.Logger?.ILog("Failed to convert artwork: " + error);
                return 2;
            }
            artwork = jpg;
        }

        return DoEmbedding(args, ffmpeg, artwork);
    }

    /// <summary>
    /// Actually does the embedding of the artwork
    /// </summary>
    /// <param name="args">the node paratmers</param>
    /// <param name="ffmpeg">the ffmpeg executable</param>
    /// <param name="image">the image to embed</param>
    /// <returns>the output to call next</returns>
    private static int DoEmbedding(NodeParameters args, string ffmpeg, string image)
    {
        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.IsFailed)
        {
            args.FailureReason = "Failed to get local file: " + localFile.Error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        // ffmpeg -i input.m4a -i image.jpg -map 0 -map 1 -c copy -disposition:v:1 attached_pic output.m4a 
        // 
        string output = FileHelper.Combine(args.TempPath, Guid.NewGuid() + FileHelper.GetExtension(localFile));
        var result = args.Execute(new ExecuteArgs
        {
            Command = ffmpeg,
            ArgumentList = [
                "-hide_banner",
                "-y",
                "-i", localFile,
                "-i", image,
                "-c", "copy",
                "-disposition:v", "attached_pic",
                output
            ]
        });

        if(result.ExitCode != 0)
        {
            args.Logger?.ELog("Invalid exit code detected: " + result.ExitCode);
            return -1;
        }

        args.Logger?.ILog("New temporary file created with artwork: " + output);
        args.SetWorkingFile(output);
        return 1;
    }

    /// <summary>
    /// Finds the artwork for the file
    /// </summary>
    /// <param name="args">the node parametesr</param>
    /// <param name="filename">the filename to find the artwork for</param>
    /// <returns>the artwork</returns>
    private string FindArtwork(NodeParameters args, string filename)
    {
        var dir = FileHelper.GetDirectory(filename);
        var allFiles = args.FileService.GetFiles(dir, "*.*");
        if (allFiles.Failed(out string error))
        {
            args.Logger?.WLog("Failed to get files: " + error);
            return string.Empty;
        }
        
        var images = allFiles.Value.Where(x => Regex.IsMatch(x, @"\.(jpg|jpeg|jpe|gif|png|webp)$",
            RegexOptions.IgnoreCase)).ToArray();
        if (images.Length == 0)
        {
            args.Logger?.ILog("No images found in: " + dir);
            return string.Empty;
        }
        
        
        // Extract the file name without extension from the filename
        var fileNameWithoutExtension = FileHelper.GetShortFileName(filename).ToLowerInvariant();
        int index = fileNameWithoutExtension.LastIndexOf('.');
        if (index > 0)
            fileNameWithoutExtension = fileNameWithoutExtension[..index];


        var exact = images.FirstOrDefault(x =>
        { 
            var shortname = FileHelper.GetShortFileName(x).ToLowerInvariant();
            if (shortname.StartsWith(fileNameWithoutExtension) == false)
                return false;
            bool isLargeEnough = IsLargeEnough(args, x);
            return isLargeEnough;
        });
        
        if (string.IsNullOrEmpty(exact) == false)
        {
            args.Logger?.ILog("Found exact matching image: " + exact);
            return exact;
        }
        
        var cover = images
            .FirstOrDefault(x =>
            {
                var shortname = FileHelper.GetShortFileName(x).ToLowerInvariant();
                if (Regex.IsMatch(shortname, "^(poster|cover|front)") == false)
                    return false;
                return IsLargeEnough(args, x);
            });
        
        if (string.IsNullOrEmpty(cover) == false)
        {
            args.Logger?.ILog("Found cover image: " + cover);
            return cover;
        }
            
        return string.Empty;
    }

    /// <summary>
    /// Tets if an image is large enough to be used
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="file">the image file to test</param>
    /// <returns>true if large enough, otherwise false</returns>
    private static bool IsLargeEnough(NodeParameters args, string file)
    {
        if (args.ImageHelper == null)
            return true; // ImageHelper is null during unit testing 
        
        var result = args.ImageHelper.GetDimensions(file);
        if (result.Failed(out string error))
        {
            args.Logger?.WLog($"Failed getting dimensions for file '{file}': {error}");
            return false;
        }

        var (width, height) = result.Value;
        args.Logger?.ILog($"Image '{file}' dimensions: {width}x{height}");
        return width > 200 && height > 200;
    }
}