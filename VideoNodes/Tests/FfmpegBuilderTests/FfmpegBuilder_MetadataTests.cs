#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_MetadataTests: TestBase
{


    [TestMethod]
    public void FfmpegBuilder_Metadata_Remover_Language()
    {
        string file = TestFile_MovText_Mp4;
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


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

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_Metadata_Remover_Additional()
    {
        string file = TestFile_MovText_Mp4;
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


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

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void FfmpegBuilder_Metadata_Remover_Images()
    {
        string file = TestFile_MovText_Mp4;
        var logger = new TestLogger();
        const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"D:\videos\temp";
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));


        FfmpegBuilderMetadataRemover ffMetadata = new();
        ffMetadata.RemoveImages = true;
        ffMetadata.PreExecute(args);
        Assert.AreEqual(1, ffMetadata.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }
    
    
    [TestMethod]
    public void FfmpegBuilder_Metadata_Remover_BitrateFromConvetted()
    {
        string file = TestFile_BasicMkv;
        var logger = new TestLogger();
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        Assert.IsTrue(ffStart.PreExecute(args));
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Codec = "h265";
        ffEncode.Quality = 30;
        ffEncode.HardwareEncoding = false;
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }
}

#endif