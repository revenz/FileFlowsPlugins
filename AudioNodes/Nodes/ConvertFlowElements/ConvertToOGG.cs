namespace FileFlows.AudioNodes;

    public class ConvertToOGG: ConvertNode
    {
        /// <inheritdoc />
        public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/convert-to-ogg";
        
        /// <inheritdoc />
        protected override string Extension => "ogg";
        
        public static List<ListOption> BitrateOptions => ConvertNode.BitrateOptions;
        
        /// <inheritdoc />
        public override string Icon => "svg:ogg";
        
        /// <summary>
        /// Gets or sets the codec
        /// </summary>
        [Select(nameof(CodecOptions), 0)]
        public string Codec { get; set; }

        /// <summary>
        /// The codec options
        /// </summary>
        private static List<ListOption> _CodecOptions;
        /// <summary>
        /// Gets the codec options
        /// </summary>
        public static List<ListOption> CodecOptions
        {
            get
            {
                if (_CodecOptions == null)
                {
                    _CodecOptions = new()
                    {
                        new() { Label = "Vorbis",Value =  "vorbis" },
                        new() { Label = "Opus", Value = "opus" }
                    };
                }

                return _CodecOptions;
            }
        }

        /// <inheritdoc />
        protected override List<string> GetArguments(NodeParameters args, out string? extension)
        {
            List<string> ffArgs = base.GetArguments(args, out extension);
            if (string.Equals(this.Codec, "opus", StringComparison.InvariantCultureIgnoreCase))
            {
                var index = ffArgs.FindIndex(arg => arg.Equals("libvorbis"));
                if (index != -1)
                {
                    ffArgs[index] = "libopus";
                    args.Logger?.ILog("Replace 'libopus' with 'libvorbis'");
                }
                else
                {
                    args.Logger?.ILog("Failed to locate libopus in arguments");
                }
            }

            return ffArgs;
        }
    }