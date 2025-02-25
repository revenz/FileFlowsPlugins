#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileFlows.VideoNodes.FfmpegBuilderNodes;


namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_VideoEncodeTests_AV1
{
    [TestMethod]
    [DataRow(1, 5, new[] { "libaom-av1", "-crf", "30", "-cpu-used", "10" })]
    [DataRow(10, 3, new[] { "libaom-av1", "-crf", "15", "-cpu-used", "6" })]
    [DataRow(5, 2, new[] { "libaom-av1", "-crf", "23", "-cpu-used", "4" })]
    [DataRow(7, 1, new[] { "libaom-av1", "-crf", "20", "-cpu-used", "2" })]
    [DataRow(8, 1, new[] { "libaom-av1", "-crf", "18", "-cpu-used", "2" })]
    public void Av1_CPU_Tests(int quality, int speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncodeSimple.AV1_CPU(quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result), 
            $"Expected: [{string.Join(", ", expected)}], but got: [{string.Join(", ", result)}]");
    }

    [TestMethod]
    [DataRow(1, 1, new[] { "av1_amf", "-qp", "30", "-preset", "high_quality", "-spatial-aq", "1" })]
    [DataRow(10, 3, new[] { "av1_amf", "-qp", "15", "-preset", "balanced", "-spatial-aq", "1" })]
    [DataRow(5, 5, new[] { "av1_amf", "-qp", "23", "-preset", "speed", "-spatial-aq", "1" })]
    public void Av1_Amd_Tests(int quality, int speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncodeSimple.AV1_Amd(quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result), 
            $"Expected: [{string.Join(", ", expected)}], but got: [{string.Join(", ", result)}]");

    }

    [TestMethod]
    [DataRow(1, 1, new[] { "av1_nvenc", "-rc", "constqp", "-qp", "30", "-preset", "p7", "-spatial-aq", "1" })]
    [DataRow(10, 3, new[] { "av1_nvenc", "-rc", "constqp", "-qp", "15", "-preset", "p4", "-spatial-aq", "1" })]
    [DataRow(5, 4, new[] { "av1_nvenc", "-rc", "constqp", "-qp", "23", "-preset", "p3", "-spatial-aq", "1" })]
    public void Av1_Nvidia_Tests(int quality, int speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncodeSimple.AV1_Nvidia(quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result), 
            $"Expected: [{string.Join(", ", expected)}], but got: [{string.Join(", ", result)}]");
    }

    [TestMethod]
    [DataRow("", 1, 1, new[] { "av1_qsv", "-global_quality:v", "30", "-preset", "1", "-qsv_device", "/dev/dri/renderD128" })]
    [DataRow("NONE", 10, 4, new[] { "av1_qsv", "-global_quality:v", "15", "-preset", "5" })]
    [DataRow("/dev/dri/renderD128", 5, 5, new[] { "av1_qsv", "-global_quality:v", "23", "-preset", "7", "-qsv_device", "/dev/dri/renderD128" })]
    public void Av1_Qsv_Tests(string device, int quality, int speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncodeSimple.AV1_Qsv(device, quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result), 
            $"Expected: [{string.Join(", ", expected)}], but got: [{string.Join(", ", result)}]");
    }

    [TestMethod]
    [DataRow(1, 30)]
    [DataRow(2, 28)]
    [DataRow(3, 27)]
    [DataRow(4, 26)]
    [DataRow(5, 24)]
    [DataRow(6, 22)] 
    [DataRow(7, 21)] 
    [DataRow(8, 20)] 
    [DataRow(9, 18)] 
    public void MapQuality_Tests(int quality, int expected)
    {
        int result = FfmpegBuilderVideoEncodeSimple.MapQuality(quality);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow(1, 60)]
    [DataRow(2, 62)]
    [DataRow(3, 65)]
    [DataRow(4, 68)]
    [DataRow(5, 70)]
    [DataRow(6, 72)] 
    [DataRow(7, 75)] 
    [DataRow(8, 78)] 
    [DataRow(9, 80)] 
    public void MapQuality_VideoToolbox_Tests(int quality, int expected)
    {
        int result = FfmpegBuilderVideoEncodeSimple.MapQualityVideoToolbox(quality);
        Assert.AreEqual(expected, result);
    }
}


#endif