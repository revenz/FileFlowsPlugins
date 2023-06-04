namespace FileFlows.VideoNodes.Helpers;

/// <summary>
/// Helper for Subtitles
/// </summary>
internal class SubtitleHelper
{
    
    public static readonly string[]  MkvSubtitles = new[]
    {
        "ass",        // Advanced SubStation Alpha
        "ssa",        // SubStation Alpha subtitle
        "srt",        // SubRip subtitle
        "subrip",     // SubRip subtitle (alternative name)
        "vtt",        // WebVTT subtitle
        "webvtt",     // WebVTT subtitle (alternative name)
        "smi",        // SAMI subtitle format
        "sami",       // SAMI subtitle format (alternative name)
        "rt",         // RealText subtitle format
        "realtext",   // RealText subtitle format (alternative name)
        "stl",        // EBU STL (Subtitling Data Exchange Format)
        "ttml",       // Timed Text Markup Language
        "ttml_legacy" // Timed Text Markup Language (legacy name)
    };
    
    /// <summary>
    /// Tests if a subtitle is an image based subtitle
    /// </summary>
    /// <param name="codec">the subtitle codec</param>
    /// <returns>true if the subtitle is an image based subtitle</returns>
    internal static bool IsImageSubtitle(string codec)
        => Regex.IsMatch(codec.Replace("_", ""), "dvbsub|pgs|xsub|vobsub", RegexOptions.IgnoreCase);

    /// <summary>
    /// Determines the appropriate subtitle codec for conversion based on the container type and current subtitle codec.
    /// </summary>
    /// <param name="containerType">The container type (mp4, mkv, webm).</param>
    /// <param name="currentCodec">The current subtitle codec.</param>
    /// <returns>The appropriate subtitle codec for conversion, or null container does not support this codec.</returns>
    public static string? GetSubtitleCodec(string containerType, string currentCodec)
    {
        // Check if the current subtitle codec is image-based
        bool isImageBased = IsImageSubtitle(currentCodec);

        // Determine the appropriate subtitle codec based on the container type and if the current codec is image-based or text-based
        switch (containerType.ToLower())
        {
            case "mp4":
                if (isImageBased)
                {
                    // MP4 container does not support image-based subtitles, so conversion is not possible
                    return null;
                }
                return "mov_text";
            case "mkv":
                if (isImageBased)
                    return "hdmv_pgs_subtitle";
                if (IsSupportedSubtitleCodecMKV(currentCodec) == false)
                    return "srt"; // or "ssa" or any other supported codec
                return currentCodec;
            case "webm":
                if (isImageBased)
                {
                    // WebM container does not support image-based subtitles, so conversion is not possible
                    return null;
                }
                // WebM container supports text-based subtitles in the webvtt codec
                return "webvtt";
            default:
                // Invalid or unsupported container type
                return null;
        }
    }
    
    /// <summary>
    /// Checks if the subtitle codec is supported in MKV container.
    /// </summary>
    /// <param name="codec">The subtitle codec to check.</param>
    /// <returns>True if the codec is supported in MKV, False otherwise.</returns>
    private static bool IsSupportedSubtitleCodecMKV(string codec)
        => Array.IndexOf(MkvSubtitles, codec.ToLower()) >= 0;

}
