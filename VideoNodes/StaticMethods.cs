namespace FileFlows.VideoNodes;

/// <summary>
/// Special class that exposes static methods to the script executor
/// </summary>
public class StaticMethods
{
    /// <summary>
    /// Gets the video info for a file
    /// </summary>
    /// <param name="args">the args</param>
    /// <param name="filename">the name of the file to read</param>
    /// <returns>the video info</returns>
    public static VideoInfo GetVideoInfo(NodeParameters args, string filename)
        => VideoInfoHelper.ReadStatic(args.Process, args.Logger, args.GetToolPath("FFMpeg"), filename).ValueOrDefault;
}