#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
[TestCategory("Slow")]
public class FfmpegBuilder_MetadataTests: VideoTestBase
{
    [TestMethod]
    public void FfmpegBuilder_Metadata_Remover_Language()
    {
        var args = GetVideoNodeParameters();
        var videoFile = new VideoFile();
        videoFile.PreExecute(args);
        videoFile.Execute(args);

        FfmpegBuilderStart ffStart = new ();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));


        FfmpegBuilderMetadataRemover ffMetadata = new();
        ffMetadata.RemoveLanguage = true;
        ffMetadata.Video = true;
        ffMetadata.Audio = true;
        ffMetadata.Subtitle = true;
        ffMetadata.PreExecute(args);
        Assert.AreEqual(1, ffMetadata.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_Metadata_Remover_Additional()
    {
        var args = GetVideoNodeParameters();
        var videoFile = new VideoFile();
        videoFile.PreExecute(args);
        videoFile.Execute(args);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));


        FfmpegBuilderMetadataRemover ffMetadata = new();
        ffMetadata.RemoveAdditionalMetadata = true;
        ffMetadata.PreExecute(args);
        Assert.AreEqual(1, ffMetadata.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }
    
    [TestMethod]
    public void FfmpegBuilder_Metadata_Remover_BitrateFromConvetted()
    {
        var args = GetVideoNodeParameters();
        var videoFile = new VideoFile();
        videoFile.PreExecute(args);
        videoFile.Execute(args);

        FfmpegBuilderStart ffStart = new();
        Assert.IsTrue(ffStart.PreExecute(args));
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Codec = "h265";
        ffEncode.Quality = 30;
        //ffEncode.HardwareEncoding = false;
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
    }
}

#endif