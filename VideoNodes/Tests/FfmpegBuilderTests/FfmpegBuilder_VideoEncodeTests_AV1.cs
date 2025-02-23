#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileFlows.VideoNodes.FfmpegBuilderNodes;


namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

[TestClass]
public class FfmpegBuilder_VideoEncodeTests_AV1
{
    [TestMethod]
    [DataRow(1, "ultrafast", new[] { "libaom-av1", "-crf", "30", "-cpu-used", "13" })]
    [DataRow(10, "medium", new[] { "libaom-av1", "-crf", "15", "-cpu-used", "6" })]
    [DataRow(5, "slow", new[] { "libaom-av1", "-crf", "23", "-cpu-used", "4" })]
    [DataRow(7, "1", new[] { "libaom-av1", "-crf", "20", "-cpu-used", "2" })]
    [DataRow(8, "1", new[] { "libaom-av1", "-crf", "18", "-cpu-used", "2" })]
    public void Av1_CPU_Tests(int quality, string speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncode.AV1_CPU(quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result), 
            $"Expected: [{string.Join(", ", expected)}], but got: [{string.Join(", ", result)}]");
    }

    [TestMethod]
    [DataRow(1, "veryslow", new[] { "av1_amf", "-qp", "30", "-preset", "veryslow", "-spatial-aq", "1" })]
    [DataRow(10, "medium", new[] { "av1_amf", "-qp", "15", "-preset", "balanced", "-spatial-aq", "1" })]
    [DataRow(5, "fast", new[] { "av1_amf", "-qp", "23", "-preset", "speed", "-spatial-aq", "1" })]
    public void Av1_Amd_Tests(int quality, string speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncode.AV1_Amd(quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result), 
            $"Expected: [{string.Join(", ", expected)}], but got: [{string.Join(", ", result)}]");

    }

    [TestMethod]
    [DataRow(1, "veryslow", new[] { "av1_nvenc", "-rc", "constqp", "-qp", "30", "-preset", "p7", "-spatial-aq", "1" })]
    [DataRow(10, "medium", new[] { "av1_nvenc", "-rc", "constqp", "-qp", "15", "-preset", "p4", "-spatial-aq", "1" })]
    [DataRow(5, "fast", new[] { "av1_nvenc", "-rc", "constqp", "-qp", "23", "-preset", "p1", "-spatial-aq", "1" })]
    public void Av1_Nvidia_Tests(int quality, string speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncode.AV1_Nvidia(quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result), 
            $"Expected: [{string.Join(", ", expected)}], but got: [{string.Join(", ", result)}]");
    }

    [TestMethod]
    [DataRow("", 1, "veryslow", new[] { "av1_qsv", "-global_quality:v", "30", "-preset", "1", "-qsv_device", "/dev/dri/renderD128" })]
    [DataRow("NONE", 10, "medium", new[] { "av1_qsv", "-global_quality:v", "15", "-preset", "4" })]
    [DataRow("/dev/dri/renderD128", 5, "fast", new[] { "av1_qsv", "-global_quality:v", "23", "-preset", "7", "-qsv_device", "/dev/dri/renderD128" })]
    public void Av1_Qsv_Tests(string device, int quality, string speed, string[] expected)
    {
        var result = FfmpegBuilderVideoEncode.AV1_Qsv(device, quality, speed).ToArray();
        Assert.AreEqual(string.Join(", ", expected), string.Join(", ", result), 
            $"Expected: [{string.Join(", ", expected)}], but got: [{string.Join(", ", result)}]");
    }

    [TestMethod]
    [DataRow(0, 30)] // Lower bound
    [DataRow(1, 30)]
    [DataRow(5, 23)]
    [DataRow(10, 15)] // Upper bound
    [DataRow(11, 15)] // Out of range
    public void MapQuality_Tests(int quality, int expected)
    {
        int result = FfmpegBuilderVideoEncode.MapQuality(quality);
        Assert.AreEqual(expected, result);
    }
}


#endif