namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public abstract class EncodingNode : VideoNode
    {
        public override int Outputs => 2;
        public override int Inputs => 1;
        public override FlowElementType Type => FlowElementType.Process;

        protected TimeSpan TotalTime;

        private NodeParameters args;

        private FFMpegEncoder Encoder;

        protected bool Encode(NodeParameters args, string ffmpegExe, string ffmpegParameters)
        {
            this.args = args;
            Encoder = new FFMpegEncoder(ffmpegExe, args.Logger);
            Encoder.AtTime += AtTimeEvent;

            string output = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + ".mkv");
            args.Logger.DLog("New Temp file: " + output);

            bool success = Encoder.Encode(args.WorkingFile, output, ffmpegParameters);
            if (success)
                args.SetWorkingFile(output);
            Encoder.AtTime -= AtTimeEvent;
            Encoder = null;
            return success;
        }

        public override Task Cancel()
        {
            if (Encoder != null)
                Encoder.Cancel();
            return base.Cancel();
        }

        void AtTimeEvent(TimeSpan time)
        {
            if (TotalTime.TotalMilliseconds == 0)
                return;
            float percent = (float)((time.TotalMilliseconds / TotalTime.TotalMilliseconds) * 100);
            args.PartPercentageUpdate(percent);
        }
    }
}