namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public abstract class FfmpegStream
    {
        public bool Deleted { get; set; }
        public int Index { get; set; }

        public abstract bool HasChange { get; }

        public abstract string[] GetParameters(int outputIndex);
    }
}
