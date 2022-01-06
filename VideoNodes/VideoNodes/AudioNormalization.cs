namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;

    public class AudioNormalization: EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-volume-up";

        [Boolean(1)]
        public bool AllAudio { get; set; }


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

                if (videoInfo.AudioStreams?.Any() != true)
                {
                    args.Logger?.ILog("No audio streams detected");
                    return 2;
                }

                //for (int i = 0; i < videoInfo.AudioStreams.Count; i++)
                //{
                //    if (i > 0 && AllAudio == false)
                //        break;

                    List<string> ffArgs = new List<string>();
                    if(videoInfo.VideoStreams?.Any() == true)
                        ffArgs.Add($"-map 0:v");

                    for (int j = 0; j < videoInfo.AudioStreams.Count;j++)
                    {
                        var audio = videoInfo.AudioStreams[j];
                        if (AllAudio || j == 0)
                        {
                            int sampleRate = audio.SampleRate > 0 ? audio.SampleRate : 48_000;
                            ffArgs.Add($"-map 0:{audio.Index} -c:a {audio.Codec} -ar {sampleRate} -af loudnorm=I=-24:LRA=7:TP=-2.0");
                        }
                        else
                            ffArgs.Add($"-map 0:{audio.Index} -c copy");
                    }

                    if(videoInfo.SubtitleStreams?.Any() == true)
                        ffArgs.Add("-map 0:s -c copy");

                    string ffArgsLine = string.Join(" ", ffArgs);

                    string extension = new FileInfo(args.WorkingFile).Extension;
                    if (extension.StartsWith("."))
                        extension = extension.Substring(1);

                    if (Encode(args, ffmpegExe, ffArgsLine, extension) == false)
                        return -1;

                //}
               

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
