namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderAutoChapters : FfmpegBuilderNode
    {
        public override int Outputs => 2;

        [NumberInt(1)]
        [DefaultValue(60)]
        public int MinimumLength { get; set; } = 60;

        [NumberInt(2)]
        [DefaultValue(45)]
        public int Percent { get; set; } = 45;

        public override int Execute(NodeParameters args)
        {
            base.Init(args);

            VideoInfo videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return -1;

            if (videoInfo.Chapters?.Count > 3)
            {
                args.Logger.ILog(videoInfo.Chapters.Count + " chapters already detected in file");
                return 2;
            }

            string tempMetaDataFile = AutoChapters.GenerateMetaDataFile(this, args, videoInfo, ffmpegExe, this.Percent, this.MinimumLength);
            if (string.IsNullOrEmpty(tempMetaDataFile))
                return 2;

            Model.InputFiles.Add(tempMetaDataFile);
            Model.MetadataParameters.AddRange(new[] { "-map_metadata", (Model.InputFiles.Count - 1).ToString() });
            return 1;
        }
    }
}
