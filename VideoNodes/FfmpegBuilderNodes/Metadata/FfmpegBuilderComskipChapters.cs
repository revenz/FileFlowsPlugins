using System.Text;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderComskipChapters : FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/comskip-chapters";
    public override int Outputs => 2;

    public override int Execute(NodeParameters args)
    {
        VideoInfo videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
            return -1;

        if (videoInfo.Chapters?.Count > 3)
        {
            args.Logger.ILog(videoInfo.Chapters.Count + " chapters already detected in file");
            return 2;
        }

        string tempMetaDataFile = GenerateMetaDataFile(args, videoInfo);
        if (string.IsNullOrEmpty(tempMetaDataFile))
            return 2;

        Model.InputFiles.Add(tempMetaDataFile);
        Model.MetadataParameters.AddRange(new[] { "-map_metadata", (Model.InputFiles.Count - 1).ToString() });
        return 1;
    }

    string GenerateMetaDataFile(NodeParameters args, VideoInfo videoInfo)
    {
        float totalTime = (float)videoInfo.VideoStreams[0].Duration.TotalSeconds;

        string edlFile = args.WorkingFile.Substring(0, args.WorkingFile.LastIndexOf(".") + 1) + "edl";
        if (File.Exists(edlFile) == false)
            edlFile = args.WorkingFile.Substring(0, args.WorkingFile.LastIndexOf(".") + 1) + "edl";
        if (File.Exists(edlFile) == false)
        {
            args.Logger?.ILog("No EDL file found for file");
            return string.Empty;
        }

        string text = File.ReadAllText(edlFile) ?? string.Empty;
        float last = 0;

        StringBuilder metadata = new StringBuilder();
        metadata.AppendLine(";FFMETADATA1");
        metadata.AppendLine("");
        int chapter = 0;

        foreach (string line in text.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
        {
            // 93526.47 93650.13 0
            string[] parts = line.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                continue;
            float start = 0;
            float end = 0;
            if (float.TryParse(parts[0], out start) == false || float.TryParse(parts[1], out end) == false)
                continue;

            if (start < last)
                continue;

            AddChapter(last, start);
            last = end;
        }

        if (chapter == 0)
        {
            args.Logger?.ILog("No ads found in edl file");
            return string.Empty;
        }
        AddChapter(last, totalTime);

        string tempMetaDataFile = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + ".txt");
        File.WriteAllText(tempMetaDataFile, metadata.ToString());
        return tempMetaDataFile;

        void AddChapter(float start, float end)
        {

            metadata.AppendLine("[CHAPTER]");
            metadata.AppendLine("TIMEBASE=1/1000");
            metadata.AppendLine("START=" + ((int)(start * 1000)));
            metadata.AppendLine("END=" + ((int)(end * 1000)));
            metadata.AppendLine("title=Chapter " + (++chapter));
            metadata.AppendLine();
        }

    }
}
