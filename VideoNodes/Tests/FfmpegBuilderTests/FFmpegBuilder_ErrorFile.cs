
#if(DEBUG)

using VideoNodes.Tests;
using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

/// <summary>
/// Tests for FFmpeg Builder for the Error file
/// </summary>
[TestClass]
public class FFmpegBuild_ErrorFile : TestBase
{
    /// <summary>
    /// Tests a subtitle using a pattern
    /// </summary>
    [TestMethod]
    public void ErrorFile()
    {
        var args = new NodeParameters(TestFile_Error, Logger, false, TestPath, new LocalFileService())
        {
            LibraryFileName = TestFile_Error
        };
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
        
        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        var ffmpegBuilderStart = new FfmpegBuilderStart();
        ffmpegBuilderStart.PreExecute(args);
        Assert.AreEqual(1, ffmpegBuilderStart.Execute(args));

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Codec = "av1 10BIT";
        ffEncode.Encoder = "qsv";
        ffEncode.Quality = 24;
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);
        
        ffEncode.PreExecute(args);
        Assert.AreEqual(1, ffEncode.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.HardwareDecoding = true;
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);
        
        Assert.AreEqual(1, result);
    }
    
}


#endif