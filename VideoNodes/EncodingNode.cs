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

        protected bool Encode(NodeParameters args, string ffmpegExe, string ffmpegParameters, string extension = "mkv", string outputFile = "")
        {
            if (string.IsNullOrEmpty(extension))
                extension = "mkv";

            this.args = args;
            Encoder = new FFMpegEncoder(ffmpegExe, args.Logger);
            Encoder.AtTime += AtTimeEvent;

            if (string.IsNullOrEmpty(outputFile))
                outputFile = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + "." + extension);

            bool success = Encoder.Encode(args.WorkingFile, outputFile, ffmpegParameters);
            args.Logger.ILog("Encoding succesful: " + success);
            if (success)
                args.SetWorkingFile(outputFile);
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