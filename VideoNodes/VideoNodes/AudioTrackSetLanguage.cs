namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class AudioTrackSetLanguage : EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-comment-dots";

        [Required]
        [Text(1)]
        public string Language { get; set; }

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

                int index = 0;
                foreach(var at in videoInfo.AudioStreams)
                {
                    if (string.IsNullOrEmpty(at.Language))
                    {
                        ffArgs.Add($"-metadata:s:a:{index} language={Language.ToLower()}");
                    }
                    ++index;
                }
                if (ffArgs.Count == 0)
                    return 2; // nothing to do 


                ffArgs.Insert(0, $"-map 0 -c copy");

                string ffArgsLine = string.Join(" ", ffArgs);

                string extension = new FileInfo(args.WorkingFile).Extension;
                if(extension.StartsWith("."))
                    extension = extension.Substring(1);
                args.Logger?.DLog("Working file: " + args.WorkingFile);
                args.Logger?.DLog("Extension: " + extension);

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
