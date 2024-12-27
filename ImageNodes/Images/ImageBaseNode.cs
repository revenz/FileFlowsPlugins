using FileFlows.Plugin.Helpers;

namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Represents an abstract base class for nodes related to image processing.
/// </summary>
public abstract class ImageBaseNode:Node
{
    /// <summary>
    /// Represents the key for storing image information in a context or dictionary.
    /// </summary>
    private const string IMAGE_INFO = "ImageInfo";

    /// <summary>
    /// Gets or sets the current format of the image.
    /// </summary>
    protected string? CurrentFormat { get; private set; }

    /// <summary>
    /// Gets or sets the current width of the image.
    /// </summary>
    protected int CurrentWidth { get; private set; }

    /// <summary>
    /// Gets or sets the current height of the image.
    /// </summary>
    protected int CurrentHeight { get; private set; }


    /// <summary>
    /// Calls any pre-execute setup code
    /// </summary>
    /// <param name="args">The NodeParameters</param>
    /// <returns>true if successful, otherwise false</returns>
    public override bool PreExecute(NodeParameters args)
    {
        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.IsFailed)
        {
            args.Logger?.ELog("Working file cannot be read: " + localFile.Error);
            return false;
        }

        var info = args.ImageHelper.GetInfo(localFile);
        if(info.Failed(out string error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return false;
        }
        var metadata = new Dictionary<string, object>();
        if(string.IsNullOrWhiteSpace(info.Value.Format) == false)
            metadata.Add("Format", info.Value.Format);
        metadata.Add("Width", info.Value.Width);
        metadata.Add("Height", info.Value.Height);
        
        CurrentFormat = info.Value.Format;
        CurrentHeight = info.Value.Height;
        CurrentWidth = info.Value.Width;
        
        args.SetMetadata(metadata);
        UpdateImageInfo(args, info);

        return true;
    }

    /// <summary>
    /// Updates information about an image based on the provided NodeParameters and optional variables.
    /// </summary>
    /// <param name="args">The NodeParameters</param>
    protected void ReadWorkingFileInfo(NodeParameters args)
    {
        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.Failed(out string error))
        {
            args.Logger?.ELog("Working file cannot be read: " + error);
            return;
        }

        var info = args.ImageHelper.GetInfo(localFile);
        if (info.Failed(out error))
        {
            args.Logger?.ELog(error);
            return;
        }

        UpdateImageInfo(args, info);
    }

    /// <summary>
    /// Updates information about an image based on the provided NodeParameters, width, height, format, variables, and dateTaken.
    /// </summary>
    /// <param name="args">The NodeParameters</param>
    /// <param name="imageInfo">The image info</param>
    protected void UpdateImageInfo(NodeParameters args, ImageInfo imageInfo)
    {
        args.Parameters[IMAGE_INFO] = imageInfo;

        var metadata = new Dictionary<string, object>();
        args.Variables["img.Width"] = imageInfo.Width;
        args.Variables["img.Height"] = imageInfo.Height;
        metadata.Add("Width", imageInfo.Width);
        metadata.Add("Height", imageInfo.Height);
        if (string.IsNullOrWhiteSpace(imageInfo.Format))
        {
            args.Variables.Remove("img.Format");
        }
        else
        {
            args.Variables["img.Format"] = imageInfo.Format;
            metadata["img.Format"] = imageInfo.Format;
        }

        args.Variables["img.IsPortrait"] = imageInfo.IsPortrait;
        args.Variables["img.IsLandscape"] = imageInfo.IsLandscape;

        if (imageInfo.DateTaken != null)
        {
            args.Variables["img.DateTaken"] = imageInfo.DateTaken.Value;
            args.Variables["img.DateYear"] = imageInfo.DateTaken.Value.Year;
            args.Variables["img.DateMonth"] = imageInfo.DateTaken.Value.Year;
        }
        else
            args.Variables.Remove("img.DateTaken");
        
        args.Logger?.ILog("About to set image mime/type: " + imageInfo.Type + " = " + imageInfo.Format);
        switch (imageInfo.Type)
        {
            case ImageType.Bmp:
                args.SetMimeType("image/bmp");
                break;
            case ImageType.Jpeg:
                args.SetMimeType("image/jpeg");
                break;
            case ImageType.Png:
                args.SetMimeType("image/png");
                break;
            case ImageType.Gif:
                args.SetMimeType("image/gif");
                break;
            case ImageType.Tiff:
                args.SetMimeType("image/tiff");
                break;
            case ImageType.Webp:
                args.SetMimeType("image/webp");
                break;
            case ImageType.Pbm:
                args.SetMimeType("image/pbm");
                break;
            case ImageType.Tga:
                args.SetMimeType("image/tga");
                break;
            case ImageType.Heic:
                args.SetMimeType("image/heic");
                break;
            default:
            {
                switch (imageInfo.Format?.ToLowerInvariant())
                {
                    case "bmp":
                        args.SetMimeType("image/bmp");
                        break;
                    case "jpeg":
                    case "jpg":
                    case "jpe":
                        args.SetMimeType("image/jpeg");
                        break;
                    case "png":
                        args.SetMimeType("image/png");
                        break;
                    case "gif":
                        args.SetMimeType("image/gif");
                        break;
                    case "tiff":
                        args.SetMimeType("image/tiff");
                        break;
                    case "webp":
                        args.SetMimeType("image/webp");
                        break;
                    case "pbm":
                        args.SetMimeType("image/pbm");
                        break;
                    case "tga":
                        args.SetMimeType("image/tga");
                        break;
                    case "heic":
                        args.SetMimeType("image/heic");
                        break;
                    default:
                        if(string.IsNullOrWhiteSpace(imageInfo.Format) == false)
                            args.SetMimeType("image/" + imageInfo.Format.ToLowerInvariant());
                        break;
                }
                break;
            }
        }
        
        args.SetMetadata(metadata);
    }
    
    /// <summary>
    /// Gets information about an image based on the provided NodeParameters.
    /// </summary>
    /// <param name="args">The NodeParameters</param>
    /// <returns>
    /// An ImageInfo object representing information about the image, or null if information could not be retrieved.
    /// </returns>
    internal ImageInfo? GetImageInfo(NodeParameters args)
    {
        if (args.Parameters.TryGetValue(IMAGE_INFO, out var parameter) == false)
        {
            args.Logger?.WLog("No image information loaded, use a 'Image File' flow element first");
            return null;
        }
        var result = parameter as ImageInfo;
        if (result == null)
        {
            args.Logger?.WLog("ImageInfo not found for file");
            return null;
        }
        return result;
    }


}
