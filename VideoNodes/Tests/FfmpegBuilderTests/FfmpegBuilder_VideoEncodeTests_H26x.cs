#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using FileFlows.VideoNodes.FfmpegBuilderNodes;


namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_VideoEncodeTests_H26x
{
    
    [TestMethod]
    [DataRow(false, 1, 3, new[] { "libx264", "-preset", "medium", "-crf", "25" })]
    [DataRow(true, 10, 4, new[] { "libx265", "-preset", "fast", "-crf", "18" })]
    [DataRow(true, 5, 1, new[] { "libx265", "-preset", "veryslow", "-crf", "24" })]
    public void H26x_CPU_Tests(bool h265, int quality, int speed, string[] expected)
    {
        string[] bit10Filters;
        var result = FfmpegBuilderVideoEncodeSimple.H26x_CPU(h265, quality, speed, out bit10Filters).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result));
    }

    [TestMethod]
    [DataRow(false, 1, 5, new[] { "h264_nvenc", "-rc", "constqp", "-qp", "25", "-preset", "p1", "-spatial-aq", "1" })]
    [DataRow(true, 10, 3, new[] { "hevc_nvenc", "-rc", "constqp", "-qp", "18", "-preset", "p3", "-spatial-aq", "1" })]
    public void H26x_Nvidia_Tests(bool h265, int quality, int speed, string[] expected)
    {
        string[] non10BitFilters;
        var result = FfmpegBuilderVideoEncodeSimple.H26x_Nvidia(h265, quality, speed, out non10BitFilters).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result));
    }

    [TestMethod]
    [DataRow(true, 10, 60f, 3, new[] { "hevc_qsv", "-load_plugin", "hevc_hw", "-r", "60", "-g", "300", "-global_quality:v", "18", "-preset", "medium" })]
    [DataRow(false, 1, 30f, 2, new[] { "h264_qsv","-r" , "30", "-g", "150", "-global_quality:v", "25", "-preset", "slow" })]
    public void H26x_Qsv_Tests(bool h265, int quality, float fps, int speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncodeSimple.H26x_Qsv(h265, quality, fps, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result));
    }

    [TestMethod]
    [DataRow(false, 1, 5, new[] { "h264_amf", "-qp", "25", "-preset", "0" })]
    [DataRow(true, 10, 3, new[] { "hevc_amf", "-qp", "18", "-preset", "6" })]
    public void H26x_Amd_Tests(bool h265, int quality, int speed, string[] expected)
    {
        string[] bit10Filters;
        var result = FfmpegBuilderVideoEncodeSimple.H26x_Amd(h265, quality, speed, out bit10Filters).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result));
    }

    [TestMethod]
    [DataRow(false, 1, 5, new[] { "h264_vaapi", "-qp", "25", "-preset", "ultrafast" })]
    [DataRow(true, 10, 3, new[] { "hevc_vaapi", "-qp", "18", "-preset", "medium" })]
    public void H26x_Vaapi_Tests(bool h265, int quality, int speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncodeSimple.H26x_Vaapi(h265, quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result));
    }

    [TestMethod]
    [DataRow(false, 1, 5, new[] { "h264_videotoolbox", "-q", "60", "-preset", "ultrafast" })]
    [DataRow(false, 5, 3, new[] { "h264_videotoolbox", "-q", "70", "-preset", "medium" })]
    [DataRow(true, 10, 3, new[] { "hevc_videotoolbox", "-q", "80", "-preset", "medium" })]
    public void H26x_VideoToolbox_Tests(bool h265, int quality, int speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncodeSimple.H26x_VideoToolbox(h265, quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result));
    }
}


#endif