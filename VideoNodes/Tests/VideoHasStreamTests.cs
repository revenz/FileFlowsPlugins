#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VideoNodes.Tests;

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

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
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

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
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

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
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

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
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
        string file = TestFile_5dot1;
        var vi = new VideoInfoHelper(FfmpegPath, Logger);
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Channels = 5.1f;
        node.Stream = "Audio";

        var args = new NodeParameters(file, Logger, false, string.Empty, new LocalFileService());
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }



    [TestMethod]
    public void VideoHasStream_Audio_Channels_Pass_61()
    {
        var vi = VideoInfoHelper.ParseOutput(Logger,
            @"Input #0, matroska,webm, from '/media/Saw 2.mkv':
  Metadata:
    encoder         : libebml v0.7.7 + libmatroska v0.8.1
    creation_time   : 2008-09-13T10:11:34.000000Z
  Duration: 01:34:34.46, start: 0.000000, bitrate: 12107 kb/s
  Chapters:
    Chapter #0:0: start 0.000000, end 290.248000
      Metadata:
        title           : Der Informant
    Chapter #0:1: start 290.248000, end 414.789000
      Metadata:
        title           : Vater und Sohn
    Chapter #0:2: start 414.789000, end 553.886000
      Metadata:
        title           : Der Tatort
    Chapter #0:3: start 553.886000, end 649.065000
      Metadata:
        title           : Genug zu tun
    Chapter #0:4: start 649.065000, end 914.038000
      Metadata:
        title           : S.W.A.T - Spezialeinheit
    Chapter #0:5: start 914.038000, end 1321.611000
      Metadata:
        title           : Wo ist er?
    Chapter #0:6: start 1321.611000, end 2045.626000
      Metadata:
        title           : Die Spielregeln
    Chapter #0:7: start 2045.626000, end 2580.619000
      Metadata:
        title           : Der Entführer
    Chapter #0:8: start 2580.619000, end 3032.446000
      Metadata:
        title           : Erinnerungen
    Chapter #0:9: start 3032.446000, end 3637.258000
      Metadata:
        title           : Die Grube des Elends
    Chapter #0:10: start 3637.258000, end 3866.654000
      Metadata:
        title           : Zahlensuche
    Chapter #0:11: start 3866.654000, end 4111.565000
      Metadata:
        title           : Der Hinweis
    Chapter #0:12: start 4111.565000, end 4184.763000
      Metadata:
        title           : Auf die harte Tour
    Chapter #0:13: start 4184.763000, end 4318.814000
      Metadata:
        title           : Helft mir!
    Chapter #0:14: start 4318.814000, end 4791.161000
      Metadata:
        title           : Flucht mit dem Fahrstuhl
    Chapter #0:15: start 4791.161000, end 4980.975000
      Metadata:
        title           : Überwindung
    Chapter #0:16: start 4980.975000, end 5328.906000
      Metadata:
        title           : Neues Spiel
    Chapter #0:17: start 5328.906000, end 5674.464000
      Metadata:
        title           : Abspann
  Stream #0:0: Video: h264 (High), yuv420p(progressive), 1920x1040, SAR 1:1 DAR 24:13, 23.98 fps, 23.98 tbr, 1k tbn (default)
    Metadata:
      title           : Saw 2 - US Directors Cut
  Stream #0:1(ger): Audio: dts (DTS-ES), 48000 Hz, 6.1, fltp, 1536 kb/s (default)");

        VideoHasStream node = new();
        node.Channels = 6.1f;
        node.Stream = "Audio";

        var args = new NodeParameters(null, Logger, false, string.Empty, new LocalFileService());
        args.Parameters["VideoInfo"] = vi;
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

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

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
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

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void VideoHasStream_Audio_Lang_Pass()
    {
        string file = TestFile_MovText_Mp4;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Language = "ita";
        node.Stream = "Audio";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void VideoHasStream_Audio_Lang_Fail()
    {
        string file = TestFile_MovText_Mp4;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        VideoHasStream node = new();
        node.Language = "mao";
        node.Stream = "Audio";

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(2, output);
    }
}


#endif