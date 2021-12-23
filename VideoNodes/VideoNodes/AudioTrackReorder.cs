namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class AudioTrackReorder: EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-volume-off";

        [StringArray(1)]
        [Required]
        public List<string> OrderedTracks { get; set; }

        public override int Execute(NodeParameters args)
        {
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                List<string> ffArgs = new List<string>();
                ffArgs.Add($"-map 0:v");

                OrderedTracks = OrderedTracks?.Select(x => x.ToLower())?.ToList() ?? new ();

                int count = 100_000;
                var reordered = videoInfo.AudioStreams.OrderBy(x =>
                {
                    int index = OrderedTracks.IndexOf(x.Codec?.ToLower() ?? String.Empty);
                    if (index >= 0)
                    {
                        // incase there are multiple tracks with the same codedc
                        // eg ac3, ac3, dts, truehd
                        return (index * 100) + x.Index;
                    }
                    return ++count;
                }).ToList();

                bool same = true;
                for(int i = 0; i < reordered.Count; i++)
                {
                    if (reordered[i] != videoInfo.AudioStreams[i])
                    {
                        same = false;
                        break;
                    }
                }
                if(same)
                {
                    args.Logger?.ILog("No audio tracks need reordering");
                    return 2;
                }

                foreach (var audio in reordered)
                {
                    ffArgs.Add($"-map " + audio.IndexString);
                }

                ffArgs.Add("-map 0:s -c copy");
                // this makes the first audio track now the default track
                ffArgs.Add("-disposition:a:0 default");

                string ffArgsLine = string.Join(" ", ffArgs);

                string extension = new FileInfo(args.WorkingFile).Extension;
                if(extension.StartsWith("."))
                    extension = extension.Substring(1); 

                if (Encode(args, ffmpegExe, ffArgsLine, extension) == false)
                    return -1;

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }
}
    }
}
