namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;

    public class AudioTrackReorder: EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-volume-off";

        [StringArray(1)]
        public List<string> Languages { get; set; }

        [StringArray(2)]
        public List<string> OrderedTracks { get; set; }

        public List<AudioStream> Reorder(List<AudioStream> input)
        {
            Languages ??= new List<string>();
            OrderedTracks ??= new List<string>();
            if (Languages.Count == 0 && OrderedTracks.Count == 0)
                return input; // nothing to do

            int count = 1_000_000_000;
            var debug = new StringBuilder();
            var data = input.OrderBy(x =>
            {
                int langIndex = Languages.IndexOf(x.Language?.ToLower() ?? String.Empty);
                int codecIndex = OrderedTracks.IndexOf(x.Codec?.ToLower() ?? String.Empty);
                int result = 0;
                if (langIndex >= 0)
                {
                    if (codecIndex >= 0)
                        result = (langIndex * 10_000) + (codecIndex * 100) + x.Index;
                    else
                        result = ((langIndex + 1) * 10_000) - 100 + x.Index;
                }
                else if (codecIndex >= 0)
                    result = ((codecIndex + 1) * 10_000_000) + x.Index;
                else
                    result = ++count;
                debug.AppendLine(x.Codec + " , " + x.Language + " = " + result);
                return result;
            }).ToList();

            
            return data;

        }

        public bool AreSame(List<AudioStream> original, List<AudioStream> reordered)
        {
            for (int i = 0; i < reordered.Count; i++)
            {
                if (reordered[i] != original[i])
                {
                    return false;
                }
            }
            return true;
        }

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

                var reordered = Reorder(videoInfo.AudioStreams);

                bool same = AreSame(videoInfo.AudioStreams, reordered);

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
