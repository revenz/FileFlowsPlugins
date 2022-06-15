namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class AudioTrackRemover : EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-volume-off";

        [TextVariable(1)]
        public string Pattern { get; set; }

        [Boolean(2)]
        public bool NotMatching { get; set; }

        [Boolean(3)]
        public bool UseLanguageCode { get; set; }

        public override int Execute(NodeParameters args)
        {
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                List<string> ffArgs = new List<string>
                {
                    "-c", "copy",
                    "-map", "0:v",
                };

                bool removing = false;
                var regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
                for(int i=0;i< videoInfo.AudioStreams.Count;i++)
                {
                    var audio = videoInfo.AudioStreams[i];
                    string str = UseLanguageCode ? audio.Language : audio.Title;                    
                    if(string.IsNullOrEmpty(str)  == false) // if empty we always use this since we have no info to go on
                    {
                        bool matches = regex.IsMatch(str);
                        if (NotMatching)
                            matches = !matches;
                        if (matches)
                        {
                            removing = true;
                            continue;
                        }
                    }

                    ffArgs.AddRange(new[] { "-map", "0:a:" + i });
                }

                if(removing == false)
                {
                    args.Logger.ILog("Nothing found to remove");
                    return 2;
                }

                if (videoInfo.SubtitleStreams?.Any() == true)
                    ffArgs.AddRange(new[] { "-map", "0:s" });

                string extension = new FileInfo(args.WorkingFile).Extension;
                if(extension.StartsWith("."))
                    extension = extension.Substring(1); 

                if (Encode(args, FFMPEG, ffArgs, extension) == false)
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
