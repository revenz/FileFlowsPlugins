#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_MetadataTests: TestBase
{
    //[TestMethod]
    //public void FfmpegBuilder_MetadataJson()
    //{
    //    const string file = @"D:\videos\unprocessed\basic.mkv";
    //    var logger = new TestLogger();
    //    const string ffmpeg = @"C:\utils\ffmpeg\ffmpeg.exe";
    //    var vi = new VideoInfoHelper(ffmpeg, logger);
    //    var vii = vi.Read(file);
    //    var args = new NodeParameters(file, logger, false, string.Empty);
    //    VideoMetadata md = System.Text.Json.JsonSerializer.Deserialize<VideoMetadata>(File.ReadAllText(@"D:\videos\metadata.json"));
    //    args.Variables.Add("VideoMetadata", md);
    //    args.GetToolPathActual = (string tool) => ffmpeg;
    //    args.TempPath = @"D:\videos\temp";
    //    args.Parameters.Add("VideoInfo", vii);


    //    FfmpegBuilderStart ffStart = new ();
    //    Assert.AreEqual(1, ffStart.Execute(args));


    //    FfmpegBuilderVideoMetadata ffMetadata = new();
    //    Assert.AreEqual(1, ffMetadata.Execute(args));

    //    FfmpegBuilderExecutor ffExecutor = new();
    //    int result = ffExecutor.Execute(args);

    //    string log = logger.ToString();
    //    Assert.AreEqual(1, result);
    //}


    [TestMethod]
    public void FfmpegBuilder_Metadata_Remover()
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

        FfmpegBuilderAudioTrackRemover ffRemover = new();
        ffRemover.StreamType = "subtitle";
        ffRemover.RemoveAll = true;
        ffRemover.PreExecute(args);
        Assert.AreEqual(1, ffRemover.Execute(args));


        FfmpegBuilderMetadataRemover ffMetadata = new();
        ffMetadata.RemoveImages = true;
        ffMetadata.RemoveTitle = true;
        ffMetadata.RemoveLanguage = true;
        ffMetadata.Video = true;
        ffMetadata.Audio= true;
        ffMetadata.Subtitle = true;
        ffMetadata.RemoveAdditionalMetadata = true;
        ffMetadata.PreExecute(args);
        Assert.AreEqual(1, ffMetadata.Execute(args));

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }
}

#endif