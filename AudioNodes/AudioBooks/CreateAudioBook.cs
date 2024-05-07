using FileFlows.AudioNodes.Helpers;

namespace FileFlows.AudioNodes;

/// <summary>
/// Creates an audio book from audio files in a directory
/// </summary>
public class CreateAudioBook: AudioNode
{
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-book";

    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 2;

    /// <summary>
    /// Gets the Help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/create-audio-book";

    /// <summary>
    /// Gets the flow element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;
    
    /// <summary>
    /// Gets or sets the destination path
    /// </summary>
    [Folder(1)]
    public string DestinationPath { get; set; }
    
    /// <summary>
    /// Gets or sets if the source files should be deleted
    /// </summary>
    [Boolean(2)] public bool DeleteSourceFiles { get; set; }
    
    /// <summary>
    /// Gets or sets if the working file should be updated
    /// </summary>
    [Boolean(3)] public bool UpdateWorkingFile { get; set; }
    
    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next, -1 to abort flow, 0 to end flow</returns>
    public override int Execute(NodeParameters args)
    {
        var ffmpegExeResult = GetFFmpeg(args);
        if (ffmpegExeResult.Failed(out string ffmpegError))
        {
            args.FailureReason = ffmpegError;
            args.Logger?.ELog(ffmpegError);
            return -1;
        }
        string ffmpeg = ffmpegExeResult.Value;

        var ffprobeResult = GetFFprobe(args);
        if (ffprobeResult.Failed(out string ffprobeError))
        {
            args.FailureReason = ffprobeError;
            args.Logger?.ELog(ffprobeError);
            return -1;
        }
        string ffprobe = ffprobeResult.Value;

        var dir = args.IsDirectory ? args.WorkingFile : FileHelper.GetDirectory(args.WorkingFile);

        var allowedExtensions = new List<string> { ".mp3", ".aac", ".m4b", ".m4a", ".flac" };
        var dirFiles = args.FileService.GetFiles(dir, "*.*");
        List<string> files = new ();
        foreach (var file in dirFiles.ValueOrDefault ?? new string[] {})
        {
            string extension = FileHelper.GetExtension(file);
            if (allowedExtensions.Contains(extension.ToLower()) == false)
                continue;
            args.Logger?.ILog("Found audio file: " + file);
            files.Add(file);
        }

        if (files.Any() == false)
        {
            args.Logger.WLog("No audio files found in directory: " + dir);
            return 2;
        }

        if (files.Count == 1)
        {
            args.Logger.WLog("Only one audio file found: " + files[0]);
            return 2;
        }

        var rgxNumbers = new Regex(@"[\d]+");
        files = files.OrderBy(x =>
        {
            string extension = FileHelper.GetExtension(x).TrimStart('.');
            string shortname = FileHelper.GetShortFileName(x);
            if (string.IsNullOrEmpty(extension) == false)
                shortname = shortname[..^(extension.Length + 1)];
            var matches = rgxNumbers.Matches(shortname);
            if (matches.Any() == false)
            {
                args.Logger.ILog("No number found in: " + shortname);
                return 100000;
            }

            if (matches.Count == 1)
            {
                args.Logger.ILog($"Number [{matches[0].Value}] found in: " + shortname);
                return int.Parse(matches[0].Value);
            }

            // we may have a year, if first number is 4 digits, assume year 
            var number = matches[0].Length == 4 ? int.Parse(matches[1].Value) : int.Parse(matches[0].Value);
            args.Logger.ILog($"Number [{number}] found in: " + shortname);
            return number;
        }).ToList();
        
        var readResult = new AudioInfoHelper(ffmpeg, ffprobe, args.Logger).Read(files.First());
        var audioInfo = readResult.IsFailed ? null : readResult.Value;

        string metadataFile = FileHelper.Combine(args.TempPath, "CreateAudioBookChapters-metadata.txt");
        TimeSpan current = TimeSpan.Zero;
        string bookName = FileHelper.GetShortFileName(dir);
        int chapterCount = 1;

        string metadataContents = @";FFMETADATA1\n\n" + string.Join("\n", files.Select(x =>
        {
            string extension = FileHelper.GetExtension(x);
            string name = FileHelper.GetShortFileName(x);
            string chapterName = GetChapterName(bookName, name[..^extension.Length], chapterCount);
            TimeSpan length = GetChapterLength(args, ffmpeg, ffprobe, x);
            var end = current.Add(length);
            var chapter = "[CHAPTER]\n" +
                          "TIMEBASE=1/1000\n" +
                          "START=" + (int)(current.TotalMilliseconds + 1500) + "\n" +
                          "END=" + ((int)end.TotalMilliseconds) + "\n" +
                          "title=" + chapterName + "\n";
            current = end;
            ++chapterCount;
            return chapter;
        }));
        args.Logger?.ILog("Metadata Content:\n" + metadataContents);
        System.IO.File.WriteAllText(metadataFile, metadataContents);
        string inputFiles = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".txt");
        string strInputFiles = string.Join("\n", files.Select(x => $"file '{x}'"));
        System.IO.File.WriteAllText(inputFiles, strInputFiles);
        args.Logger?.ILog("Input Files:\n" + inputFiles);

        string outputFile = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".m4b");

        string? artwork = null; //FindArtwork(dir);

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

        if (audioInfo != null)
        {
            audioInfo.Track = 0;
            audioInfo.TotalDiscs = 0;
            audioInfo.TotalTracks = 0;
            var metadata = MetadataHelper.GetMetadataParameters(audioInfo);
            if (metadata.Any())
                execArgs.AddRange(metadata);
        }

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

        if (System.IO.File.Exists(outputFile) == false || new System.IO.FileInfo(outputFile).Length == 0)
        {
            args.FailureReason = "Failed to create output file: " + outputFile;
            args.Logger.ELog("Failed to create output file: " + outputFile);
            return -1;
        }
        args.Logger.ILog("Created output file: " + outputFile);

        if (DeleteSourceFiles)
        {
            foreach (var file in files)
            {
                var result = args.FileService.FileDelete(file);
                if(result.IsFailed)
                    args.Logger.ILog("Failed deleting source file: " + file + " -> " + result.Error);
                else if(result.Value == false)
                    args.Logger.ILog("Failed deleting source file: " + file);
                else
                    args.Logger.ILog("Deleted source file: " + file);
            }
        }

        if (string.IsNullOrWhiteSpace(DestinationPath) == false)
        {
            var dest = args.ReplaceVariables(DestinationPath, stripMissing: true);
            if (args.FileService.FileMove(outputFile, dest).Failed(out string error))
            {
                args.FailureReason = "Failed to save to destination: " + error;
                args.Logger?.ELog("Failed to save to destination: " + error);
                return -1;
            }

            outputFile = dest;
        }

        if (UpdateWorkingFile)
        {
            args.Logger?.ILog("Updating working file to: " + outputFile);
            args.IsDirectory = false; // no longer a directory based flow
            args.SetWorkingFile(outputFile);
        }
        
        return 1;
    }

    // private string FindArtwork(DirectoryInfo dir)
    // {
    //     var files = dir.GetFiles("*.*");
    //     var extensions = new[] { ".png", ".jpg", ".jpe", ".jpeg" };
    //     foreach(string possible in new [] { "cover", "artwork", "thumbnail", dir.Name.ToLowerInvariant()})
    //     {
    //         var matching = files.Where(x => x.Name.ToLowerInvariant().StartsWith(possible)).ToArray();
    //         foreach (var file in matching)
    //         {
    //             if (extensions.Contains(file.Extension.ToLowerInvariant()))
    //                 return file.FullName;
    //         }
    //     }
    //
    //     return string.Empty;
    // }

    private TimeSpan GetChapterLength(NodeParameters args, string ffmpeg, string ffprobe, string filename)
    {
        var info = new AudioInfoHelper(ffmpeg, ffprobe, args.Logger).Read(filename).Value;
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