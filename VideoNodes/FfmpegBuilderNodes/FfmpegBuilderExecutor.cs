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

        public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder";

        public override bool NoEditorOnAdd => true;

        [DefaultValue(true)]
        [Boolean(1)]
        public bool HardwareDecoding { get; set; }

        public override int Execute(NodeParameters args)
        {
            var model = this.Model;
            List<string> ffArgs = new List<string>();

            if(model.CustomParameters?.Any() == true)
                ffArgs.AddRange(model.CustomParameters);

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

            if (model.ForceEncode == false && hasChange == false && (string.IsNullOrWhiteSpace(model.Extension) || args.WorkingFile.ToLower().EndsWith("." + model.Extension.ToLower())))
                return 2; // nothing to do 

            string extension = model.Extension?.EmptyAsNull() ?? "mkv";

            List<string> startArgs = new List<string>();
            if (model.InputFiles?.Any() == false)
                model.InputFiles.Add(args.WorkingFile);
            else
                model.InputFiles[0] = args.WorkingFile;

            startArgs.AddRange(new[] { "-strict", "-2" }); // allow experimental stuff
            startArgs.AddRange(new[] { "-fflags", "+genpts" }); //Generate missing PTS if DTS is present.

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


            if (Encode(args, FFMPEG, ffArgs, extension, dontAddInputFile: true) == false)
                return -1;

            return 1;
        }

        internal string[] GetHardwareDecodingArgs()
        {
            string testFile = Path.Combine(Args.TempPath, Guid.NewGuid() + ".hwtest.mkv");
            var video = this.Model.VideoStreams.Where(x => x.Stream.IsImage == false).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(video?.Stream?.Codec))
                return new string[] { };
            bool isH264 = video.Stream.Codec.Contains("264");
            bool isHevc = video.Stream.Codec.Contains("265") || video.Stream.Codec.ToLower().Contains("hevc");

            var decoders = isH264 ? Decoders_h264() :
                            isHevc ? Decoders_hevc() :
                            Decoders_Default();

            foreach(var hw in decoders)
            {
                try
                {
                    var arguments = new List<string>()
                    {
                        "-y",
                    };
                    arguments.AddRange(hw);
                    arguments.AddRange(new[]
                    {
                        "-f", "lavfi",
                        "-i", "color=color=red",
                        "-frames:v", "10",
                        testFile
                    });

                    var result = Args.Execute(new ExecuteArgs
                    {
                        Command = FFMPEG,                        
                        ArgumentList = arguments.ToArray()
                    });
                    if (result.ExitCode == 0)
                    {
                        Args.Logger?.ILog("Supported hardware decoding detected: " + string.Join(" ", hw));
                        return hw;
                    }
                }
                catch (Exception) { }
            }

            Args.Logger?.ILog("No hardware decoding availble");
            return new string[] { };
        }


        private string[][] Decoders_h264()
        {
            return new[]
            {
                new [] { "-hwaccel", "nvdec", "-hwaccel_output_format", "cuda" },
                new [] { "-hwaccel", "cuda" },
                new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "qsv" },
                new [] { "-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi" },
                new [] { "-hwaccel", "dxva2" },
                new [] { "-hwaccel", "d3d11va" },
                new [] { "-hwaccel", "opencl" },
            };
        }

        private string[][] Decoders_hevc()
        {
            return new[]
            {
                new [] { "-hwaccel", "nvdec", "-hwaccel_output_format", "cuda" },
                new [] { "-hwaccel", "cuda" },rep
                new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "qsv" },
                new [] { "-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi" },
                new [] { "-hwaccel", "dxva2" },
                new [] { "-hwaccel", "d3d11va" },
                new [] { "-hwaccel", "opencl" },
            };
        }

        private string[][] Decoders_Default()
        {
            return new[]
            {
                new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "cuda" },
                new [] { "-hwaccel", "cuda" },
                new [] { "-hwaccel", "qsv", "-hwaccel_output_format", "qsv" },
                new [] { "-hwaccel", "vaapi", "-hwaccel_output_format", "vaapi" },
                new [] { "-hwaccel", "dxva2" },
                new [] { "-hwaccel", "d3d11va" },
                new [] { "-hwaccel", "opencl" },
            };
        }

    }
}
