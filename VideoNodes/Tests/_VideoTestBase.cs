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
    /// Video 4:7 MKV file
    /// </summary>
    protected static readonly string Video4by7 = ResourcesTestFilesDir + "/video4by7.mkv";

    /// <summary>
    /// Video 4:3 mp4 file
    /// </summary>
    protected static readonly string Video4by3 = ResourcesTestFilesDir + "/video4by3.mp4";
    
    /// <summary>
    /// Video Corrupt file
    /// </summary>
    protected static readonly string VideoCorrupt = ResourcesTestFilesDir + "/corrupt.mkv";

    /// <summary>
    /// Video with many subtitles file
    /// </summary>
    protected static readonly string VideoSubtitles = ResourcesTestFilesDir + "/subtitles.mkv";

    /// <summary>
    /// Video with english and german audio
    /// </summary>
    protected static readonly string VideoEngGerAudio = ResourcesTestFilesDir + "/eng_ger_audio.mp4";
    
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

        FFmpeg = "ffmpeg";
        FFprobe = "ffprobe";
        foreach (var ffmpegLoc in new string[] { "/tools/ffmpeg/", "/usr/local/bin/", "/usr/bin/" })
        {
            if (File.Exists(ffmpegLoc + "ffmpeg"))
            {
                FFmpeg = ffmpegLoc + "ffmpeg";
                FFprobe = ffmpegLoc + "ffprobe";
                break;
            }
        }
        
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