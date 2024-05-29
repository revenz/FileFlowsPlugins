#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VideoNodes.Tests;

[TestClass]
public class VideoHasErrorsTests : TestBase
{
    [TestMethod]
    public void VideoHasErrors_Video()
    {
        string file = TestFile_Corrupt;
        var args = new NodeParameters(file, Logger, false, string.Empty, new LocalFileService());
        args.GetToolPathActual = (string tool) =>
        {
            if(tool.ToLowerInvariant() == "ffmpeg") return FfmpegPath;
            if(tool.ToLowerInvariant() == "ffprobe") return FfprobePath;
            return null;
        };
        args.TempPath = TempPath;

        VideoFile vf = new();
        vf.PreExecute(args);
        vf.Execute(args);
        
        VideoHasErrors element = new();
        element.PreExecute(args);
        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }
}


#endif