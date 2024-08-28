#if(DEBUG)

using PluginTestLibrary;
using File = System.IO.File;

namespace VideoNodes.Tests;

/// <summary>
/// Test base dor the video tests
/// </summary>
public abstract class VideoTestBase : TestBase
{
    
    /// <summary>
    /// The resources test file directory
    /// </summary>
    protected static readonly string ResourcesTestFilesDir = "Tests/Resources";
    
    /// <summary>
    /// Video MKV file
    /// </summary>
    protected static readonly string VideoMkv = ResourcesTestFilesDir + "/video.mkv";

    /// <summary>
    /// Video MP4 file
    /// </summary>
    protected static readonly string VideoMp4 = ResourcesTestFilesDir + "/video.mp4";

    /// <summary>
    /// Video HEVC MKV file
    /// </summary>
    protected static readonly string VideoMkvHevc = ResourcesTestFilesDir + "/hevc.mkv";

    /// <summary>
    /// Video Corrutp file
    /// </summary>
    protected static readonly string VideoCorrupt = ResourcesTestFilesDir + "/corrupt.mkv";
    
    /// <summary>
    /// Audio MP3 file
    /// </summary>
    protected static readonly string AudioMp3 = ResourcesTestFilesDir + "/audio.mp3";

    
    /// <summary>
    /// Gets the FFmpeg location
    /// </summary>
    protected static string FFmpeg { get; private set; }

    /// <summary>
    /// Gets the FFprobe location
    /// </summary>
    protected static string FFprobe { get; private set; }
    
    /// <summary>
    /// Gets the Node Parameters
    /// </summary>
    /// <param name="filename">the file to initialise, will use VideoMkv if not set</param>
    /// <param name="isDirectory">if the file is directory
    /// <returns>the node parameters</returns>
    public NodeParameters GetVideoNodeParameters(string? filename = null, bool isDirectory = false)
    {
        filename ??= VideoMkv;
        var args = new NodeParameters(filename, Logger, isDirectory, string.Empty, new LocalFileService())
        {
            LibraryFileName = filename
        };
        args.InitFile(filename);

        FFmpeg = File.Exists("/usr/local/bin/ffmpeg") ? "/usr/local/bin/ffmpeg" : "ffmpeg";
        FFprobe = File.Exists("/usr/local/bin/ffprobe") ? "/usr/local/bin/ffprobe" : "ffprobe";
        
        args.GetToolPathActual = (tool) =>
        {
            if(tool.ToLowerInvariant().Contains("ffmpeg")) return FFmpeg;
            if(tool.ToLowerInvariant().Contains("ffprobe")) return FFprobe;
            return tool;
        };
        args.TempPath = TempPath;
        return args;
    }
}

#endif