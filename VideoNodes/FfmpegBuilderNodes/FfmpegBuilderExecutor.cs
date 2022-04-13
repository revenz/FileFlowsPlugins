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
            ffArgs.AddRange(new[] { "-strict", "-2" }); // allow experimental stuff
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
                hasChange |= item.stream.HasChange | item.stream.ForcedChange;
                ++actualIndex;
            }

            if(model.MetadataParameters?.Any() == true)
            {
                hasChange = true;
                ffArgs.AddRange(model.MetadataParameters);
            }

            if (hasChange == false && (string.IsNullOrWhiteSpace(model.Extension) || args.WorkingFile.ToLower().EndsWith("." + model.Extension.ToLower())))
                return 2; // nothing to do 

            string extension = model.Extension?.EmptyAsNull() ?? "mkv";

            List<string> startArgs = new List<string>();
            if (model.InputFiles?.Any() == false)
                model.InputFiles.Add(args.WorkingFile);
            else
                model.InputFiles[0] = args.WorkingFile;

            foreach(string file in model.InputFiles)
            {
                startArgs.Add("-i");
                startArgs.Add(file);
            }
            startArgs.Add("-y");
            ffArgs = startArgs.Concat(ffArgs).ToList();


            if (Encode(args, ffmpegExe, ffArgs, extension, dontAddInputFile: true) == false)
                return -1;

            return 1;
        }
    }
}
