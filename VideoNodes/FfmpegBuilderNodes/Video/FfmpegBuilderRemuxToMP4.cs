namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderRemuxToMP4: FfmpegBuilderNode
    {
        public override int Execute(NodeParameters args)
        {
            base.Init(args);
            this.Model.Extension = "mp4";
            return 1;
        }
    }
}
