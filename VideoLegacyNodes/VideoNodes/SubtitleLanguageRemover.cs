namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class SubtitleLanguageRemover: EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-comment";


        [TextVariable(1)]
        public string Pattern { get; set; }

        [Boolean(2)]
        public bool NotMatching { get; set; }

        [Boolean(3)]
        public bool UseTitle { get; set; }


        public override int Execute(NodeParameters args)
        {
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                List<string> ffArgs = new List<string>()
                {
                    "-map", "0:v",
                    "-map", "0:a",
                };

                bool removing = false;
                var regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
                for (int i = 0; i < videoInfo.SubtitleStreams.Count; i++)
                {
                    var sub = videoInfo.SubtitleStreams[i];
                    string str = UseTitle ? sub.Title : sub.Language;
                    if (string.IsNullOrEmpty(str) == false) // if empty we always use this since we have no info to go on
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
                    ffArgs.AddRange(new[] { "-map", "0:s:" + i });
                }

                if(removing == false)
                {
                    // nothing to remove
                    return 2;
                }
                ffArgs.AddRange(new[] { "-c", "copy" });


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
