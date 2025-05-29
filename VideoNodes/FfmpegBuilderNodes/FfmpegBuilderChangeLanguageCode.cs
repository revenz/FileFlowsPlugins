using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Changes the language codes in the FFmpeg and VideoInfo models
/// </summary>
public class FfmpegBuilderChangeLanguageCode : FfmpegBuilderNode
{
    /// <inheritdoc/>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/change-language-code";

    /// <inheritdoc/>
    public override int Outputs => 2;

    /// <inheritdoc/>
    public override string Icon => "fas fa-language";
    
    /// <summary>
    /// Gets or sets replacements to replace
    /// </summary>
    [KeyValue(1, null)]
    [Required]
    public List<KeyValuePair<string, string>> Replacements{ get; set; }
    
    /// <inheritdoc/>
    public override int Execute(NodeParameters args)
    {
        if (Replacements?.Any() != true)
            return 2; // no replacements

        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
            return args.Fail("Failed to retrieve video info");
        
        var model = GetModel();
        bool changed = false;
        changed |= ChangeFFmpegStream(model.VideoStreams ?? [], args.Logger, args.StringHelper, Replacements);
        changed |= ChangeFFmpegStream(model.AudioStreams ?? [], args.Logger, args.StringHelper, Replacements);
        changed |= ChangeFFmpegStream(model.SubtitleStreams ?? [], args.Logger, args.StringHelper, Replacements);

        changed |= ChangeVideoFileStream(videoInfo.VideoStreams ?? [], args.Logger, args.StringHelper, Replacements);
        changed |= ChangeVideoFileStream(videoInfo.AudioStreams ?? [], args.Logger, args.StringHelper, Replacements);
        changed |= ChangeVideoFileStream(videoInfo.SubtitleStreams ?? [], args.Logger, args.StringHelper, Replacements);

        return changed ? 1 : 2;
    }

    public static bool ChangeFFmpegStream<T>(List<T> streams, ILogger logger, StringHelper stringHelper, List<KeyValuePair<string, string>> replacements ) where T: FfmpegStream
    {
        bool changed = false;
        foreach (var stream in streams)
        {
            foreach (var replacement in replacements)
            {
                if (stringHelper.Matches(stream.Language, replacement.Key))
                {
                    logger?.ILog($"Changing stream '{stream}' language from '{stream.Language}' to '{replacement.Value}'");
                    stream.Language = replacement.Value;
                    changed = true;
                }
            }
        }

        return changed;
    }

    public static bool ChangeVideoFileStream<T>(List<T> streams, ILogger logger, StringHelper stringHelper, List<KeyValuePair<string, string>> replacements) where T: VideoFileStream
    {
        bool changed = false;
        foreach (var stream in streams)
        {
            foreach (var replacement in replacements)
            {
                if (stringHelper.Matches(stream.Language, replacement.Key))
                {
                    logger?.ILog($"Changing stream '{stream}' language from '{stream.Language}' to '{replacement.Value}'");
                    stream.Language = replacement.Value;
                    changed = true;
                }
            }
        }

        return changed;
    }
}