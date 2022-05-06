using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;

namespace FileFlows.ImageNodes.Images;

public abstract class ImageNode : ImageBaseNode
{
    [Select(nameof(FormatOptions), 1)]
    public string Format { get; set; }
    
    private static List<ListOption> _FormatOptions;
    public static List<ListOption> FormatOptions
    {
        get
        {
            if (_FormatOptions == null)
            {
                _FormatOptions = new List<ListOption>
                {
                    new ListOption { Value = "", Label = "Same as source"},
                    new ListOption { Value = IMAGE_FORMAT_BMP, Label = "Bitmap"},
                    new ListOption { Value = IMAGE_FORMAT_GIF, Label = "GIF"},
                    new ListOption { Value = IMAGE_FORMAT_JPEG, Label = "JPEG"},
                    new ListOption { Value = IMAGE_FORMAT_PBM, Label = "PBM"},
                    new ListOption { Value = IMAGE_FORMAT_PNG, Label = "PNG"},
                    new ListOption { Value = IMAGE_FORMAT_TIFF, Label = "TIFF"},
                    new ListOption { Value = IMAGE_FORMAT_TGA, Label = "TGA" },
                    new ListOption { Value = IMAGE_FORMAT_WEBP, Label = "WebP"},
                };
            }
            return _FormatOptions;
        }
    }

    protected (IImageFormat? format, string file) GetFormat(NodeParameters args)
    {
        IImageFormat? format = null;
        
        var newFile = Path.Combine(args.TempPath, Guid.NewGuid().ToString());
        switch (this.Format)
        {
            case IMAGE_FORMAT_BMP:
                newFile = newFile + ".bmp";
                format = BmpFormat.Instance;
                break; 
            case IMAGE_FORMAT_GIF:
                newFile = newFile + ".gif";
                format = GifFormat.Instance;
                break; 
            case IMAGE_FORMAT_JPEG:
                newFile = newFile + ".jpg";
                format = JpegFormat.Instance;
                break;
            case IMAGE_FORMAT_PBM:
                newFile = newFile + ".pbm";
                format = PbmFormat.Instance;
                break; 
            case IMAGE_FORMAT_PNG:
                newFile = newFile + ".png";
                format = PngFormat.Instance;
                break; 
            case IMAGE_FORMAT_TIFF:
                newFile = newFile + ".tiff";
                format = TiffFormat.Instance;
                break;
            case IMAGE_FORMAT_TGA:
                newFile = newFile + ".tga";
                format = TgaFormat.Instance;
                break; 
            case IMAGE_FORMAT_WEBP:
                newFile = newFile + ".webp";
                format = WebpFormat.Instance;
                break;
            default:
                newFile = newFile + "." + args.WorkingFile.Substring(args.WorkingFile.LastIndexOf(".") + 1);
                break;
        }

        return (format, newFile);
    }

    protected void SaveImage(NodeParameters args, Image img, string file, IImageFormat format, bool updateWorkingFile = true)
    {
        using Stream outStream = new FileStream(file, FileMode.Create);
        img.Save(outStream, format);
        if (updateWorkingFile)
        {
            args.SetWorkingFile(file);
            UpdateImageInfo(args, img, format, Variables);
        }
    }
}