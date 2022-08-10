using System.ComponentModel;

namespace FileFlows.ComicNodes.Comics;

public class ComicConverter: Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-book";
    public override bool FailureNode => true;

    CancellationTokenSource cancellation = new CancellationTokenSource();

    [DefaultValue("cbz")]
    [Select(nameof(FormatOptions), 1)]
    public string Format { get; set; } = string.Empty;

    private static List<ListOption> _FormatOptions;
    public static List<ListOption> FormatOptions
    {
        get
        {
            if (_FormatOptions == null)
            {
                _FormatOptions = new List<ListOption>
                    {
                        new ListOption { Label = "CBZ", Value = "CBZ"},
                        //new ListOption { Label = "CB7", Value = "cb7"},
                        new ListOption { Label = "PDF", Value = "PDF" }
                    };
            }
            return _FormatOptions;
        }
    }

    public override int Execute(NodeParameters args)
    {
        string currentFormat = new FileInfo(args.WorkingFile).Extension;
        if (string.IsNullOrEmpty(currentFormat))
        {
            args.Logger?.ELog("Could not detect format for: " + args.WorkingFile);
            return -1;
        }
        Format = Format?.ToUpper() ?? string.Empty;

        if (currentFormat[0] == '.')
            currentFormat = currentFormat[1..]; // remove the dot
        currentFormat = currentFormat.ToUpper();


        var metadata = new Dictionary<string, object>();
        int pageCount = GetPageCount(args, currentFormat, args.WorkingFile);
        metadata.Add("Format", currentFormat);
        metadata.Add("Pages", pageCount);
        args.RecordStatistic("COMIC_FORMAT", currentFormat);
        args.RecordStatistic("COMIC_PAGES", pageCount);
        args.SetMetadata(metadata);
        args.Logger?.ILog("Setting metadata: " + currentFormat);

        if (currentFormat == Format)
        {
            args.Logger?.ILog($"Already in the target format of '{Format}'");
            return 2;
        }

        string destinationPath = Path.Combine(args.TempPath, Guid.NewGuid().ToString());
        
        Directory.CreateDirectory(destinationPath);
        if (Helpers.ComicExtractor.Extract(args, args.WorkingFile, destinationPath, halfProgress: true, cancellation: cancellation.Token) == false)
            return -1;

        string newFile = CreateComic(args, destinationPath, this.Format);

        args.SetWorkingFile(newFile);   

        return 1;
    }

    public override Task Cancel()
    {
        cancellation.Cancel();
        return base.Cancel();
    }

    private int GetPageCount(NodeParameters args, string format, string workingFile)
    {
        if (format == null)
            return 0;
        format = format.ToUpper().Trim();
        switch (format)
        {
            case "PDF":
                return Helpers.PdfHelper.GetPageCount(workingFile);
            default:
                return Helpers.GenericExtractor.GetImageCount(args, workingFile);
        }
    }

    private string CreateComic(NodeParameters args, string directory, string format)
    {
        string file = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + "." + format.ToLower());
        args.Logger?.ILog("Creating comic: " + file);
        if (format == "CBZ")
            Helpers.ZipHelper.Compress(args, directory, file);
        //else if (format == "CB7")
        //    Helpers.SevenZipHelper.Compress(args, directory, file + ".7z");
        else if (format == "PDF")
            Helpers.PdfHelper.Create(args, directory, file);
        else
            throw new Exception("Unknown format:" + format);
        Directory.Delete(directory, true);
        args.Logger?.ILog("Created comic: " + file);
        args.Logger?.ILog("Deleted temporary extraction directory: " + directory);


        var metadata = new Dictionary<string, object>();
        metadata.Add("Format", format);
        metadata.Add("Pages", GetPageCount(args, format, file));
        args.SetMetadata(metadata);
        args.Logger?.ILog("Setting metadata: " + format);

        return file;
    }
}
