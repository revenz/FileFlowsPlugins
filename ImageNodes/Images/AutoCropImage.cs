using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;
using ImageMagick;

namespace FileFlows.ImageNodes.Images;

public class AutoCropImage : ImageNode
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Process;
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/auto-crop-image";
    public override string Icon => "fas fa-crop";

    [Slider(1)]
    [Range(1, 100)]
    [DefaultValue(50)]
    public int Threshold { get; set; }

    public override int Execute(NodeParameters args)
        => ExecuteImageMagick(args);

    private int ExecuteImageMagick(NodeParameters args)
    {
        using MagickImage image = new MagickImage(args.WorkingFile);
        (int originalWidth, int originalHeight) = (image.Width, image.Height);
        
        // image magick threshold is reversed, 100 means dont trim much, 1 means trim a lot
        image.Trim(new Percentage(100 - Threshold));

        if (image.Width == originalWidth && image.Height == originalHeight)
            return 2;
        
        var formatOpts = GetFormat(args);
        SaveImage(args, image, formatOpts.file, updateWorkingFile:true);
        args.Logger?.ILog($"Image cropped from '{originalWidth}x{originalHeight}' to '{image.Width}x{image.Height}'");

        return 1;
    }

    private int ExecuteImageSharp(NodeParameters args)
    {
        var format = Image.DetectFormat(args.WorkingFile);
        using var image = Image.Load(args.WorkingFile);
        int originalWidth = image.Width;
        int originalHeight= image.Height;
        float threshold = Threshold / 100f;
        if (threshold < 0)
            threshold = 0.5f;

        var scaleFactor = originalWidth > 4000 && originalHeight > 4000 ? 10 :
                          originalWidth > 2000 && originalHeight > 2000 ? 6 :
                          4;

        // entropycrop of Sixlabors does not give good results if the image has artifacts in it, eg from a scanned in image
        // so we need to detect the whitespace ourselves.   first we convert to greyscale and downscale, this should remove some artifacts for us
        // when then look for the outer most non white pixels for out bounds
        // then we upscale those bounds to the original dimensions and crop with those bounds
        image.Mutate(c =>
        {
            c.Grayscale().Resize(originalWidth / scaleFactor, originalHeight / scaleFactor);
        });
        string temp = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".jpg");
        image.SaveAsJpeg(temp);
        var bounds = GetTrimBounds(temp);
        bounds.X *= scaleFactor;
        bounds.Y *= scaleFactor;
        bounds.Width *= scaleFactor;
        bounds.Height *= scaleFactor;
        image.Dispose();

        args.Logger?.ILog("Attempting to auto crop using threshold: " + threshold);
        if (bounds.Width == originalWidth && bounds.Height == originalHeight)
            return 2;

        format = Image.DetectFormat(args.WorkingFile);
        using var image2 = Image.Load(args.WorkingFile);

        image2.Mutate(c =>
        {
            c.Crop(bounds);
            //c.EntropyCrop(threshold);
        });

        if (image2.Width == originalWidth && image2.Height == originalHeight)
            return 2;

        var formatOpts = GetFormat(args);
        SaveImage(args, image2, formatOpts.file, formatOpts.format ?? format);
        args.Logger?.ILog($"Image cropped from '{originalWidth}x{originalHeight}' to '{image2.Width}x{image2.Height}'");

        return 1;
    }

    public Rectangle GetTrimBounds(string file)
    {
        int threshhold = 255 - (this.Threshold / 4);


        int topOffset = 0;
        int bottomOffset = 0;
        int leftOffset = 0;
        int rightOffset = 0;


        using Image<Rgba32> image = Image.Load<Rgba32>(file);
        bool foundColor = false;
        // Get left bounds to crop
        for (int x = 1; x < image.Width && foundColor == false; x++)
        {
            for (int y = 1; y < image.Height && foundColor == false; y++)
            {
                var color = image[x, y];
                if (color.R < threshhold || color.G < threshhold || color.B < threshhold)
                    foundColor = true;
            }
            leftOffset += 1;
        }


        foundColor = false;
        // Get top bounds to crop
        for (int y = 1; y < image.Height && foundColor == false; y++)
        {
            for (int x = 1; x < image.Width && foundColor == false; x++)
            {
                var color = image[x, y];
                if (color.R < threshhold || color.G < threshhold || color.B < threshhold)
                    foundColor = true;
            }
            topOffset += 1;
        }


        foundColor = false;
        // Get right bounds to crop
        for (int x = image.Width - 1; x >= 1 && foundColor == false; x--)
        {
            for (int y = 1; y < image.Height && foundColor == false; y++)
            {
                var color = image[x, y];
                if (color.R < threshhold || color.G < threshhold || color.B < threshhold)
                    foundColor = true;
            }
            rightOffset += 1;
        }


        foundColor = false;
        // Get bottom bounds to crop
        for (int y = image.Height - 1; y >= 1 && foundColor == false; y--)
        {
            for (int x = 1; x < image.Width && foundColor == false; x++)
            {
                var color = image[x, y];
                if (color.R < threshhold || color.G < threshhold || color.B < threshhold)
                    foundColor = true;
            }
            bottomOffset += 1;
        }


        var bounds = new Rectangle(
            leftOffset, 
            topOffset, 
            image.Width - leftOffset - rightOffset, 
            image.Height - topOffset - bottomOffset
        );
        return bounds;
    }
}
