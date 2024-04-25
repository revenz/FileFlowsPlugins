using ImageMagick;
using SixLabors.ImageSharp.Formats;

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
    protected string CurrentFormat { get; private set; }

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
        if (args.WorkingFile.ToLowerInvariant().EndsWith(".heic"))
        {
            using var image = new MagickImage(localFile);
            CurrentHeight = image.Height;
            CurrentWidth = image.Width;
            CurrentFormat = "HEIC";
        }
        else
        {
            
            var format = Image.DetectFormat(localFile);
            using var image = Image.Load(localFile);
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
    
    /// <summary>
    /// Updates information about an image based on the provided NodeParameters and optional variables.
    /// </summary>
    /// <param name="args">The NodeParameters</param>
    /// <param name="variables">Additional variables associated with the image (optional).</param>
    protected void UpdateImageInfo(NodeParameters args, Dictionary<string, object> variables = null)
    {
        string extension = FileHelper.GetExtension(args.WorkingFile).ToLowerInvariant().TrimStart('.');
        if (extension == "heic")
        {
            using var image = new MagickImage(args.WorkingFile);
            UpdateImageInfo(args, image.Width, image.Height, "HEIC", variables);
        }
        else
        {
            var format = Image.DetectFormat(args.WorkingFile);
            using var image = Image.Load(args.WorkingFile);
            DateTime? dateTaken = null;
            if (image.Metadata.ExifProfile != null)
            {
                args.Logger?.ILog("EXIF Profile found");
                if(image.Metadata.ExifProfile.TryGetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeOriginal, out var dateTimeOriginalString) == false
                   || string.IsNullOrWhiteSpace(dateTimeOriginalString?.Value))
                {
                    args.Logger?.ILog("No DateTimeOriginal found");
                }
                else
                {
                    if (TryParseDateTime(dateTimeOriginalString.Value, out DateTime? dateTimeOriginal))
                    {
                        dateTaken = dateTimeOriginal;
                        args.Logger?.ILog("DateTimeOriginal: " + dateTimeOriginal);
                    }
                    else
                    {
                        args.Logger?.ILog("Invalid date format for DateTimeOriginal: " + dateTimeOriginalString.Value);
                    }
                }
            }
            else
            {
                args.Logger?.ILog("No EXIF Profile found");
            }
            
            UpdateImageInfo(args, image.Width, image.Height, format.Name, variables: variables, dateTaken: dateTaken);
        }
    }
    
    /// <summary>
    /// Updates information about an image based on the provided NodeParameters, width, height, format, variables, and dateTaken.
    /// </summary>
    /// <param name="args">The NodeParameters</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="format">The format of the image.</param>
    /// <param name="variables">Additional variables associated with the image (optional).</param>
    /// <param name="dateTaken">The date when the image was taken (optional).</param>
    protected void UpdateImageInfo(NodeParameters args, int width, int height, string format, Dictionary<string, object> variables = null, DateTime? dateTaken = null)
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

        if (dateTaken != null)
        {
            variables.AddOrUpdate("img.DateTaken", dateTaken.Value);
        }

        var metadata = new Dictionary<string, object>();
        metadata.Add("Format", imageInfo.Format);
        metadata.Add("Width", imageInfo.Width);
        metadata.Add("Height", imageInfo.Height);
        args.SetMetadata(metadata);

        args.UpdateVariables(variables);
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
        if (args.Parameters.ContainsKey(IMAGE_INFO) == false)
        {
            args.Logger?.WLog("No image information loaded, use a 'Image File' flow element first");
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
        string extension = FileHelper.GetExtension(args.WorkingFile).ToLowerInvariant().TrimStart('.');
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
    
    /// <summary>
    /// Tries to parse a DateTime from a string, attempting different formats.
    /// </summary>
    /// <param name="dateTimeString">The string representation of the DateTime.</param>
    /// <param name="dateTime">When this method returns, contains the parsed DateTime if successful; otherwise, null.</param>
    /// <returns>
    /// True if the parsing was successful; otherwise, false.
    /// </returns>
    static bool TryParseDateTime(string dateTimeString, out DateTime? dateTime)
    {
        DateTime parsedDateTime;

        // Try parsing using DateTime.TryParse
        if (DateTime.TryParse(dateTimeString, out parsedDateTime))
        {
            dateTime = parsedDateTime;
            return true;
        }

        // Define an array of possible date formats for additional attempts
        string[] dateFormats = { "yyyy:MM:dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss" /* Add more formats if needed */ };

        // Attempt to parse using different formats
        foreach (var format in dateFormats)
        {
            if (DateTime.TryParseExact(dateTimeString, format, null, System.Globalization.DateTimeStyles.None, out parsedDateTime))
            {
                dateTime = parsedDateTime;
                return true;
            }
        }

        // Set dateTime to null if parsing fails with all formats
        dateTime = null;
        return false;
    }

}
