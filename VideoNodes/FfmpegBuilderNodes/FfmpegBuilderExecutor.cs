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

        public override bool NoEditorOnAdd => true;

        [DefaultValue(true)]
        [Boolean(1)]
        public bool HardwareDecoding { get; set; }

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

            startArgs.AddRange(new[] {
                "-probesize", VideoInfoHelper.ProbeSize + "M"
            });

            if (HardwareDecoding)
            {
                startArgs.AddRange(GetHardwareDecodingArgs());
            }

            foreach (string file in model.InputFiles)
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

        internal string[] GetHardwareDecodingArgs()
        {
            string testFile = Path.Combine(args.TempPath, Guid.NewGuid() + ".hwtest.mkv");
            foreach(var hw in new [] { "cuda", "dxva2", "qsv", "d3d11va", "opencl" })
            {
                // ffmpeg -y -hwaccel qsvf -f lavfi -i color=color=red -frames:v 10 test.mkv
                try
                {
                    var result = args.Execute(new ExecuteArgs
                    {
                        Command = ffmpegExe,                        
                        ArgumentList = new[]
                        {
                            "-y",
                            "-hwaccel", hw,
                            "-f", "lavfi",
                            "-i", "color=color=red",
                            "-frames:v", "10",
                            testFile
                        }
                    });
                    if (result.ExitCode == 0)
                    {
                        args.Logger?.ILog("Supported hardware decoding detected: " + hw);
                        return new[] { "-hwaccel", hw };
                    }
                }
                catch (Exception) { }
            }

            args.Logger?.ILog("No hardware decoding availble");
            return new string[] { };
        }
    }
}
