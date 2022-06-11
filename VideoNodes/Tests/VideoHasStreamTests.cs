#if(DEBUG)

namespace VideoNodes.Tests;

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TestClass]
public class VideoHasStreamTests : TestBase
{
    [TestMethod]
    public void VideoHasStream_Video_H264()
    {
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Codec = "h264";
        node.Stream = "Video";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void VideoHasStream_Video_H265()
    {
        string file = TestFile_120_mbps_4k_uhd_hevc_10bit;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Codec = "h265";
        node.Stream = "Video";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void VideoHasStream_Video_Hevc()
    {
        string file = TestFile_120_mbps_4k_uhd_hevc_10bit;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Codec = "h265";
        node.Stream = "Video";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void VideoHasStream_Audio_Vorbis()
    {
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Codec = "vorbis";
        node.Stream = "Audio";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }



    [TestMethod]
    public void VideoHasStream_Audio_Channels_Pass()
    {
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Channels = 5.1f;
        node.Stream = "Audio";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void VideoHasStream_Audio_Channels_Fail()
    {
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Channels = 2;
        node.Stream = "Audio";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(2, output);
    }


    [TestMethod]
    public void VideoHasStream_Video_Tag()
    {
        string file = TestFile_Tag;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Codec = "h264";
        node.Stream = "Video";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty);
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }
}


#endif