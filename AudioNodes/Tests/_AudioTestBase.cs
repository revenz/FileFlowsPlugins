#if(DEBUG)

using File = System.IO.File;

namespace FileFlows.AudioNodes.Tests;

/// <summary>
/// Test base dor the audio tests
/// </summary>
public abstract class AudioTestBase : TestBase
{
    
    /// <summary>
    /// The resources test file directory
    /// </summary>
    protected static readonly string ResourcesTestFilesDir = "Tests/Resources";
    
    /// <summary>
    /// Audio MP3 file
    /// </summary>
    protected static readonly string AudioMp3 = ResourcesTestFilesDir + "/audio.mp3";

    /// <summary>
    /// Audio OGG file
    /// </summary>
    protected static readonly string AudioOgg = ResourcesTestFilesDir + "/audio.ogg";

    /// <summary>
    /// Audio FLAC file
    /// </summary>
    protected static readonly string AudioFlac = ResourcesTestFilesDir + "/audio.flac";

    /// <summary>
    /// Audio WAV file
    /// </summary>
    protected static readonly string AudioWav = ResourcesTestFilesDir + "/audio.wav";
    
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
    /// <param name="filename">the file to initialise, will use AudioMp3 if not set</param>
    /// <param name="isDirectory">if the file is directory
    /// <returns>the node parameters</returns>
    public NodeParameters GetAudioNodeParameters(string? filename = null, bool isDirectory = false)
    {
        filename ??= AudioMp3;
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