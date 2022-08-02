using Docnet.Core;
using Docnet.Core.Editors;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.RegularExpressions;

namespace FileFlows.ComicNodes.Helpers;

internal class PdfHelper
{
    public static void Extract(NodeParameters args, string pdfFile, string destinationDirectory, string filePrefix, bool halfProgress = true)
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
            string file = Path.Combine(destinationDirectory, filePrefix + "-" + i.ToString(new String('0', pageCount.ToString().Length)) + ".png");
            image.SaveAsPng(file);

            if (args?.PartPercentageUpdate != null)
            {
                float percent = (i / pageCount) * 100f;
                if (halfProgress)
                    percent = (percent / 2);
                args?.PartPercentageUpdate(percent);
            }
        }
        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 0);
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
}
