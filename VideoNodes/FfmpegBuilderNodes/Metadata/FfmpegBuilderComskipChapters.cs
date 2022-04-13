namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderComskipChapters : FfmpegBuilderNode
    {
        public override int Outputs => 2;

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

            string tempMetaDataFile = ComskipChapters.GenerateMetaDataFile(args, videoInfo);
            if (string.IsNullOrEmpty(tempMetaDataFile))
                return 2;

            Model.InputFiles.Add(tempMetaDataFile);
            Model.MetadataParameters.AddRange(new[] { "-map_metadata", (Model.InputFiles.Count - 1).ToString() });
            return 1;
        }
    }
}
