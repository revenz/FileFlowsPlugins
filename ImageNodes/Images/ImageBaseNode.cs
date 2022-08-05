using SixLabors.ImageSharp.Formats;

namespace FileFlows.ImageNodes.Images;

public abstract class ImageBaseNode:Node
{

    private const string IMAGE_INFO = "ImageInfo";
    protected void UpdateImageInfo(NodeParameters args, Dictionary<string, object> variables = null)
    {
        using var image = Image.Load(args.WorkingFile, out IImageFormat format);
        var imageInfo = new ImageInfo
        {
            Width = image.Width,
            Height = image.Height,
            Format = format.Name
        };

        var metadata = new Dictionary<string, object>();
        metadata.Add("Format", imageInfo.Format);
        metadata.Add("Width", imageInfo.Width);
        metadata.Add("Height", imageInfo.Height);
        args.SetMetadata(metadata);

        variables ??= new Dictionary<string, object>();
        if (args.Parameters.ContainsKey(IMAGE_INFO))
            args.Parameters[IMAGE_INFO] = imageInfo;
        else
            args.Parameters.Add(IMAGE_INFO, imageInfo);        

        variables.AddOrUpdate("img.Width", imageInfo.Width);
        variables.AddOrUpdate("img.Height", imageInfo.Height);
        variables.AddOrUpdate("img.Format", imageInfo.Format);
        variables.AddOrUpdate("img.IsPortrait", imageInfo.IsPortrait);
        variables.AddOrUpdate("img.IsLandscape", imageInfo.IsLandscape);

        args.UpdateVariables(variables);
    }
    protected void UpdateImageInfo(NodeParameters args, Image image, IImageFormat format, Dictionary<string, object> variables = null)
    {
        var imageInfo = new ImageInfo
        {
            Width = image.Width,
            Height = image.Height,
            Format = format.Name
        };

        variables ??= new Dictionary<string, object>();
        if (args.Parameters.ContainsKey(IMAGE_INFO))
            args.Parameters[IMAGE_INFO] = imageInfo;
        else
            args.Parameters.Add(IMAGE_INFO, imageInfo);

        variables.AddOrUpdate("img.Width", imageInfo.Width);
        variables.AddOrUpdate("img.Height", imageInfo.Height);
        variables.AddOrUpdate("img.Format", imageInfo.Format);
        variables.AddOrUpdate("img.IsPortrait", imageInfo.IsPortrait);
        variables.AddOrUpdate("img.IsLandscape", imageInfo.IsLandscape);

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
}
