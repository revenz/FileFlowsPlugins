using FileFlows.Plugin;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderStart: FfmpegBuilderNode
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override string Icon => "far fa-file-video";
        public override FlowElementType Type => FlowElementType.BuildStart;

        public override int Execute(NodeParameters args)
        {
            VideoInfo videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return -1;

            this.Model = Models.FfmpegModel.CreateModel(videoInfo);
            return 1;
        }
    }
}
