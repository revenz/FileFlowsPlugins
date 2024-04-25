#if(DEBUG)

using FileFlows.AudioNodes.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudioNodes.Tests;

public abstract class AudioTestBase
{
    private TestContext testContextInstance;

    internal TestLogger logger = new ();

    public TestContext TestContext
    {
        get { return testContextInstance; }
        set { testContextInstance = value; }
    }
    
    protected readonly string ffmpeg = (OperatingSystem.IsLinux() ? "/usr/local/bin/ffmpeg" :  @"C:\utils\ffmpeg\ffmpeg.exe");
    protected readonly string ffprobe = (OperatingSystem.IsLinux() ? "/usr/local/bin/ffprobe" :  @"C:\utils\ffmpeg\ffprobe.exe");


    protected NodeParameters GetNodeParameters(string file, bool isDirectory = false)
    {
        var args = new FileFlows.Plugin.NodeParameters(file, logger, isDirectory, string.Empty, new LocalFileService());
        
        args.GetToolPathActual = (string tool) =>
        {
            if (tool.ToLowerInvariant() == "ffmpeg")
                return ffmpeg;
            if (tool.ToLowerInvariant() == "ffprobe")
                return ffprobe;
            return null;
        };
        args.TempPath = @"/home/john/Music/temp";

        return args;
    }
}

#endif