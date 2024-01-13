using ImageMagick;
using SixLabors.ImageSharp.Formats;

namespace FileFlows.ImageNodes.Images;

public abstract class ImageBaseNode:Node
{

    private const string IMAGE_INFO = "ImageInfo";
    protected string CurrentFormat { get; private set; }
    protected int CurrentWidth{ get; private set; }
    protected int CurrentHeight { get; private set; }

    public override bool PreExecute(NodeParameters args)
    {
        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.IsFailed)
        {
            args.Logger?.ELog("Working file cannot be read: " + localFile.Error);
            return false;
        }
        if (args.WorkingFile.ToLowerInvariant().EndsWith(".heic"))
        {
            using var image = new MagickImage(localFile);
            CurrentHeight = image.Height;
            CurrentWidth = image.Width;
            CurrentFormat = "HEIC";
        }
        else
        {
            using var image = Image.Load(localFile, out IImageFormat format);
            CurrentHeight = image.Height;
            CurrentWidth = image.Width;
            CurrentFormat = format.Name;
        }
        var metadata = new Dictionary<string, object>();
        metadata.Add("Format", CurrentFormat);
        metadata.Add("Width", CurrentWidth);
        metadata.Add("Height", CurrentHeight);
        args.SetMetadata(metadata);

        return true;
    }

    protected void UpdateImageInfo(NodeParameters args, Dictionary<string, object> variables = null)
    {
        string extension = FileHelper.GetExtension(args.WorkingFile).ToLowerInvariant();
        if (extension == "heic")
        {
            using var image = new MagickImage(args.WorkingFile);
            UpdateImageInfo(args, image.Width, image.Height, "HEIC", variables);
        }
        else
        {
            using var image = Image.Load(args.WorkingFile, out IImageFormat format);
            UpdateImageInfo(args, image.Width, image.Height, format.Name, variables);
        }
    }
    protected void UpdateImageInfo(NodeParameters args, int width, int height, string format, Dictionary<string, object> variables = null)
    {
        var imageInfo = new ImageInfo
        {
            Width = width,
            Height = height,
            Format = format
        };

        variables ??= new Dictionary<string, object>();
        args.Parameters[IMAGE_INFO] = imageInfo;

        variables.AddOrUpdate("img.Width", imageInfo.Width);
        variables.AddOrUpdate("img.Height", imageInfo.Height);
        variables.AddOrUpdate("img.Format", imageInfo.Format);
        variables.AddOrUpdate("img.IsPortrait", imageInfo.IsPortrait);
        variables.AddOrUpdate("img.IsLandscape", imageInfo.IsLandscape);

        var metadata = new Dictionary<string, object>();
        metadata.Add("Format", imageInfo.Format);
        metadata.Add("Width", imageInfo.Width);
        metadata.Add("Height", imageInfo.Height);
        args.SetMetadata(metadata);

        args.UpdateVariables(variables);
    }
    internal ImageInfo? GetImageInfo(NodeParameters args)
    {
        if (args.Parameters.ContainsKey(IMAGE_INFO) == false)
        {
            args.Logger?.WLog("No image information loaded, use a 'Image File' node first");
            return null;
        }
        var result = args.Parameters[IMAGE_INFO] as ImageInfo;
        if (result == null)
        {
            args.Logger?.WLog("ImageInfo not found for file");
            return null;
        }
        return result;
    }

    /// <summary>
    /// Converts an image to a format we can use, if needed
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the filename fo the image to use</returns>
    protected string ConvertImageIfNeeded(NodeParameters args)
    {
        string extension = FileHelper.GetExtension(args.WorkingFile).ToLowerInvariant();
        if (extension == "heic")
        {
            // special case have to use imagemagick
            
            using var image = new MagickImage(args.WorkingFile);
            image.Format = MagickFormat.Png;
            var newFile = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".png");
            image.Write(newFile);
            return newFile;
        }

        return args.WorkingFile;
    }
}
