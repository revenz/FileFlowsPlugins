using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes;

/// <summary>
/// Element that extracts subtitles
/// </summary>
public class SubtitleExtractor : EncodingNode
{
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets the icon for the element
    /// </summary>
    public override string Icon => "fas fa-comment-dots";
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/subtitle-extractor";

    /// <summary>
    /// Gets or sets the language to extract
    /// </summary>
    [Text(1)]
    public string Language { get; set; }
    /// <summary>
    /// Gets or sets if all subtitles should be extracted
    /// </summary>
    [Boolean(2)]
    public bool ExtractAll { get; set; }
    /// <summary>
    /// Gets or sets the destination output file
    /// </summary>
    [File(3)]
    [ConditionEquals(nameof(ExtractAll), false)]
    public string OutputFile { get; set; }
    
    
    // /// <summary>
    // /// Gets or sets if the new file should be set as the current working file
    // /// </summary>
    // [Boolean(3)]
    // public bool SetWorkingFile { get; set; }
    
    /// <summary>
    /// Gets or sets if only forced subtitles should be extracted
    /// </summary>
    [Boolean(4)]
    public bool ForcedOnly { get; set; }
    /// <summary>
    /// Gets or sets if only text subtitles should be extracted
    /// </summary>
    [Boolean(5)]
    public bool OnlyTextSubtitles { get; set; }
    
    private Dictionary<string, object> _Variables;
    /// <summary>
    /// Gets or sets the variables
    /// </summary>
    public override Dictionary<string, object> Variables => _Variables;
    
    /// <summary>
    /// Creates an instance of the subtitle extractor
    /// </summary>
    public SubtitleExtractor()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "sub.FileName", "/path/to/subtitle.sub" }
        };
    }

    /// <summary>
    /// Executes the subtitle extractor
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        try
        {
            VideoInfo videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return -1;
            
            // ffmpeg -i input.mkv -map "0:m:language:eng" -map "-0:v" -map "-0:a" output.srt
            var subTracks = videoInfo.SubtitleStreams?.Where(x =>
            {
                if (OnlyTextSubtitles && SubtitleHelper.IsImageSubtitle(x.Codec))
                    return false;

                if (ForcedOnly && x.Forced == false)
                    return false;
                if(string.IsNullOrWhiteSpace(Language))
                    return true;
                if (x.Language?.ToLowerInvariant() == Language.ToLowerInvariant())
                    return true;

                try
                {
                    var rgx = new Regex(Language, RegexOptions.IgnoreCase);
                    if (rgx.IsMatch(x.Language))
                        return true;
                }
                catch (Exception)
                {
                }

                return false;
            }).ToArray();
            if (subTracks?.Any() != true)
            {
                args.Logger?.ILog("No subtitles found to extract");
                return 2;
            }

            int extractedCount = 0;

            for(int i=0;i<(ExtractAll ? subTracks.Length : 1);i++)
            {
                var subTrack = subTracks[i];
                string output;
                if (ExtractAll == false && string.IsNullOrEmpty(OutputFile) == false)
                {
                    output = args.ReplaceVariables(OutputFile, true);
                    output = args.MapPath(output);
                }
                else
                {
                    string fileFullname = args.FileName;
                    string fileExtension = FileHelper.GetExtension(fileFullname);
                    output = fileFullname[..fileFullname.LastIndexOf(fileExtension, StringComparison.Ordinal)];

                    output = output +
                             (string.IsNullOrWhiteSpace(subTrack.Language) == false ? subTrack.Language  : "") +
                             (i == 0 ? "" : "." + i);
                    
                    output = output.Replace("..", ".");
                }


                if (output.ToLower().EndsWith(".srt") || output.ToLower().EndsWith(".sup"))
                    output = output[0..^4];

                output = output.TrimEnd('.') + (subTrack.IsImage ? ".sup" : ".srt");
                
                args.Logger?.ILog($"Extracting subtitle codec '{subTrack.Codec}' to '{output}'");

                var extracted = ExtractSubtitle(args, FFMPEG, "0:s:" + subTrack.TypeIndex, output);
                if (extracted)
                    ++extractedCount;
                if (extracted && ExtractAll == false)
                {
                    args.UpdateVariables(new Dictionary<string, object>
                    {
                        { "sub.FileName", output }
                    });
                    // if (SetWorkingFile)
                    //     args.SetWorkingFile(OutputFile, dontDelete: true);

                    return 1;
                }

                if (ExtractAll == false)
                    break;
            }

            args.Logger?.ILog($"Extracted {extractedCount} subtitle{(extractedCount == 1 ? "" : "s")}");
            return extractedCount > 0 ? 1 : 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed processing VideoFile: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }

    /// <summary>
    /// Extracts a subtitle
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="ffmpegExe">the FFmpeg executable</param>
    /// <param name="subtitleStream">the FFmpeg subtitle stream to extract</param>
    /// <param name="output">the destination file of the extracted subtitle</param>
    /// <returns>if it was successful or not</returns>
    internal bool ExtractSubtitle(NodeParameters args, string ffmpegExe, string subtitleStream, string output)
    {
        if (File.Exists(output))
        {
            args.Logger?.ILog("File already exists, deleting file: " + output);
            File.Delete(output);
        }

        bool textSubtitles = Regex.IsMatch(output.ToLower(), @"\.(sup)$") == false;
        // -y means it will overwrite a file if output already exists
        var result = args.Process.ExecuteShellCommand(new ExecuteArgs
        {
            Command = ffmpegExe,
            ArgumentList = textSubtitles ? 
                new[] {

                    "-i", args.WorkingFile,
                    "-map", subtitleStream,
                    output
                } : 
                new[] {

                    "-i", args.WorkingFile,
                    "-map", subtitleStream,
                    "-c:s", "copy",
                    output
                }
        }).Result;

        var of = new FileInfo(output);
        args.Logger?.ILog("Extraction exit code: " + result.ExitCode);
        if (result.ExitCode != 0)
        {
            args.Logger?.ELog("FFMPEG process failed to extract subtitles");
            args.Logger?.ILog("Unexpected exit code: " + result.ExitCode);
            args.Logger?.ILog(result.StandardOutput ?? String.Empty);
            args.Logger?.ILog(result.StandardError ?? String.Empty);
            if (of.Exists && of.Length == 0)
            {
                // delete the output file if it created an empty file
                try
                {
                    of.Delete();
                }
                catch (Exception) { }
            }
            return false;
        }
        args.Logger?.ILog("Extracted file successful: " + of.Exists);
        return of.Exists;
    }
}
