using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace FileFlows.VideoNodes.VideoNodes;

internal class DetectEndCredits: VideoNode
{
    public override int Execute(NodeParameters args)
    {
        var imageDir = ExportImages(args, args.WorkingFile);
        var time = ScanImages(args, imageDir);
        return 1;
    }

    private string ExportImages(NodeParameters args, string file)
    {
        string dir = Path.Combine(args.TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var result = args.Execute(new()
        {
            Command = FFMPEG,
            ArgumentList = new[]
            {
                "-sseof",
                "-600",
                "-i",
                file,
                "-vf",
                "fps=1",
                Path.Combine(dir, "out%04d.png")
            }
        });
        return dir;
    }

    private object ScanImages(NodeParameters args, string imageDir)
    {
        var images = Directory.GetFiles(imageDir, "*.png");
        DateTime dt = DateTime.Now;
 
        using var engine = new TesseractEngine(@"D:\videos\temp\tesseract", "eng", EngineMode.Default);
        Dictionary<string, int> imagesWithText = new();
        bool last2 = false, last1 = false;
        for(int i=0;i<images.Length;i += 5) // every 5th image just to speed things up
        {
            var imageFile = images[i];
            using var img = Pix.LoadFromFile(imageFile);
            using var page = engine.Process(img);
            var text = page.GetText();
            bool hasText = false;
            if (string.IsNullOrWhiteSpace(text) == false)
            {
                text = Regex.Replace(text, "[^\\w]", string.Empty);
                if (text.Length > 10)
                {
                    hasText = true;
                    imagesWithText.Add(imageFile, 1 + (last2 ? 1 : 0) + (last1 ? 1: 0));
                    args.Logger.DLog(imageFile + " , text = " + text);
                }
            }
            last2 = last1;
            last1 = hasText;
        }
        args.Logger.ILog("Time taken to scan images: "+ (DateTime.Now.Subtract(dt)));
        return imagesWithText;
    }

}
