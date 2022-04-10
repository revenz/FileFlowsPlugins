using FileFlows.Plugin;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderExecutor: FfmpegBuilderNode
    {
        public override string Icon => "far fa-file-video";
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.BuildEnd;

        public override int Execute(NodeParameters args)
        {
            this.Init(args);
            var model = this.Model;
            List<string> ffArgs = new List<string>();
            bool hasChange = false;
            int actualIndex = 0;
            int currentType = 0;
            foreach (var item in model.VideoStreams.Select((x, index) => (stream: (FfmpegStream)x, index, type: 1)).Union(
                                 model.AudioStreams.Select((x, index) => (stream: (FfmpegStream)x, index, type: 2))).Union(
                                 model.SubtitleStreams.Select((x, index) => (stream: (FfmpegStream)x, index, type: 3))))
            {
                if (item.stream.Deleted)
                {
                    hasChange = true;
                    continue;
                }
                if (currentType != item.type)
                {
                    actualIndex = 0;
                    currentType = item.type;    
                }
                ffArgs.AddRange(item.stream.GetParameters(actualIndex));
                hasChange |= item.stream.HasChange;
                ++actualIndex;
            }

            if (hasChange == false && (string.IsNullOrWhiteSpace(model.Extension) || args.WorkingFile.ToLower().EndsWith("." + model.Extension.ToLower())))
                return 2; // nothing to do 

            string extension = model.Extension?.EmptyAsNull() ?? "mkv";

            if (Encode(args, ffmpegExe, ffArgs, extension) == false)
                return -1;

            return 1;
        }
    }
}
