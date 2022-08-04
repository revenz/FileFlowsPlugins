#if(DEBUG)

namespace VideoNodes.Tests;

using FileFlows.VideoNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class AudioToVideoTests : TestBase
{
    [TestMethod]
    public void AudioToVideo_Waves_h265()
        => TestStyle(FfmpegBuilderVideoEncode.CODEC_H264, AudioToVideo.VisualizationStyle.Waves);

    [TestMethod]
    public void AudioToVideo_AudioVectorScope_H265()
        => TestStyle(FfmpegBuilderVideoEncode.CODEC_H265, AudioToVideo.VisualizationStyle.AudioVectorScope);


    [TestMethod]
    public void AudioToVideo_Spectrum_H265_10Bit()
        => TestStyle(FfmpegBuilderVideoEncode.CODEC_H265_10BIT, AudioToVideo.VisualizationStyle.Spectrum);



    private void TestStyle(string codec, AudioToVideo.VisualizationStyle style)
    {
        var logger = new TestLogger();
        string file = @"D:\music\unprocessed\01-billy_joel-movin_out.mp3";
        var vi = new VideoInfoHelper(FfmpegPath, logger);
        var vii = vi.Read(file);

        var args = new NodeParameters(file, logger, false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;


        AudioToVideo node = new();
        node.Container = "mkv";
        node.Resolution = "1280x720";
        node.Codec = codec;
        node.HardwareEncoding = true;
        node.Visualization = style;
        if (node.Visualization == AudioToVideo.VisualizationStyle.Waves)
            node.Color = "#007bff";
        node.PreExecute(args);
        int output = node.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(1, output);
    }
}


#endif