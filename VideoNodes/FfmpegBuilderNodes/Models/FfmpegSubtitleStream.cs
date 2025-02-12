﻿using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

public class FfmpegSubtitleStream : FfmpegStream
{
    /// <summary>
    /// Gets or sets the source subtitle stream
    /// </summary>
    public SubtitleStream Stream { get; set; }
    
    /// <summary>
    /// Gets or sets if this is a forced subtitle
    /// </summary>
    public bool IsForced { get; set; }

    /// <summary>
    /// Gets or sets if this is a Hearing Impaired subtitle
    /// </summary>
    public bool IsHearingImpaired { get; set; }

    /// <summary>
    /// Gets or sets if this stream has changed
    /// </summary>
    public override bool HasChange => false;

    /// <summary>
    /// Gets the parameters for this stream
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the parameters to pass to FFmpeg for this stream</returns>
    public override string[] GetParameters(GetParametersArgs args)
    {
        if (Deleted)
            return new string[] { };

        bool containerSame =
            string.Equals(args.SourceExtension, args.DestinationExtension, StringComparison.InvariantCultureIgnoreCase);
        
        string? destCodec;
        if (Stream.Codec?.ToLowerInvariant().Equals("mov_text") == true &&
            args.DestinationExtension?.ToLowerInvariant()?.EndsWith("mkv") == true)
        {
            args.Logger?.ILog("Force subtitle from mov_text to srt for a MKV container");
            destCodec = "srt"; // force mov_text in mkvs to be srt
        }
        else if(containerSame)
            destCodec = "copy";
        else
        {
            destCodec = SubtitleHelper.GetSubtitleCodec(args.DestinationExtension, Stream.Codec);
            if (string.IsNullOrEmpty(destCodec))
            {
                // this subtitle is not supported by the new container, remove it.
                args.Logger?.WLog($"Subtitle stream is not supported in destination container, removing: {Stream.Codec} {Stream.Title ?? string.Empty}");
                return new string[] { };
            }
        }

        if (destCodec == "copy" && Stream.Codec == "webvtt")
            destCodec = "webvtt"; // FF-1534: webvtt issue

        List<string> results= new List<string> { "-map", Stream.InputFileIndex + ":s:{sourceTypeIndex}", "-c:s:{index}", destCodec };

        if (string.IsNullOrWhiteSpace(this.Title) == false)
        {
            // first s: means stream specific, this is suppose to have :s:s
            // https://stackoverflow.com/a/21059838
            results.Add($"-metadata:s:s:{args.OutputTypeIndex}");
            results.Add($"title={(this.Title == FfmpegStream.REMOVED ? "" : this.Title)}");
        }
        if (string.IsNullOrWhiteSpace(this.Language) == false)
        {
            results.Add($"-metadata:s:s:{args.OutputTypeIndex}");
            // splitting ,; as a file was reported with a double up on languages, "eng,eng" which caused issues
            results.Add($"language={(Language == REMOVED ? "" : Language.Split(',', ';')[0])}");
        }

        if (Metadata.Any())
            results.AddRange(Metadata.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));
        
        //if (args.UpdateDefaultFlag) // we always update the default flags for subtitles FF-381
        if(this.IsDefault && this.IsForced)
            results.AddRange(new[] { "-disposition:s:" + args.OutputTypeIndex, "+default+forced" });
        else if(this.IsDefault)
            results.AddRange(new[] { "-disposition:s:" + args.OutputTypeIndex, "default" });
        else if(this.IsForced)
            results.AddRange(new[] { "-disposition:s:" + args.OutputTypeIndex, "forced" });
        else if(this.IsHearingImpaired)
            results.AddRange(new[] { "-disposition:s:" + args.OutputTypeIndex, "hearing_impaired" });
        else
            results.AddRange(new[] { "-disposition:s:" + args.OutputTypeIndex, "0" });

        return results.ToArray();
    }
    
    
    /// <summary>
    /// Converts the object to a string
    /// </summary>
    /// <returns>the string representation of stream</returns>
    public override string ToString()
    {
        // if (Stream != null)
        //     return Stream.ToString() + (Deleted ? " / Deleted" : "");

        // can be null in unit tests
        return string.Join(" / ", new string[]
        {
            Index.ToString(),
            Language,
            Codec,
            Title,
            IsDefault ? "Default" : null,
            Stream?.Forced == true ? "Forced" : null,
            Stream?.HearingImpaired == true ? "Hearing Impaired" : null,
            Deleted ? "Deleted" : null,
            HasChange ? "Changed" : null
        }.Where(x => string.IsNullOrWhiteSpace(x) == false));
    }
}