#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginTestLibrary;

namespace VideoNodes.Tests;

[TestClass]
public class VideoHasStreamTests : VideoTestBase
{
    [TestMethod]
    public void VideoHasStream_Video_H264()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Codec = "h264";
        element.Stream = "Video";
        element.PreExecute(args);

        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void VideoHasStream_Video_H265()
    {
        var args = GetVideoNodeParameters(VideoMkvHevc);

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Codec = "h265";
        element.Stream = "Video";
        element.PreExecute(args);

        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void VideoHasStream_Video_Hevc()
    {
        var args = GetVideoNodeParameters(VideoMkvHevc);

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Codec = "hevc";
        element.Stream = "Video";
        element.PreExecute(args);

        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void VideoHasStream_Audio_Aac()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Codec = "aac";
        element.Stream = "Audio";
        element.PreExecute(args);

        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }



    [TestMethod]
    public void VideoHasStream_Audio_Channels_Pass()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Channels = "=2.0";
        element.Stream = "Audio";
        element.PreExecute(args);

        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void VideoHasStream_Audio_Channels_GreaterThan2()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Channels = ">2";
        element.Stream = "Audio";
        element.PreExecute(args);

        int output = element.Execute(args);

        Assert.AreEqual(2, output);
    }
    [TestMethod]
    public void VideoHasStream_Audio_Channels_GreaterThan5dot1()
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


        var args = new NodeParameters(null, Logger, false, string.Empty, new LocalFileService());
        args.Parameters["VideoInfo"] = vi;
        args.GetToolPathActual = (string tool) => FFmpeg;
        args.TempPath = TempPath;

        VideoHasStream node = new();
        node.Stream = "Audio";
        
        node.Channels = ">5.1";
        vi.AudioStreams[0].Channels = 5.09999978f;
        int output = node.Execute(args);
        Assert.AreEqual(2, output);
        
        node.Channels = ">=5.1";
        vi.AudioStreams[0].Channels = 5.09999978f;
        output = node.Execute(args);
        Assert.AreEqual(1, output);
        
        node.Channels = "<5.1";
        vi.AudioStreams[0].Channels = 5.09999978f;
        output = node.Execute(args);
        Assert.AreEqual(2, output);
        
        node.Channels = "<=5.1";
        vi.AudioStreams[0].Channels = 5.09999978f;
        output = node.Execute(args);
        Assert.AreEqual(1, output);
        
        node.Channels = "=5.1";
        vi.AudioStreams[0].Channels = 5.09999978f;
        output = node.Execute(args);
        Assert.AreEqual(1, output);
        
        node.Channels = ">5.1";
        vi.AudioStreams[0].Channels = 5.1000000234f;
        output = node.Execute(args);
        Assert.AreEqual(2, output);
        node.Channels = ">=5.1";
        vi.AudioStreams[0].Channels = 5.1000000234f;
        output = node.Execute(args);
        Assert.AreEqual(1, output);
        
        node.Channels = "<5.1";
        vi.AudioStreams[0].Channels = 5.1000000234f;
        output = node.Execute(args);
        Assert.AreEqual(2, output);
        
        node.Channels = "<=5.1";
        vi.AudioStreams[0].Channels = 5.1000000234f;
        output = node.Execute(args);
        Assert.AreEqual(1, output);
        
        node.Channels = "=5.1";
        vi.AudioStreams[0].Channels = 5.1000000234f;
        output = node.Execute(args);
        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void VideoHasStream_Audio_Channels_GreaterOrEqual()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Channels = ">=4.5";
        element.Stream = "Audio";
        element.PreExecute(args);
    }
    
    [TestMethod]
    public void VideoHasStream_Audio_Channels_Between()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Channels = "5<>5.1";
        element.Stream = "Audio";
        element.PreExecute(args);
    }

    [TestMethod]
    public void VideoHasStream_Audio_Channels_NotBetween()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Channels = "2><3";
        element.Stream = "Audio";
        element.PreExecute(args);

        int output = element.Execute(args);

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
        node.Channels = "=6.1";
        node.Stream = "Audio";

        var args = new NodeParameters(null, Logger, false, string.Empty, new LocalFileService());
        args.Parameters["VideoInfo"] = vi;
        args.GetToolPathActual = (string tool) => FFmpeg;
        args.TempPath = TempPath;

        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void VideoHasStream_Audio_Channels_Fail()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Channels = "=5.1";
        element.Stream = "Audio";
        element.PreExecute(args);

        int output = element.Execute(args);

        Assert.AreEqual(2, output);
    }



    [TestMethod]
    public void VideoHasStream_Audio_Lang_Pass()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Language = "eng";
        element.Stream = "Audio";
        element.PreExecute(args);
        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void VideoHasStream_Audio_Lang_Regex()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Language = "/en|de|fr/";
        element.Stream = "Audio";
        element.PreExecute(args);
        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }
    [TestMethod]
    public void VideoHasStream_Audio_Lang_Regex_Invert()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Language = "!/de|fr/";
        element.Stream = "Audio";
        element.PreExecute(args);
        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void VideoHasStream_Audio_Lang_English()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Language = "English";
        element.Stream = "Audio";
        element.PreExecute(args);
        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void VideoHasStream_Audio_Lang_StartsWith()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Language = "Eng*";
        element.Stream = "Audio";
        element.PreExecute(args);
        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void VideoHasStream_Audio_Lang_Multi()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Stream = "Audio";
        element.PreExecute(args);

        foreach (var lang in new[] { "en", "eng", "english", "!de", "!/de|fr/", "eng*", "!de*", "*ish" })
        {
            element.Language = lang;
            int output = element.Execute(args);
            Assert.AreEqual(1, output);
        }
    }
    
    [TestMethod]
    public void VideoHasStream_Audio_Lang_Variable()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Stream = "Audio";
        element.PreExecute(args);

        foreach (var lang in new[] {  "orig", "original", "OriginalLanguage", "{OriginalLanguage}", "{orig}", })
        {
            Logger.ILog("###### Testing: " + lang);
            args.Variables["OriginalLanguage"] = "eng";
            element.Language = lang;
            int output = element.Execute(args);
            Assert.AreEqual(1, output);
        }
    }
    
    [TestMethod]
    public void VideoHasStream_Audio_Lang_Fail()
    {
        var args = GetVideoNodeParameters();

        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        VideoHasStream element = new();
        element.Channels = "mao";
        element.Stream = "Audio";
        element.PreExecute(args);
        int output = element.Execute(args);

        Assert.AreEqual(2, output);
    }
}


#endif