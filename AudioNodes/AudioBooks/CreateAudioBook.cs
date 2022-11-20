using System.Text.RegularExpressions;
using FileFlows.Plugin;

namespace FileFlows.AudioNodes.AudioBooks;

/// <summary>
/// Creates an audio book from audio files in a directory
/// </summary>
public class CreateAudioBook: AudioNode
{
    public override string Icon => "fas fa-book";

    public override int Inputs => 1;
    
    public override int Outputs => 2;

    public override int Execute(NodeParameters args)
    {
        var ffmpeg = GetFFmpeg(args);
        if (string.IsNullOrEmpty(ffmpeg))
            return -1;

        var dir = args.IsDirectory ? new DirectoryInfo(args.WorkingFile) : new FileInfo(args.WorkingFile).Directory;

        var allowedExtensions = new[] { ".mp3", ".aac", ".m4b", ".m4a" };
        List<FileInfo> files = new List<FileInfo>();
        foreach (var file in dir.GetFiles("*.*"))
        {
            if (file.Extension == null)
                continue;
            if (allowedExtensions.Contains(file.Extension.ToLower()) == false)
                continue;
            files.Add(file);
        }

        if (files.Any() == false)
        {
            args.Logger.WLog("No audio files found in directory: " + dir.FullName);
            return 2;
        }

        if (files.Count == 1)
        {
            args.Logger.WLog("Only one audio file found: " + files[0].FullName);
            return 2;
        }

        var rgxNumbers = new Regex(@"[\d]+");
        files = files.OrderBy(x =>
        {
            var shortname = x.Name[..^x.Extension.Length];
            var matches = rgxNumbers.Matches(shortname);
            if (matches.Any() == false)
            {
                args.Logger.DLog("No number found in: " + shortname);
                return 100000;
            }

            if (matches.Count == 1)
            {
                args.Logger.DLog($"Number [{matches[0].Value}] found in: " + shortname);
                return int.Parse(matches[0].Value);
            }

            // we may have a year, if first number is 4 digits, assume year 
            var number = matches[0].Length == 4 ? int.Parse(matches[1].Value) : int.Parse(matches[0].Value);
            args.Logger.DLog($"Number [{number}] found in: " + shortname);
            return number;
        }).ToList();

        string metadataFile = Path.Combine(args.TempPath, "CreateAudioBookChapters-metadata.txt");
        TimeSpan current = TimeSpan.Zero;
        string bookName = dir.Name;
        int chapterCount = 1;
        File.WriteAllText(metadataFile, @";FFMETADATA1\n\n" + string.Join("\n", files.Select(x =>
        {
            string chapterName = GetChapterName(bookName, x.Name[..^x.Extension.Length], chapterCount);
            TimeSpan length = GetChapterLength(args, ffmpeg, x.FullName);
            var end = current.Add(length);
            var chapter = "[CHAPTER]\n" +
                          "TIMEBASE=1/1000\n" +
                          "START=" + (int)(current.TotalMilliseconds + 1500) + "\n" +
                          "END=" + ((int)end.TotalMilliseconds) + "\n" +
                          "title=" + chapterName + "\n";
            current = end;
            ++chapterCount;
            return chapter;
        })));
        string inputFiles = Path.Combine(args.TempPath, Guid.NewGuid() + ".txt");
        File.WriteAllText(inputFiles, string.Join("\n", files.Select(x => $"file '{x.FullName}'")));

        string outputFile = Path.Combine(args.TempPath, Guid.NewGuid() + ".m4b");

        string artwork = null; //FindArtwork(dir);

        List<string> execArgs = new() { 
            "-f",
            "concat",
            "-safe",
            "0",
            "-i",
            inputFiles,
            "-i",
            metadataFile
        };

        if (string.IsNullOrEmpty(artwork) == false)
        {
            execArgs.AddRange(new []
            {
                "-i",
                artwork,
                "-map", "0",
                "-map_metadata", "1",
                "-map", "2",
                "-c:v:2", artwork.ToLower().EndsWith("png") ? "png" : "jpg" 
            });
        }
        else
        {
            execArgs.AddRange(new []
            {
                "-map", "0",
                "-map_metadata", "1",
            });
        }

        execArgs.AddRange(new[]
        {
            "-vn", // no video
            //"-c", "copy",
            "-c:a", "aac",
            //"-b:a", "128k",
        });


        if (string.IsNullOrEmpty(artwork) == false)
        {
            execArgs.AddRange(new []
            {
                "-disposition:0",
                "attached_pic"
            });
        }
        
        execArgs.Add(outputFile);

        args.Execute(new()
        {
            Command = ffmpeg,
            ArgumentList = execArgs.ToArray()
        });

        if (File.Exists(outputFile) == false || new FileInfo(outputFile).Length == 0)
        {
            args.Logger.ELog("Failed to create output file: " + outputFile);
            return -1;
        }
        args.Logger.ELog("Created output file: " + outputFile);
        return 1;
    }

    private string FindArtwork(DirectoryInfo dir)
    {
        var files = dir.GetFiles("*.*");
        var extensions = new[] { ".png", ".jpg", ".jpe", ".jpeg" };
        foreach(string possible in new [] { "cover", "artwork", "thumbnail", dir.Name.ToLowerInvariant()})
        {
            var matching = files.Where(x => x.Name.ToLowerInvariant().StartsWith(possible)).ToArray();
            foreach (var file in matching)
            {
                if (extensions.Contains(file.Extension.ToLowerInvariant()))
                    return file.FullName;
            }
        }

        return string.Empty;
    }

    private TimeSpan GetChapterLength(NodeParameters args, string ffmpeg, string filename)
    {
        var info = new AudioInfoHelper(ffmpeg, args.Logger).Read(filename);
        return TimeSpan.FromSeconds(info.Duration);
    }

    private string GetChapterName(string bookName, string chapterName, int chapter)
    {
        chapterName= chapterName.Replace(bookName, string.Empty).Trim();
        string test = chapterName.Replace("0", string.Empty);
        test = test.Replace(chapter.ToString(), string.Empty);
        test = Regex.Replace(test, "[\"',\\.]", string.Empty);
        if(string.IsNullOrEmpty(test))
            return "Chapter " + chapter;
        return chapterName;
    }
}