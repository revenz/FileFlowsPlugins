//namespace FileFlows.VideoNodes.FfmpegBuilderNodes
//{
//    public class FfmpegBuilderUnsupportedMP4Subtitles : FfmpegBuilderNode
//    {
//        public override string Icon => "fas fa-comment";

//        public override int Outputs => 2; 

//        public override int Execute(NodeParameters args)
//        {
//            this.Init(args);
//            bool removing = false;
//            string[] unsupported = new[] { "" };
//            foreach (var stream in Model.SubtitleStreams)
//            {
//                if (unsupported.Contains(stream.Stream.Codec?.ToLower()))
//                {
//                    stream.Deleted = true;
//                    removing = true;
//                }
//            }
//            return removing ? 1 : 2;
//        }
//    }
//}
