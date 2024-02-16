using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Text;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderComskipChapters : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/comskip-chapters";
    public override int Outputs => 2;
    
    
    /// <summary>
    /// Gets or sets if comskip should be run if no EDL file is found
    /// </summary>
    [Boolean(1)]
    public bool RunComskipIfNoEdl { get; set; }

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

        Model.InputFiles.Add(new InputFile(tempMetaDataFile));
        Model.MetadataParameters.AddRange(new[] { "-map_metadata", (Model.InputFiles.Count - 1).ToString() });
        return 1;
    }

    string GenerateMetaDataFile(NodeParameters args, VideoInfo videoInfo)
    {
        float totalTime = (float)videoInfo.VideoStreams[0].Duration.TotalSeconds;

        var edlFile = GetLocalEdlFile(args);
        if (edlFile.IsFailed)
        {
            args.Logger.ILog(edlFile.Error);
            if (RunComskipIfNoEdl == false)
                return string.Empty;
            var csResult = ComskipHelper.RunComskip(args, args.FileService.GetLocalPath(args.WorkingFile));
            if (csResult.Failed(out string error))
            {
                args.Logger.ILog(error);
                return string.Empty;
            }

            edlFile = csResult;
            args.Logger?.ILog("Created EDL File: " + edlFile);
        }

        string text = System.IO.File.ReadAllText(edlFile) ?? string.Empty;
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

        string tempMetaDataFile = System.IO.Path.Combine(args.TempPath, Guid.NewGuid() + ".txt");
        System.IO.File.WriteAllText(tempMetaDataFile, metadata.ToString());
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

    /// <summary>
    /// Gets the edl file to use locally if remote
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the edl file</returns>
    private Result<string> GetLocalEdlFile(NodeParameters args)
    {
        string edlFile = args.WorkingFile.Substring(0, args.WorkingFile.LastIndexOf(".", StringComparison.Ordinal) + 1) + "edl";
        if (args.FileService.FileExists(edlFile))
            return args.FileService.GetLocalPath(edlFile);
        
        edlFile = args.WorkingFile.Substring(0, args.WorkingFile.LastIndexOf(".", StringComparison.Ordinal) + 1) + "edl";
        if (args.FileService.FileExists(edlFile))
            return args.FileService.GetLocalPath(edlFile);
        
        return Result<string>.Fail("No EDL file found for file");
    }
}
