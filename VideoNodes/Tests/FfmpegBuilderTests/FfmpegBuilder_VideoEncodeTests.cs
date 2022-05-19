#if(DEBUG)

using FileFlows.VideoNodes.FfmpegBuilderNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_VideoEncode_VideoEncodeTests: TestBase
{
    private (int output, string log) Encode(string codec, int quality, bool hardwareEncoding, string file, string outfile)
    {
        if (File.Exists(file) == false)
            throw new FileNotFoundException(file);

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

        FfmpegBuilderVideoEncode ffEncode = new();
        ffEncode.Codec = codec;
        ffEncode.Quality = quality;
        ffEncode.HardwareEncoding = hardwareEncoding;
        ffEncode.PreExecute(args);
        ffEncode.Execute(args);
        
        FfmpegBuilderExecutor ffExecutor = new();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);
        string log = logger.ToString();
        if(args.WorkingFile.StartsWith(args.TempPath))
            File.Move(args.WorkingFile, Path.Combine(args.TempPath, outfile), true);
        Assert.AreEqual(1, result);
        return (result, log);
    }

    private void TestEncode(bool h265, bool bit10, bool hardware)
    {
        foreach (var quality in new int[] { 18, 20, 23, 28, 35, 50 })
        {
            string codec = h265 && bit10 ? FfmpegBuilderVideoEncode.CODEC_H265_10BIT :
                           h265 ? FfmpegBuilderVideoEncode.CODEC_H265 :
                           bit10 ? FfmpegBuilderVideoEncode.CODEC_H264_10BIT :
                           FfmpegBuilderVideoEncode.CODEC_H264;

            var result = Encode(codec, quality, hardware, TestFile_120_mbps_4k_uhd_hevc_10bit,
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
    public void FfmpegBuilder_VideoEncode_H264_10bit_Hardware() => TestEncode(false, true, true);
    [TestMethod]
    public void FfmpegBuilder_VideoEncode_H264_Hardware() => TestEncode(false, false, true);
    [TestMethod]
    public void FfmpegBuilder_VideoEncode_H264() => TestEncode(false, false, false);
    [TestMethod]
    public void FfmpegBuilder_VideoEncode_H264_10bit() => TestEncode(false, false, true);
}

#endif