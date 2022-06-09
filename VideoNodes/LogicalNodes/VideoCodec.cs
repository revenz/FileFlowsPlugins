namespace FileFlows.VideoNodes
{
    using System.Linq;
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class VideoCodec : VideoNode
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Logic;

        public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/logical-nodes/video-codec";

        [StringArray(1)]
        [Required]
        public string[] Codecs { get; set; }

        public override int Execute(NodeParameters args)
        {
            var videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return -1;

            var codec = videoInfo.VideoStreams.FirstOrDefault(x => Codecs.Contains(x.Codec.ToLower()));
            if (codec != null)
            {
                args.Logger.ILog($"Matching video codec found[{codec.Index}]: {codec.Codec}");
                return 1;
            }

            var acodec = videoInfo.AudioStreams.FirstOrDefault(x => Codecs.Contains(x.Codec.ToLower()));
            if (acodec != null)
            {
                args.Logger.ILog($"Matching audio codec found[{acodec.Index}]: {acodec.Codec}, language: {acodec.Language}");
                return 1;
            }

            // not found, execute 2nd outputacodec
            return 2;
        }
    }
}