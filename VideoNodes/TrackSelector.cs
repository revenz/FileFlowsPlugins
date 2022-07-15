using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlows.VideoNodes;

internal class TrackSelector
{
    public string Title { get; set; }
    public string Codec{ get; set; }
    public string Language { get; set; }
    public bool NotMatching { get; set; }
    public float Channels { get; set; }

    public VideoStream FindVideoStream(VideoInfo videoInfo)
    {
        var stream = videoInfo.VideoStreams.Where(x =>
        {
            bool matches = true;
            if (TitleMatches(x.Title) == MatchResult.NoMatch)
                matches = false;
            else if (string.IsNullOrWhiteSpace(x.CodecTag) == false && CodecMatches(x.CodecTag) == MatchResult.Matched)
                matches = true;
            else if (CodecMatches(x.Codec) == MatchResult.NoMatch)
                matches = false;
            return matches != NotMatching;
        }).FirstOrDefault();
        return stream;
    }

    public AudioStream FindAudioStream(VideoInfo videoInfo)
    {
        if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Codec) && string.IsNullOrEmpty(Language) && Channels <= 0)
            return videoInfo.AudioStreams.FirstOrDefault();
        var stream = videoInfo.AudioStreams.Where(x =>
        {
            bool matches = true;
            if (TitleMatches(x.Title) == MatchResult.NoMatch)
                matches = false;
            else if (CodecMatches(x.Codec) == MatchResult.NoMatch)
                matches = false;
            else if (LanguageMatches(x.Language) == MatchResult.NoMatch)
                matches = false;
            else if (this.Channels > 0 && Math.Abs(x.Channels - this.Channels) > 0.05f)
                matches = false;
            return matches != NotMatching;
        }).FirstOrDefault();
        return stream;        
    }

    public SubtitleStream FindSubtitleStream(VideoInfo videoInfo)
    {
        var stream = videoInfo.SubtitleStreams.Where(x =>
        {
            bool matches = true;
            if (TitleMatches(x.Title) == MatchResult.NoMatch)
                matches = false;
            else if (CodecMatches(x.Codec) == MatchResult.NoMatch)
                matches = false;
            else if (LanguageMatches(x.Language) == MatchResult.NoMatch)
                matches = false;
            return matches != NotMatching;
        }).FirstOrDefault();
        return stream;
           
    }

    private MatchResult TitleMatches(string value) => ValueMatch(this.Title, value);
    private MatchResult CodecMatches(string value) => ValueMatch(this.Codec, value);
    private MatchResult LanguageMatches(string value) => ValueMatch(this.Language, value);
    private MatchResult ValueMatch(string pattern, string value)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return MatchResult.Skipped;
        try
        {

            if (string.IsNullOrEmpty(value))
                return MatchResult.NoMatch;
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            if (rgx.IsMatch(value))
                return MatchResult.Matched;

            if (value.ToLower() == "hevc" && pattern.ToLower() == "h265")
                return MatchResult.Matched; // special case

            return MatchResult.NoMatch;
        }
        catch (Exception)
        {
            return MatchResult.NoMatch;
        }
    }

    private enum MatchResult
    {
        NoMatch = 0,
        Matched = 1,
        Skipped = 2
    }
}
