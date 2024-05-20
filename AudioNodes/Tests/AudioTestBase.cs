#if(DEBUG)

using FileFlows.AudioNodes.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudioNodes.Tests;

public abstract class AudioTestBase : TestBase
{
    protected NodeParameters GetNodeParameters(string file, bool isDirectory = false)
    {
        var args = new FileFlows.Plugin.NodeParameters(file, Logger, isDirectory, string.Empty, new LocalFileService());
        
        args.GetToolPathActual = (string tool) =>
        {
            if (tool.ToLowerInvariant() == "ffmpeg")
                return ffmpeg;
            if (tool.ToLowerInvariant() == "ffprobe")
                return ffprobe;
            return null;
        };
        args.TempPath = TempPath;

        return args;
    }
}

#endif