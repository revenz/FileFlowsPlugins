namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;

    public class AudioAdjustVolume: EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-volume-up";

        [NumberInt(1)]
        [Range(0, 1000)]
        public int VolumePercent { get; set; }


        public override int Execute(NodeParameters args)
        {
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;
                
                if (videoInfo.AudioStreams?.Any() != true)
                {
                    args.Logger?.ILog("No audio streams detected");
                    return 2;
                }

                if(VolumePercent == 100)
                {
                    args.Logger?.ILog("Volume percent set to 100, no adjustment necessary");
                    return 2;
                }

                List<string> ffArgs = new List<string>
                {
                    "-c", "copy",
                    "-map", "0:v",
                };

                float volume = this.VolumePercent / 100f;
                foreach (var audio in videoInfo.AudioStreams)
                {
                    ffArgs.AddRange(new[] { "-map", $"0:a:{audio.TypeIndex}", "-filter:a", $"volume={volume.ToString(".0######")}" });
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
