using Docnet.Core;
using Docnet.Core.Editors;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace FileFlows.ComicNodes.Helpers;

internal class PdfHelper
{
    public static void Extract(NodeParameters args, string pdfFile, string destinationDirectory, string filePrefix, bool halfProgress, CancellationToken cancellation)
    {
        using var library = DocLib.Instance;
        using var docReader = library.GetDocReader(pdfFile, new PageDimensions(1080, 1920));

        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 0);

        int pageCount = docReader.GetPageCount();
        for (int i = 1; i < pageCount; i++)
        {
            using var pageReader = docReader.GetPageReader(i);
            var rawBytes = pageReader.GetImage();

            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            using var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
            
            // Infer the image format
            (IImageFormat? imageFormat, string? fileExtension) = InferImageFormat(rawBytes);
            if (imageFormat == null)
            {
                args?.Logger?.WLog("Failed to inter image type from PDF, failing back to JPG");
                imageFormat = JpegFormat.Instance;
                fileExtension = "jpg";
            }
            else
            {
                args?.Logger?.ILog("File Extension of image: " + fileExtension);
            }

            var file = Path.Combine(destinationDirectory, 
                filePrefix + "-" + i.ToString(new string('0', pageCount.ToString().Length))) 
                       + "." + fileExtension;

            using (var outputStream = File.Create(file + "." + fileExtension))
            {
                image.Save(outputStream, imageFormat);
            }

            if (args?.PartPercentageUpdate != null)
            {
                float percent = (i / pageCount) * 100f;
                if (halfProgress)
                    percent = (percent / 2);
                args?.PartPercentageUpdate(percent);
            }
            if (cancellation.IsCancellationRequested)
                return;
        }
        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 0);
    }

    /// <summary>
    /// Infers the image format based on the first few bytes of the image data.
    /// </summary>
    /// <param name="bytes">The image data bytes.</param>
    /// <returns>The inferred image format and file extension.</returns>
    private static (IImageFormat? Format, string Extension)  InferImageFormat(byte[] bytes)
    {
        // Try to infer image format based on magic numbers
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xD8) // JPEG
            return (JpegFormat.Instance, "jpg");
        if (bytes.Length >= 8 && BitConverter.ToUInt64(bytes, 0) == 0x89504E470D0A1A0A) // PNG
            return (PngFormat.Instance, "png");
        if (bytes.Length >= 4 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38) // GIF
            return (GifFormat.Instance, "gif");
        if (bytes.Length >= 4 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50) // WebP
            return (WebpFormat.Instance, "webp");
        if (bytes.Length >= 4 && BitConverter.ToUInt32(bytes, 0) == 0x49492A00) // TIFF
            return (TiffFormat.Instance, "tiff");
        if (bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D) // BMP
            return (BmpFormat.Instance, "bmp");

        // If none of the known formats are detected, fall back to Image.DetectFormat()
        try
        {
            IImageFormat format = Image.DetectFormat(bytes);
            string extension = format?.DefaultMimeType?.Split('/')[1] ?? "png";
            return (format, extension);
        }
        catch (Exception)
        {
            return (null, null);
        }
    }


    /// <summary>
    /// Creates a PDF from images
    /// </summary>
    /// <param name="args">the NodeParameters</param>
    /// <param name="directory">the directory to of images</param>
    /// <param name="output">the output file of the pdf</param>
    /// <param name="halfProgress">if the NodePArameter.PartPercentageUpdate should start at 50%</param>
    internal static void Create(NodeParameters args, string directory, string output, bool halfProgress = true)
    {
        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 0);
        var rgxImages = new Regex(@"\.(jpeg|jpg|jpe|png|webp)$");
        var files = Directory.GetFiles(directory).Where(x => rgxImages.IsMatch(x)).ToArray();

        List<JpegImage> images = new List<JpegImage>();
        for(int i = 0; i < files.Length; i++)   
        {
            var file = files[i];
            var format = Image.DetectFormat(file);
            var info = Image.Identify(file);
            if (file.ToLower().EndsWith(".png"))
            {
                var img = Image.Load(file);
                using var memoryStream = new MemoryStream();
                img.SaveAsJpeg(memoryStream);
                var jpeg = new JpegImage
                {
                    Bytes = memoryStream.ToArray(),
                    Width = info.Width,
                    Height = info.Height
                };
                images.Add(jpeg);
            }
            else if(file.ToLower().EndsWith(".webp"))
            {
                var img = Image.Load(file);
                using var memoryStream = new MemoryStream();
                img.SaveAsJpeg(memoryStream);
                var jpeg = new JpegImage
                {
                    Bytes = memoryStream.ToArray(),
                    Width = info.Width,
                    Height = info.Height
                };
                images.Add(jpeg);
            }
            else // jpeg
            {
                var jpeg = new JpegImage
                {
                    Bytes = File.ReadAllBytes(file),
                    Width = info.Width,
                    Height = info.Height
                };
                images.Add(jpeg);

            }
            if (args?.PartPercentageUpdate != null)
            {
                float percent = (i / ((float)files.Length)) * 100f;
                if (halfProgress)
                    percent = 50 + (percent / 2);
                args?.PartPercentageUpdate(percent);
            }
        }

        var bytes = DocLib.Instance.JpegToPdf(images);
        File.WriteAllBytes(output, bytes);

        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(100);
    }

    /// <summary>
    /// Gets the number of pages in a PDF
    /// </summary>
    /// <param name="pdfFile">the PDF file</param>
    /// <returns>the number of pages in the PDF</returns>
    internal static int GetPageCount(string pdfFile)
    {
        using var library = DocLib.Instance;
        using var docReader = library.GetDocReader(pdfFile, new PageDimensions(1080, 1920));
        return docReader.GetPageCount();
    }
}
