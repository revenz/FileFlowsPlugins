using System.ComponentModel;

namespace FileFlows.ComicNodes.Comics;

public class ComicConverter: Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-book";
    public override bool FailureNode => true;


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
                        new ListOption { Label = "CBZ", Value = "cbz"},
                        //new ListOption { Label = "CB7", Value = "cb7"},
                        new ListOption { Label = "PDF", Value = "pdf" }
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
        if(currentFormat[0] == '.')
            currentFormat = currentFormat[1..]; // remove the dot
        currentFormat = currentFormat.ToLower();

        if(currentFormat == Format)
        {
            args.Logger?.ILog($"Already in the target format of '{Format}'");
            return 2;
        }

        string destinationPath = Path.Combine(args.TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(destinationPath);
        if (Helpers.ComicExtractor.Extract(args, args.WorkingFile, destinationPath, halfProgress: true) == false)
            return -1;

        string newFile = CreateComic(args, destinationPath, this.Format);

        args.SetWorkingFile(newFile);   

        return 1;
    }

    private string CreateComic(NodeParameters args, string directory, string format)
    {
        string file = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + "." + format);
        args.Logger?.ILog("Creating comic: " + file);
        if (format == "cbz")
            Helpers.ZipHelper.Compress(args, directory, file);
        //else if (format == "cb7")
        //    Helpers.SevenZipHelper.Compress(args, directory, file + ".7z");
        else if (format == "pdf")
            Helpers.PdfHelper.Create(args, directory, file);
        else
            throw new Exception("Unknown format:" + format);
        Directory.Delete(directory, true);
        args.Logger?.ILog("Created comic: " + file);
        args.Logger?.ILog("Deleted temporary extraction directory: " + directory);
        return file;
    }
}
