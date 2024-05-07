using Docnet.Core;
using Docnet.Core.Editors;
using Docnet.Core.Models;

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

            var file = Path.Combine(destinationDirectory,
                filePrefix + "-" + i.ToString(new string('0', pageCount.ToString().Length)));
            var result = args!.ImageHelper.SaveImage(rawBytes, file);
            if (result.Failed(out string error))
            {
                args.Logger?.WLog("Failed to save image: " + error);
                continue;
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
    /// Creates a PDF from images
    /// </summary>
    /// <param name="args">the NodeParameters</param>
    /// <param name="directory">the directory to of images</param>
    /// <param name="output">the output file of the pdf</param>
    /// <param name="halfProgress">if the NodePArameter.PartPercentageUpdate should start at 50%</param>
    internal static void Create(NodeParameters args, string directory, string output, bool halfProgress = true)
    {
        if (args.PartPercentageUpdate != null)
            args.PartPercentageUpdate(halfProgress ? 50 : 0);
        var rgxImages = new Regex(@"\.(jpeg|jpg|jpe|png|webp)$");
        var files = Directory.GetFiles(directory).Where(x => rgxImages.IsMatch(x)).ToArray();

        List<JpegImage> images = new List<JpegImage>();
        for(int i = 0; i < files.Length; i++)   
        {
            var file = files[i];
            if (file.ToLowerInvariant().EndsWith(".png") || file.ToLowerInvariant().EndsWith(".webp"))
            {
                string jpegImage = Path.ChangeExtension(file, "jpg");
                args!.ImageHelper.ConvertToJpeg(file, jpegImage, null);
                file = jpegImage;
            }

            (int width, int height) = args!.ImageHelper.GetDimensions(file).Value;
            
            var jpeg = new JpegImage
            {
                Bytes = File.ReadAllBytes(file),
                Width = width,
                Height = height
            };
            images.Add(jpeg);
            
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
