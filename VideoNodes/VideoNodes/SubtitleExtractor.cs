using System.Net;
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
    /// Gets or sets the destination output file
    /// </summary>
    [File(2)]
    public string OutputFile { get; set; }
    /// <summary>
    /// Gets or sets if the new file should be set as the current working file
    /// </summary>
    [Boolean(3)]
    public bool SetWorkingFile { get; set; }
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
            var subTrack = videoInfo.SubtitleStreams?.Where(x =>
            {
                if (OnlyTextSubtitles && SubtitleHelper.IsImageSubtitle(x.Codec))
                    return false;

                if (ForcedOnly && x.Forced == false)
                    return false;
                if(string.IsNullOrEmpty(Language))
                    return true;
                if (x.Language?.ToLowerInvariant() == Language.ToLowerInvariant())
                    return true;
                return false;
            }).FirstOrDefault();
            if (subTrack == null)
            {
                args.Logger?.ILog("No subtitles found to extract");
                return 2;
            }

            if (string.IsNullOrEmpty(OutputFile) == false)
            {
                OutputFile = args.ReplaceVariables(OutputFile, true);
            }
            else
            {
                var file = new FileInfo(args.FileName);


                OutputFile = file.FullName.Substring(0, file.FullName.LastIndexOf(file.Extension));
            }
            OutputFile = args.MapPath(OutputFile);

            string extension = "srt";
            if (SubtitleHelper.IsImageSubtitle(subTrack.Codec))
                extension = "sup";
            if (OutputFile.ToLower().EndsWith(".srt") || OutputFile.ToLower().EndsWith(".sup"))
                OutputFile = OutputFile[0..^4];

            OutputFile += "." + extension;
            
            var extracted = ExtractSubtitle(args, FFMPEG, "0:s:" + subTrack.TypeIndex, OutputFile);
            if(extracted)
            {
                args.UpdateVariables(new Dictionary<string, object>
                {
                    { "sub.FileName", OutputFile }
                });
                if (SetWorkingFile)
                    args.SetWorkingFile(OutputFile, dontDelete: true);

                return 1;
            }

            return -1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed processing VideoFile: " + ex.Message);
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
        if (File.Exists(OutputFile))
        {
            args.Logger?.ILog("File already exists, deleting file: " + OutputFile);
            File.Delete(OutputFile);
        }

        bool textSubtitles = Regex.IsMatch(OutputFile.ToLower(), @"\.(sup)$") == false;
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

        var of = new FileInfo(OutputFile);
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
        return of.Exists;
    }
}
