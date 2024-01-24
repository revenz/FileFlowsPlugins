#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;
using System.IO;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_VideoEncode_VideoEncodeTests: TestBase
{
    private (int output, string log) Encode(string codec, int quality, bool hardwareEncoding, string file, string outfile)
    {
        if (File.Exists(file) == false)
            throw new FileNotFoundException(file);

        var logger = new TestLogger();
        string ffmpeg = FfmpegPath;
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(file);
        var args = new NodeParameters(file, logger, false, string.Empty, new LocalFileService());
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Codec = codec;
        ffEncode.Quality = quality;
        //ffEncode.HardwareEncoding = hardwareEncoding;
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);
        
        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);
        string log = logger.ToString();
        if(args.WorkingFile.StartsWith(args.TempPath))
            File.Move(args.WorkingFile, FileHelper.Combine(args.TempPath, outfile), true);
        Assert.AreEqual(1, result);
        return (result, log);
    }

    private void TestEncode(bool h265, bool bit10, bool hardware)
    {
        foreach (var quality in new int[] { 18, 20, 23, 28, 35, 50 })
        {
            string codec = h265 && bit10 ? FfmpegBuilderVideoEncode.CODEC_H265_10BIT :
                           h265 ? FfmpegBuilderVideoEncode.CODEC_H265 :
                           FfmpegBuilderVideoEncode.CODEC_H264;

            var result = Encode(codec, quality, hardware, TestFile_Sitcom,
                $"{(hardware ? "nvidia" : "cpu")}_h26{(h265 ? "5" : "4")}{(bit10 ? "_10bit" : "")}_{quality}.mkv");
        }

    }

    [TestMethod] 
    public void FfmpegBuilder_VideoEncode_H265_10bit_Hardware() => TestEncode(true, true, true);
    [TestMethod]
    public void FfmpegBuilder_VideoEncode_H265_Hardware() => TestEncode(true, false, true);
    [TestMethod]
    public void FfmpegBuilder_VideoEncode_H265() => TestEncode(true, false, false);
    [TestMethod]
    public void FfmpegBuilder_VideoEncode_H265_10bit() => TestEncode(true, false, true);


    [TestMethod]
    public void FfmpegBuilder_VideoEncode_H264_Hardware() => TestEncode(false, false, true);
    [TestMethod]
    public void FfmpegBuilder_VideoEncode_H264() => TestEncode(false, false, false);


    [TestMethod]
    public void FfmpegBuilder_VideoEncode_FailIfNoHardware()
    {
        var logger = new TestLogger();
        string ffmpeg = FfmpegPath;
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(TestFile_120_mbps_4k_uhd_hevc_10bit);
        var args = new NodeParameters(TestFile_50_mbps_hd_h264, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);


        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffCodec = new();
        ffCodec.VideoCodec = "h265";
        ffCodec.VideoCodecParameters = "hevc_qsv";
        ffCodec.PreExecute(args);
        ffCodec.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);
        string log = logger.ToString();
        Assert.AreEqual(-1, result);

    }

    [TestMethod]
    public void FfmpegBuilder_VideoEncode_AutoUseHardware()
    {
        var logger = new TestLogger(); 
        string ffmpeg = FfmpegPath;
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(TestFile_BasicMkv);
        var args = new NodeParameters(TestFile_BasicMkv, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);
        
        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoCodec ffCodec = new();
        ffCodec.VideoCodec = "h265";
        ffCodec.VideoCodecParameters = "h265";
        ffCodec.PreExecute(args);
        ffCodec.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);
        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }
    
    

    [TestMethod]
    public void FfmpegBuilder_VideoEncode_QSV()
    {
        var logger = new TestLogger(); 
        string ffmpeg = FfmpegPath;
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(TestFile_BasicMkv);
        var args = new NodeParameters(TestFile_BasicMkv, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);
        
        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Quality = 28;
        ffEncode.Speed = "ultrafast";
        ffEncode.Encoder = "Intel QSV";
        ffEncode.Codec = "h265";
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);

        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);
        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }
    
    
    [TestMethod]
    public void FfmpegBuilder_VideoEncode_Av1()
    {
        var logger = new TestLogger(); 
        string ffmpeg = FfmpegPath;
        var vi = new VideoInfoHelper(ffmpeg, logger);
        var vii = vi.Read(TestFile_BasicMkv);
        var args = new NodeParameters(TestFile_BasicMkv, logger, false, string.Empty, null);
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = TempPath;
        args.Parameters.Add("VideoInfo", vii);
        
        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Quality = 28;
        ffEncode.Speed = "veryslow";
        //ffEncode.Encoder = "Nvid;
        ffEncode.Codec = "av1 10BIT";
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