using FileFlows.Plugin;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Runtime.InteropServices;

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
            if(model == null)
            {
                args.Logger.ELog("FFMPEG Builder model is null");
                return -1;
            }
            else if (model.VideoInfo == null)
            {
                args.Logger.ELog("FFMPEG Builder VideoInfo is null");
                return -1;
            }
            else if (model.VideoInfo.FileName == null)
            {
                args.Logger.ELog("FFMPEG Builder VideoInfo Filename is null");
                return -1;
            }
            List<string> ffArgs = new List<string>();

            if(model.CustomParameters?.Any() == true)
                ffArgs.AddRange(model.CustomParameters);

            bool hasChange = false;
            int actualIndex = 0;
            int overallIndex = 0;
            int currentType = 0;

            string sourceExtension = model.VideoInfo.FileName.Substring(model.VideoInfo.FileName.LastIndexOf(".") + 1).ToLower();
            string extension = (model.Extension?.EmptyAsNull() ?? "mkv").ToLower();

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

                VideoFileStream vfs = item.stream is FfmpegVideoStream ? ((FfmpegVideoStream)item.stream).Stream :
                    item.stream is FfmpegAudioStream ? ((FfmpegAudioStream)item.stream).Stream :
                    ((FfmpegSubtitleStream)item.stream).Stream;


                var streamArgs = item.stream.GetParameters(new FfmpegStream.GetParametersArgs()
                {
                    OutputOverallIndex = overallIndex,
                    OutputTypeIndex = actualIndex,
                    SourceExtension = sourceExtension,
                    DestinationExtension = extension
                });
                for(int i = 0; i < streamArgs.Length; i++)
                {
                    streamArgs[i] = streamArgs[i].Replace("{sourceTypeIndex}", vfs.TypeIndex.ToString());
                    streamArgs[i] = streamArgs[i].Replace("{index}", actualIndex.ToString());
                }
                    
                ffArgs.AddRange(streamArgs);
                hasChange |= item.stream.HasChange | item.stream.ForcedChange;
                ++actualIndex;
                ++overallIndex;
            }

            if(model.MetadataParameters?.Any() == true)
            {
                hasChange = true;
                ffArgs.AddRange(model.MetadataParameters);
            }

            if (model.ForceEncode == false && hasChange == false && (string.IsNullOrWhiteSpace(model.Extension) || args.WorkingFile.ToLower().EndsWith("." + model.Extension.ToLower())))
                return 2; // nothing to do 


            List<string> startArgs = new List<string>();
            if (model.InputFiles?.Any() == false)
                model.InputFiles.Add(args.WorkingFile);
            else
                model.InputFiles[0] = args.WorkingFile;

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
            if (extension.ToLower() == "mp4" && ffArgs.IndexOf("-movflags") < 0 && startArgs.IndexOf("-movflgs") < 0)
            {
                startArgs.AddRange(new[] { "-movflags", "+faststart" });
            }
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
                if (hw == null)
                    continue;
                if (CanUseHardwareEncoding.DisabledByVariables(Args, string.Join(" ", hw)))
                    continue;
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

        private static readonly bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);


        private string[][] Decoders_h264()
        {
            return new[]
            {
                //new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "cuda" }, // this fails with Impossible to convert between the formats supported by the filter 'Parsed_crop_0' and the filter 'auto_scale_0'
                IsMac ? new [] { "-hwaccel", "videotoolbox" } : null,
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
                //new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "cuda" }, // this fails with Impossible to convert between the formats supported by the filter 'Parsed_crop_0' and the filter 'auto_scale_0'
                IsMac ? new [] { "-hwaccel", "videotoolbox" } : null,
                new [] { "-hwaccel", "cuda" },
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
                //new [] { "-hwaccel", "cuda", "-hwaccel_output_format", "cuda" }, // this fails with Impossible to convert between the formats supported by the filter 'Parsed_crop_0' and the filter 'auto_scale_0'
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
