using System.ComponentModel;
using FileFlows.Plugin.Helpers;

namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Base image flow element
/// </summary>
public abstract class ImageNode : ImageBaseNode
{
    /// <summary>
    /// Gets or sets the format to save the image in
    /// </summary>
    [Select(nameof(FormatOptions), 51)]
    public string Format { get; set; } = string.Empty;
    
    private static List<ListOption>? _FormatOptions;
    
    /// <summary>
    /// Gets the image format options
    /// </summary>
    public static List<ListOption> FormatOptions
    {
        get
        {
            if (_FormatOptions == null)
            {
                _FormatOptions = new List<ListOption>
                {
                    new () { Value = "", Label = "Same as source"},
                    new () { Value = "###GROUP###", Label = "Lossless Formats" },
                    new () { Value = IMAGE_FORMAT_PNG, Label = "PNG" },
                    new () { Value = IMAGE_FORMAT_BMP, Label = "Bitmap" },
                    new () { Value = IMAGE_FORMAT_TIFF, Label = "TIFF" },
                    new () { Value = IMAGE_FORMAT_TGA, Label = "TGA" },
                    new () { Value = IMAGE_FORMAT_WEBP, Label = "WebP" },
                    new () { Value = "###GROUP###", Label = "Lossy Formats" },
                    new () { Value = IMAGE_FORMAT_JPEG, Label = "JPEG" },
                    new () { Value = IMAGE_FORMAT_GIF, Label = "GIF" },
                    new () { Value = IMAGE_FORMAT_PBM, Label = "PBM" },
                };
            }
            return _FormatOptions;
        }
    }
    
    /// <summary>
    /// Gets or sets the quality to save the image in
    /// </summary>
    [Slider(52)]
    [Range(1, 100)]
    [DefaultValue(100)]
    [ConditionEquals(nameof(Format), $"/^({IMAGE_FORMAT_WEBP}|{IMAGE_FORMAT_JPEG})$/")]
    public int Quality { get; set; }

    /// <summary>
    /// Gets the image type from the image format
    /// </summary>
    /// <returns>the image type, or null to keep original</returns>
    protected ImageType? GetImageTypeFromFormat()
    {
        switch (Format)
        {
            case IMAGE_FORMAT_BMP: return ImageType.Bmp;
            case IMAGE_FORMAT_GIF: return ImageType.Gif;
            case IMAGE_FORMAT_TGA: return ImageType.Tga;
            case IMAGE_FORMAT_PNG: return ImageType.Png;
            case IMAGE_FORMAT_PBM: return ImageType.Pbm;
            case IMAGE_FORMAT_JPEG: return ImageType.Jpeg;
            case IMAGE_FORMAT_TIFF: return ImageType.Tiff;
            case IMAGE_FORMAT_WEBP: return ImageType.Webp;
        }
        return null;
    }

    /// <summary>
    /// Gets the image type extension with leading period
    /// </summary>
    /// <param name="file">the current file, will be used if the type is not set</param>
    /// <returns>the extension with leading period</returns>
    protected string GetImageTypeExtension(string file)
    {
        var type = GetImageTypeFromFormat();
        if (type == ImageType.Jpeg)
            return ".jpg";
        if (type == null)
            return FileHelper.GetExtension(file);
        return "." + (type.ToString()!.ToLowerInvariant());
    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.Failed(out string error))
        {
            args.FailureReason = "Failed to get local file: " + localFile.Error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var destination = FileHelper.Combine(args.TempPath, Guid.NewGuid() + GetImageTypeExtension(localFile));

        var result = PerformAction(args, localFile, destination);
        
        if (result.Failed(out error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (result == false)
            return 2;

        args.SetWorkingFile(destination);
        ReadWorkingFileInfo(args);

        return 1;
    }

    /// <summary>
    /// Performs the image action
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="localFile">the local file</param>
    /// <param name="destination">the destination file to create</param>
    /// <returns>true if successful (output 1), false if not (output 2), failure result if failed</returns>
    protected abstract Result<bool> PerformAction(NodeParameters args, string localFile, string destination);
}