namespace FileFlows.VideoNodes
{
    internal class ResolutionHelper
    {
        public enum Resolution
        {
            Unknown,
            r480p,
            r720p,
            r1080p,
            r1440p,
            r4k,
        }

        public static Resolution GetResolution(VideoInfo videoInfo)
        {
            var video = videoInfo?.VideoStreams?.FirstOrDefault();
            if (video == null)
                return Resolution.Unknown;
            return GetResolution(video.Width, video.Height);
        }

        public static Resolution GetResolution(int width, int height)
        {
            // so if the video is in portait mode, we test the height as if it were the width
            int w = Math.Max(width, height);
            int h = Math.Min(width, height);

            if (Between(w, 1860, 1980))
                return Resolution.r1080p;
            else if (Between(w, 2500, 2620))
                return Resolution.r1440p;
            else if (Between(w, 3780, 3900))
                return Resolution.r4k;
            else if (Between(w, 1220, 1340))
                return Resolution.r720p;
            else if (Between(w, 600, 700))
                return Resolution.r480p;

            return Resolution.Unknown;
        }


        private static bool Between(int value, int lower, int max) => value >= lower && value <= max;
    }
}
