namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderRemuxToMkv: FfmpegBuilderNode
    {
        public override int Execute(NodeParameters args)
        {
            base.Init(args);
            this.Model.Extension = "mkv";
            return 1;
        }
    }
}
