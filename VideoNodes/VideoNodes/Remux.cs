namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using System;

    public class RemuxToMKV: EncodingNode
    {

        public override int Outputs => 1;
        public override string Icon => "far fa-file-video";

        public override int Execute(NodeParameters args)
        {
            string ffmpegExe = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                return -1;

            try 
            { 
                if (Encode(args, ffmpegExe, "-c copy -map 0", "mkv") == false)
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

    public class RemuxToMP4 : EncodingNode
    {

        public override int Outputs => 1;
        public override string Icon => "far fa-file-video";

        public override int Execute(NodeParameters args)
        {
            string ffmpegExe = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                return -1;

            try
            {
                if (Encode(args, ffmpegExe, "-c copy -map 0", "mp4") == false)
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
