#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VideoNodes.Tests;

[TestClass]
public class VideoInfoHelperTests : VideoTestBase
{

  // [TestMethod]
  // public void ComskipTest()
  // {
  //
  //   var vi = new VideoInfoHelper(@"C:\utils\ffmpeg\ffmpeg.exe", new TestLogger());
  //   var vii = vi.Read(file);
  //   args.SetParameter("VideoInfo", vii);
  //   //args.Process = new FileFlows.Plugin.ProcessHelper(args.Logger);
  //
  //   var node = new ComskipRemoveAds();
  //   int output = node.Execute(args);
  //   Assert.AreEqual(1, output);
  // }




  [TestMethod]
  public void VideoInfoTest_Subtitle_Extractor()
  {
    var args = GetVideoNodeParameters();
    var vf = new VideoFile();
    vf.PreExecute(args);
    vf.Execute(args);

    SubtitleExtractor element = new();
    element.PreExecute(args);
    int output = element.Execute(args);

    Assert.AreEqual(1, output);
  }

  [TestMethod]
  public void VideoInfoTest_AC1()
  {
    string ffmpegOutput =
      @"Input #0, mov,mp4,m4a,3gp,3g2,mj2, from '/media/Videos/Input3/input file.mp4':
Metadata:
major_brand     : mp42
minor_version   : 512
compatible_brands: mp42iso6
creation_time   : 2022-01-07T06:30:47.000000Z
title           : Episode title
comment         : Episode description
Duration: 00:42:23.75, start: 0.000000, bitrate: 3174 kb/s
Stream #0:0(und): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 1920x1080 [SAR 1:1 DAR 16:9], 2528 kb/s, 23.98 fps, 23.98 tbr, 24k tbn, 47.95 tbc (default)
Metadata:
  creation_time   : 2022-01-07T06:30:47.000000Z
  handler_name    : VideoHandler
Stream #0:1(deu): Audio: eac3 (ec-3 / 0x332D6365), 48000 Hz, 5.1(side), fltp, 640 kb/s (default)
Metadata:
  creation_time   : 2022-01-07T06:30:47.000000Z
  handler_name    : SoundHandler
Side data:
  audio service type: main";
    var vi = VideoInfoHelper.ParseOutput(null, ffmpegOutput);
    Assert.AreEqual(1920, vi.VideoStreams[0].Width);
    Assert.AreEqual(1080, vi.VideoStreams[0].Height);
    Assert.AreEqual("nv12", vi.VideoStreams[0].PixelFormat);
  }


  [TestMethod]
  public void VideoInfoTest_Chapters()
  {
    string ffmpegOutput =
      @"[matroska,webm @ 00000263322abdc0] Could not find codec parameters for stream 3 (Subtitle: hdmv_pgs_subtitle (pgssub)): unspecified size
Consider increasing the value for the 'analyzeduration' (0) and 'probesize' (5000000) options
[matroska,webm @ 00000263322abdc0] Could not find codec parameters for stream 4 (Subtitle: hdmv_pgs_subtitle (pgssub)): unspecified size
Consider increasing the value for the 'analyzeduration' (0) and 'probesize' (5000000) options
Input #0, matroska,webm, from 'D:\downloads\sabnzbd\complete\movies\Cast.Away.2000.BluRay.1080p.REMUX.AVC.DTS-HD.MA.5.1-LEGi0N\b0e4afee2ced4ae3a3592b82ae335608.mkv':
Metadata:
encoder         : libebml v1.4.2 + libmatroska v1.6.4
creation_time   : 2022-02-02T22:32:47.000000Z
Duration: 02:23:46.66, start: 0.000000, bitrate: 38174 kb/s
Chapters:
Chapter #0:0: start 0.000000, end 110.819000
  Metadata:
    title           : Chapter 01
Chapter #0:1: start 110.819000, end 517.851000
  Metadata:
    title           : Chapter 02
Chapter #0:2: start 517.851000, end 743.326000
  Metadata:
    title           : Chapter 03
Chapter #0:3: start 743.326000, end 1061.269000
  Metadata:
    title           : Chapter 04
Chapter #0:4: start 1061.269000, end 1243.534000
  Metadata:
    title           : Chapter 05
Chapter #0:5: start 1243.534000, end 1360.234000
  Metadata:
    title           : Chapter 06
Chapter #0:6: start 1360.234000, end 1545.461000
  Metadata:
    title           : Chapter 07
Chapter #0:7: start 1545.461000, end 1871.620000
  Metadata:
    title           : Chapter 08
Chapter #0:8: start 1871.620000, end 2155.320000
  Metadata:
    title           : Chapter 09
Chapter #0:9: start 2155.320000, end 2375.623000
  Metadata:
    title           : Chapter 10
Chapter #0:10: start 2375.623000, end 2543.207000
  Metadata:
    title           : Chapter 11
Chapter #0:11: start 2543.207000, end 2794.208000
  Metadata:
    title           : Chapter 12
Chapter #0:12: start 2794.208000, end 3109.314000
  Metadata:
    title           : Chapter 13
Chapter #0:13: start 3109.314000, end 3389.052000
  Metadata:
    title           : Chapter 14
Chapter #0:14: start 3389.052000, end 3694.357000
  Metadata:
    title           : Chapter 15
Chapter #0:15: start 3694.357000, end 3873.119000
  Metadata:
    title           : Chapter 16
Chapter #0:16: start 3873.119000, end 4391.846000
  Metadata:
    title           : Chapter 17
Chapter #0:17: start 4391.846000, end 4657.736000
  Metadata:
    title           : Chapter 18
Chapter #0:18: start 4657.736000, end 4749.745000
  Metadata:
    title           : Chapter 19
Chapter #0:19: start 4749.745000, end 4842.045000
  Metadata:
    title           : Chapter 20
Chapter #0:20: start 4842.045000, end 5197.901000
  Metadata:
    title           : Chapter 21
Chapter #0:21: start 5197.901000, end 5640.176000
  Metadata:
    title           : Chapter 22
Chapter #0:22: start 5640.176000, end 6037.365000
  Metadata:
    title           : Chapter 23
Chapter #0:23: start 6037.365000, end 6321.398000
  Metadata:
    title           : Chapter 24
Chapter #0:24: start 6321.398000, end 6458.368000
  Metadata:
    title           : Chapter 25
Chapter #0:25: start 6458.368000, end 6810.470000
  Metadata:
    title           : Chapter 26
Chapter #0:26: start 6810.470000, end 6959.953000
  Metadata:
    title           : Chapter 27
Chapter #0:27: start 6959.953000, end 7499.575000
  Metadata:
    title           : Chapter 28
Chapter #0:28: start 7499.575000, end 7707.575000
  Metadata:
    title           : Chapter 29
Chapter #0:29: start 7707.575000, end 7941.725000
  Metadata:
    title           : Chapter 30
Chapter #0:30: start 7941.725000, end 8214.414000
  Metadata:
    title           : Chapter 31
Chapter #0:31: start 8214.414000, end 8626.656000
  Metadata:
    title           : Chapter 32
Stream #0:0(eng): Video: h264 (High), yuv420p(progressive), 1920x1080 [SAR 1:1 DAR 16:9], 23.98 fps, 23.98 tbr, 1k tbn, 47.95 tbc (default)
Metadata:
  title           : English
  BPS             : 33666894
  DURATION        : 02:23:46.618000000
  NUMBER_OF_FRAMES: 206832
  NUMBER_OF_BYTES : 36303929846
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:1(eng): Audio: dts (DTS-HD MA), 48000 Hz, 5.1(side), s32p (24 bit) (default)
Metadata:
  title           : English
  BPS             : 4236399
  DURATION        : 02:23:46.624000000
  NUMBER_OF_FRAMES: 808746
  NUMBER_OF_BYTES : 4568228448
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:2(eng): Audio: ac3, 48000 Hz, stereo, fltp, 224 kb/s (comment)
Metadata:
  title           : English commentary
  BPS             : 224000
  DURATION        : 02:23:46.656000000
  NUMBER_OF_FRAMES: 269583
  NUMBER_OF_BYTES : 241546368
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:3(eng): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : English (SDH)
  BPS             : 25275
  DURATION        : 02:14:32.439000000
  NUMBER_OF_FRAMES: 1740
  NUMBER_OF_BYTES : 25504616
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:4(spa): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : Spanish
  BPS             : 21585
  DURATION        : 02:12:54.884000000
  NUMBER_OF_FRAMES: 1412
  NUMBER_OF_BYTES : 21517695
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES";
    var vi = VideoInfoHelper.ParseOutput(null, ffmpegOutput);
    Assert.AreEqual(32, vi.Chapters?.Count ?? 0);
    Assert.AreEqual("Chapter 32", vi.Chapters[31].Title);
    Assert.AreEqual(TimeSpan.FromSeconds(8214.414000), vi.Chapters[31].Start);
    Assert.AreEqual(TimeSpan.FromSeconds(8626.656000), vi.Chapters[31].End);
  }



  [TestMethod]
  public void VideoInfoTest_Chapters_NoStart()
  {
    string ffmpegOutput =
      @"[matroska,webm @ 00000263322abdc0] Could not find codec parameters for stream 3 (Subtitle: hdmv_pgs_subtitle (pgssub)): unspecified size
Consider increasing the value for the 'analyzeduration' (0) and 'probesize' (5000000) options
[matroska,webm @ 00000263322abdc0] Could not find codec parameters for stream 4 (Subtitle: hdmv_pgs_subtitle (pgssub)): unspecified size
Consider increasing the value for the 'analyzeduration' (0) and 'probesize' (5000000) options
Input #0, matroska,webm, from 'D:\downloads\sabnzbd\complete\movies\Cast.Away.2000.BluRay.1080p.REMUX.AVC.DTS-HD.MA.5.1-LEGi0N\b0e4afee2ced4ae3a3592b82ae335608.mkv':
Metadata:
encoder         : libebml v1.4.2 + libmatroska v1.6.4
creation_time   : 2022-02-02T22:32:47.000000Z
Duration: 02:23:46.66, start: 0.000000, bitrate: 38174 kb/s
Chapters:
Chapter #0:0: end 110.819000
  Metadata:
    title           : Chapter 01
Chapter #0:1: start 110.819000, end 517.851000
  Metadata:
    title           : Chapter 02
Chapter #0:2: start 517.851000, end 743.326000
  Metadata:
    title           : Chapter 03
Chapter #0:3: start 743.326000, end 1061.269000
  Metadata:
    title           : Chapter 04
Chapter #0:4: start 1061.269000, end 1243.534000
  Metadata:
    title           : Chapter 05
Chapter #0:5: start 1243.534000, end 1360.234000
  Metadata:
    title           : Chapter 06
Chapter #0:6: start 1360.234000, end 1545.461000
  Metadata:
    title           : Chapter 07
Chapter #0:7: start 1545.461000, end 1871.620000
  Metadata:
    title           : Chapter 08
Chapter #0:8: start 1871.620000, end 2155.320000
  Metadata:
    title           : Chapter 09
Chapter #0:9: start 2155.320000, end 2375.623000
  Metadata:
    title           : Chapter 10
Chapter #0:10: start 2375.623000, end 2543.207000
  Metadata:
    title           : Chapter 11
Chapter #0:11: start 2543.207000, end 2794.208000
  Metadata:
    title           : Chapter 12
Chapter #0:12: start 2794.208000, end 3109.314000
  Metadata:
    title           : Chapter 13
Chapter #0:13: start 3109.314000, end 3389.052000
  Metadata:
    title           : Chapter 14
Chapter #0:14: start 3389.052000, end 3694.357000
  Metadata:
    title           : Chapter 15
Chapter #0:15: start 3694.357000, end 3873.119000
  Metadata:
    title           : Chapter 16
Chapter #0:16: start 3873.119000, end 4391.846000
  Metadata:
    title           : Chapter 17
Chapter #0:17: start 4391.846000, end 4657.736000
  Metadata:
    title           : Chapter 18
Chapter #0:18: start 4657.736000, end 4749.745000
  Metadata:
    title           : Chapter 19
Chapter #0:19: start 4749.745000, end 4842.045000
  Metadata:
    title           : Chapter 20
Chapter #0:20: start 4842.045000, end 5197.901000
  Metadata:
    title           : Chapter 21
Chapter #0:21: start 5197.901000, end 5640.176000
  Metadata:
    title           : Chapter 22
Chapter #0:22: start 5640.176000, end 6037.365000
  Metadata:
    title           : Chapter 23
Chapter #0:23: start 6037.365000, end 6321.398000
  Metadata:
    title           : Chapter 24
Chapter #0:24: start 6321.398000, end 6458.368000
  Metadata:
    title           : Chapter 25
Chapter #0:25: start 6458.368000, end 6810.470000
  Metadata:
    title           : Chapter 26
Chapter #0:26: start 6810.470000, end 6959.953000
  Metadata:
    title           : Chapter 27
Chapter #0:27: start 6959.953000, end 7499.575000
  Metadata:
    title           : Chapter 28
Chapter #0:28: start 7499.575000, end 7707.575000
  Metadata:
    title           : Chapter 29
Chapter #0:29: start 7707.575000, end 7941.725000
  Metadata:
    title           : Chapter 30
Chapter #0:30: start 7941.725000, end 8214.414000
  Metadata:
    title           : Chapter 31
Chapter #0:31: start 8214.414000, end 8626.656000
  Metadata:
    title           : Chapter 32
Stream #0:0(eng): Video: h264 (High), yuv420p(progressive), 1920x1080 [SAR 1:1 DAR 16:9], 23.98 fps, 23.98 tbr, 1k tbn, 47.95 tbc (default)
Metadata:
  title           : English
  BPS             : 33666894
  DURATION        : 02:23:46.618000000
  NUMBER_OF_FRAMES: 206832
  NUMBER_OF_BYTES : 36303929846
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:1(eng): Audio: dts (DTS-HD MA), 48000 Hz, 5.1(side), s32p (24 bit) (default)
Metadata:
  title           : English
  BPS             : 4236399
  DURATION        : 02:23:46.624000000
  NUMBER_OF_FRAMES: 808746
  NUMBER_OF_BYTES : 4568228448
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:2(eng): Audio: ac3, 48000 Hz, stereo, fltp, 224 kb/s (comment)
Metadata:
  title           : English commentary
  BPS             : 224000
  DURATION        : 02:23:46.656000000
  NUMBER_OF_FRAMES: 269583
  NUMBER_OF_BYTES : 241546368
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:3(eng): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : English (SDH)
  BPS             : 25275
  DURATION        : 02:14:32.439000000
  NUMBER_OF_FRAMES: 1740
  NUMBER_OF_BYTES : 25504616
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:4(spa): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : Spanish
  BPS             : 21585
  DURATION        : 02:12:54.884000000
  NUMBER_OF_FRAMES: 1412
  NUMBER_OF_BYTES : 21517695
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES";
    var vi = VideoInfoHelper.ParseOutput(null, ffmpegOutput);
    Assert.AreEqual(32, vi.Chapters?.Count ?? 0);
    Assert.AreEqual(TimeSpan.FromSeconds(0), vi.Chapters[0].Start);
    Assert.AreEqual(TimeSpan.FromSeconds(110.819000), vi.Chapters[0].End);
  }

  [TestMethod]
  public void VideoInfoTest_Chapters_Bad()
  {
    string ffmpegOutput =
      @"[matroska,webm @ 00000263322abdc0] Could not find codec parameters for stream 3 (Subtitle: hdmv_pgs_subtitle (pgssub)): unspecified size
Consider increasing the value for the 'analyzeduration' (0) and 'probesize' (5000000) options
[matroska,webm @ 00000263322abdc0] Could not find codec parameters for stream 4 (Subtitle: hdmv_pgs_subtitle (pgssub)): unspecified size
Consider increasing the value for the 'analyzeduration' (0) and 'probesize' (5000000) options
Input #0, matroska,webm, from 'D:\downloads\sabnzbd\complete\movies\Cast.Away.2000.BluRay.1080p.REMUX.AVC.DTS-HD.MA.5.1-LEGi0N\b0e4afee2ced4ae3a3592b82ae335608.mkv':
Metadata:
encoder         : libebml v1.4.2 + libmatroska v1.6.4
creation_time   : 2022-02-02T22:32:47.000000Z
Duration: 02:23:46.66, start: 0.000000, bitrate: 38174 kb/s
Chapters:
Chapter #0:0: end 110.819000
  Metadata:
    title           : Chapter 01
Chapter #0:1: start 110.819000, end 517.851000
  Metadata:
    title           : Chapter 0200, end 5640.176000
  Metadata:
    title           : Chapter 2200, end 7499.575000
  Metadata:
    title           : Chapter 28
Chapter #0:28: start 7499.575000, end 7707.575000
  Metadata:
    title           : Chapter 29
Chapter #0:29: start 7707.575000, end 7941.725000
  Metadata:
    title           : Chapter 30
Chapter #0:30: start 7941.725000, end 8214.414000
  Metadata:
    title           : Chapter 31
Chapter #0:31: start 8214.414000, end 8626.656000
  Metadata:
    title           : Chapter 32
Stream #0:0(eng): Video: h264 (High), yuv420p(progressive), 1920x1080 [SAR 1:1 DAR 16:9], 23.98 fps, 23.98 tbr, 1k tbn, 47.95 tbc (default)
Metadata:
  title           : English
  BPS             : 33666894
  DURATION        : 02:23:46.618000000
  NUMBER_OF_FRAMES: 206832
  NUMBER_OF_BYTES : 36303929846
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:1(eng): Audio: dts (DTS-HD MA), 48000 Hz, 5.1(side), s32p (24 bit) (default)
Metadata:
  title           : English
  BPS             : 4236399
  DURATION        : 02:23:46.624000000
  NUMBER_OF_FRAMES: 808746
  NUMBER_OF_BYTES : 4568228448
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:2(eng): Audio: ac3, 48000 Hz, stereo, fltp, 224 kb/s (comment)
Metadata:
  title           : English commentary
  BPS             : 224000
  DURATION        : 02:23:46.656000000
  NUMBER_OF_FRAMES: 269583
  NUMBER_OF_BYTES : 241546368
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:3(eng): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : English (SDH)
  BPS             : 25275
  DURATION        : 02:14:32.439000000
  NUMBER_OF_FRAMES: 1740
  NUMBER_OF_BYTES : 25504616
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:4(spa): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : Spanish
  BPS             : 21585
  DURATION        : 02:12:54.884000000
  NUMBER_OF_FRAMES: 1412
  NUMBER_OF_BYTES : 21517695
  _STATISTICS_WRITING_APP: mkvmerge v64.0.0 ('Willows') 64-bit
  _STATISTICS_WRITING_DATE_UTC: 2022-02-02 22:32:47
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES";
    var vi = VideoInfoHelper.ParseOutput(null, ffmpegOutput);
    Assert.AreEqual(6, vi.Chapters?.Count ?? 0);
    Assert.AreEqual("Chapter 29", vi.Chapters[2].Title);
  }

  [TestMethod]
  public void AudioParsingTest()
  {
    string audioInfo =
      @"Stream #0:1[0x2](fre): Audio: eac3 (ec-3 / 0x332D6365), 48000 Hz, 5.1(side), fltp, 640 kb/s (default)
Metadata:
  handler_name    : SoundHandler
  vendor_id       : [0][0][0][0]
Side data:
  audio service type: main";
    var audio = VideoInfoHelper.ParseAudioStream(Logger, audioInfo);
    Assert.AreEqual("fre", audio.Language);

    string audioInfo2 = @"Stream #0:1(eng): Audio: ac3, 48000 Hz, stereo, fltp, 192 kb/s (default)
Metadata:
  BPS             : 192000
  BPS-eng         : 192000
  DURATION        : 00:43:19.456000000
  DURATION-eng    : 00:43:19.456000000
  NUMBER_OF_FRAMES: 81233
  NUMBER_OF_FRAMES-eng: 81233
  NUMBER_OF_BYTES : 62386944
  NUMBER_OF_BYTES-eng: 62386944
  _STATISTICS_WRITING_APP: DVDFab 10.0.6.6
  _STATISTICS_WRITING_APP-eng: DVDFab 10.0.6.6
  _STATISTICS_WRITING_DATE_UTC: 2018-01-06 22:12:14
  _STATISTICS_WRITING_DATE_UTC-eng: 2018-01-06 22:12:14
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES";
    var audio2 = VideoInfoHelper.ParseAudioStream(Logger, audioInfo2);
    Assert.AreEqual("eng", audio2.Language);

  }


  /// <summary>
  /// A test that testes the audio parsing of a ffmpeg output
  /// This is from a user who was getting 0 channels for all 3 audio streams
  /// </summary>
  [TestMethod]
  public void AudioChannelsParsingTest()
  {
    string ffmpegOutput =
      @"Input #0, matroska,webm, from 'W:\\tvshows\test\HEVC\Game of Thrones - S06E06 - Blood of My Blood Bluray-1080p Remux AVC.mkv':
Metadata:
title           : Game of Thrones  (S06E06) - Blood of My Blood - ZQ
encoder         : libebml v1.3.4 + libmatroska v1.4.5
creation_time   : 2016-11-03T04:12:51.000000Z
Duration: 00:51:27.62, start: 0.000000, bitrate: 24656 kb/s
Chapters:
Chapter #0:0: start 0.000000, end 116.533000
  Metadata:
    title           : Chapter 1
Chapter #0:1: start 116.533000, end 850.767000
  Metadata:
    title           : Chapter 2
Chapter #0:2: start 850.767000, end 1425.090000
  Metadata:
    title           : Chapter 3
Chapter #0:3: start 1425.090000, end 1949.906000
  Metadata:
    title           : Chapter 4
Chapter #0:4: start 1949.906000, end 2291.164000
  Metadata:
    title           : Chapter 5
Chapter #0:5: start 2291.164000, end 2613.861000
  Metadata:
    title           : Chapter 6
Chapter #0:6: start 2613.861000, end 3007.004000
  Metadata:
    title           : Chapter 7
Chapter #0:7: start 3007.004000, end 3087.616000
  Metadata:
    title           : Chapter 8
Stream #0:0: Video: h264 (High), yuv420p(progressive), 1920x1080 [SAR 1:1 DAR 16:9], 23.98 fps, 23.98 tbr, 1k tbn (default)
Metadata:
  title           : MPEG-4 AVC Video / 19480 kbps / 1080p / 23.976 fps / 16:9 / High Profile 4.1
  BPS             : 19352137
  BPS-eng         : 19352137
  DURATION        : 00:51:27.585000000
  DURATION-eng    : 00:51:27.585000000
  NUMBER_OF_FRAMES: 74028
  NUMBER_OF_FRAMES-eng: 74028
  NUMBER_OF_BYTES : 7468921033
  NUMBER_OF_BYTES-eng: 7468921033
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:1(eng): Audio: truehd, 48000 Hz, 7.1, s32 (24 bit) (default)
Metadata:
  title           : Dolby Atmos Audio / 7.1 / 48 kHz / 4049 kbps / 24-bit
  BPS             : 4049069
  BPS-eng         : 4049069
  DURATION        : 00:51:27.585000000
  DURATION-eng    : 00:51:27.585000000
  NUMBER_OF_FRAMES: 3705102
  NUMBER_OF_FRAMES-eng: 3705102
  NUMBER_OF_BYTES : 1562730694
  NUMBER_OF_BYTES-eng: 1562730694
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:2(eng): Audio: ac3, 48000 Hz, 5.1(side), fltp, 640 kb/s
Metadata:
  title           : Compatibility Track / 5.1-EX / 48 kHz / 640 kbps
  BPS             : 640000
  BPS-eng         : 640000
  DURATION        : 00:51:27.616000000
  DURATION-eng    : 00:51:27.616000000
  NUMBER_OF_FRAMES: 96488
  NUMBER_OF_FRAMES-eng: 96488
  NUMBER_OF_BYTES : 247009280
  NUMBER_OF_BYTES-eng: 247009280
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:3(eng): Audio: ac3, 48000 Hz, 5.1(side), fltp, 448 kb/s
Metadata:
  title           : Commentary with Director Jack Bender, Director of Photography Jonathan Freeman, John Bradley (Samwell Tarly), and Hannah Murray (Gilly) / Dolby Digital Audio / 5.1 / 48 kHz / 448 kbps
  BPS             : 448000
  BPS-eng         : 448000
  DURATION        : 00:51:27.616000000
  DURATION-eng    : 00:51:27.616000000
  NUMBER_OF_FRAMES: 96488
  NUMBER_OF_FRAMES-eng: 96488
  NUMBER_OF_BYTES : 172906496
  NUMBER_OF_BYTES-eng: 172906496
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:4(eng): Subtitle: hdmv_pgs_subtitle (default)
Metadata:
  title           : Foreign Parts Only
  BPS             : 10463
  BPS-eng         : 10463
  DURATION        : 00:02:02.748000000
  DURATION-eng    : 00:02:02.748000000
  NUMBER_OF_FRAMES: 36
  NUMBER_OF_FRAMES-eng: 36
  NUMBER_OF_BYTES : 160544
  NUMBER_OF_BYTES-eng: 160544
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:5(eng): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : SDH
  BPS             : 10953
  BPS-eng         : 10953
  DURATION        : 00:48:01.837000000
  DURATION-eng    : 00:48:01.837000000
  NUMBER_OF_FRAMES: 1292
  NUMBER_OF_FRAMES-eng: 1292
  NUMBER_OF_BYTES : 3945866
  NUMBER_OF_BYTES-eng: 3945866
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:6(fre): Subtitle: hdmv_pgs_subtitle
Metadata:
  BPS             : 8899
  BPS-eng         : 8899
  DURATION        : 00:48:33.661000000
  DURATION-eng    : 00:48:33.661000000
  NUMBER_OF_FRAMES: 1210
  NUMBER_OF_FRAMES-eng: 1210
  NUMBER_OF_BYTES : 3241221
  NUMBER_OF_BYTES-eng: 3241221
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:7(spa): Subtitle: hdmv_pgs_subtitle
Metadata:
  BPS             : 9743
  BPS-eng         : 9743
  DURATION        : 00:47:02.569000000
  DURATION-eng    : 00:47:02.569000000
  NUMBER_OF_FRAMES: 1152
  NUMBER_OF_FRAMES-eng: 1152
  NUMBER_OF_BYTES : 3437560
  NUMBER_OF_BYTES-eng: 3437560
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:8(spa): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : Castillan
  BPS             : 9708
  BPS-eng         : 9708
  DURATION        : 00:47:02.444000000
  DURATION-eng    : 00:47:02.444000000
  NUMBER_OF_FRAMES: 1088
  NUMBER_OF_FRAMES-eng: 1088
  NUMBER_OF_BYTES : 3425040
  NUMBER_OF_BYTES-eng: 3425040
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:9(ger): Subtitle: hdmv_pgs_subtitle
Metadata:
  BPS             : 10110
  BPS-eng         : 10110
  DURATION        : 00:47:02.236000000
  DURATION-eng    : 00:47:02.236000000
  NUMBER_OF_FRAMES: 1036
  NUMBER_OF_FRAMES-eng: 1036
  NUMBER_OF_BYTES : 3566891
  NUMBER_OF_BYTES-eng: 3566891
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:10(por): Subtitle: hdmv_pgs_subtitle
Metadata:
  title           : Brazilian
  BPS             : 9851
  BPS-eng         : 9851
  DURATION        : 00:47:02.569000000
  DURATION-eng    : 00:47:02.569000000
  NUMBER_OF_FRAMES: 1158
  NUMBER_OF_FRAMES-eng: 1158
  NUMBER_OF_BYTES : 3475674
  NUMBER_OF_BYTES-eng: 3475674
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:11(dut): Subtitle: hdmv_pgs_subtitle
Metadata:
  BPS             : 9435
  BPS-eng         : 9435
  DURATION        : 00:47:02.569000000
  DURATION-eng    : 00:47:02.569000000
  NUMBER_OF_FRAMES: 1154
  NUMBER_OF_FRAMES-eng: 1154
  NUMBER_OF_BYTES : 3329208
  NUMBER_OF_BYTES-eng: 3329208
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:12(dan): Subtitle: hdmv_pgs_subtitle
Metadata:
  BPS             : 9498
  BPS-eng         : 9498
  DURATION        : 00:47:02.569000000
  DURATION-eng    : 00:47:02.569000000
  NUMBER_OF_FRAMES: 1152
  NUMBER_OF_FRAMES-eng: 1152
  NUMBER_OF_BYTES : 3351239
  NUMBER_OF_BYTES-eng: 3351239
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:13(fin): Subtitle: hdmv_pgs_subtitle
Metadata:
  BPS             : 8572
  BPS-eng         : 8572
  DURATION        : 00:47:02.569000000
  DURATION-eng    : 00:47:02.569000000
  NUMBER_OF_FRAMES: 1154
  NUMBER_OF_FRAMES-eng: 1154
  NUMBER_OF_BYTES : 3024401
  NUMBER_OF_BYTES-eng: 3024401
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:14(nor): Subtitle: hdmv_pgs_subtitle
Metadata:
  BPS             : 9183
  BPS-eng         : 9183
  DURATION        : 00:47:02.569000000
  DURATION-eng    : 00:47:02.569000000
  NUMBER_OF_FRAMES: 1154
  NUMBER_OF_FRAMES-eng: 1154
  NUMBER_OF_BYTES : 3240071
  NUMBER_OF_BYTES-eng: 3240071
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
Stream #0:15(swe): Subtitle: hdmv_pgs_subtitle
Metadata:
  BPS             : 9333
  BPS-eng         : 9333
  DURATION        : 00:47:02.569000000
  DURATION-eng    : 00:47:02.569000000
  NUMBER_OF_FRAMES: 1154
  NUMBER_OF_FRAMES-eng: 1154
  NUMBER_OF_BYTES : 3292894
  NUMBER_OF_BYTES-eng: 3292894
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES";
    var videoInfo = VideoInfoHelper.ParseOutput(Logger, ffmpegOutput);
    Assert.AreEqual(3, videoInfo.AudioStreams.Count);


    foreach (var vs in videoInfo.AudioStreams)
    {
      Logger.ILog($"Audio stream '{vs.Codec}' '{vs.Index}' 'Language: {vs.Language}' 'Channels: {vs.Channels}'");
    }


    Assert.AreEqual(7.1f, videoInfo.AudioStreams[0].Channels);
    Assert.AreEqual(5.1f, videoInfo.AudioStreams[1].Channels);
    Assert.AreEqual(5.1f, videoInfo.AudioStreams[2].Channels);

  }


  [TestMethod]
  public void AudioParsingTest2()
  {
    string audioInfo =
      @"Stream #0:1(eng): Audio: truehd, 48000 Hz, 7.1, s32 (24 bit) (default)
Metadata:
  title           : Dolby Atmos Audio / 7.1 / 48 kHz / 4049 kbps / 24-bit
  BPS             : 4049069
  BPS-eng         : 4049069
  DURATION        : 00:51:27.585000000
  DURATION-eng    : 00:51:27.585000000
  NUMBER_OF_FRAMES: 3705102
  NUMBER_OF_FRAMES-eng: 3705102
  NUMBER_OF_BYTES : 1562730694
  NUMBER_OF_BYTES-eng: 1562730694
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES";
    var audio = VideoInfoHelper.ParseAudioStream(Logger, audioInfo);
    Assert.AreEqual("eng", audio.Language);
    Assert.AreEqual(4049069, audio.Bitrate);
    Assert.AreEqual(7.1f, audio.Channels);
  }
  
  [TestMethod]
  public void InterlacedProgressive()
  {
    string output =
      @"Stream #0:0: Video: h264 (High), yuv420p(tv, bt709, progressive), 1920x1080
Metadata:
  title           : Dolby Atmos Audio / 7.1 / 48 kHz / 4049 kbps / 24-bit
  BPS             : 4049069
  BPS-eng         : 4049069
  DURATION        : 00:51:27.585000000
  DURATION-eng    : 00:51:27.585000000
  NUMBER_OF_FRAMES: 3705102
  NUMBER_OF_FRAMES-eng: 3705102
  NUMBER_OF_BYTES : 1562730694
  NUMBER_OF_BYTES-eng: 1562730694
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES";
    var video = VideoInfoHelper.ParseVideoStream(Logger, output, output);
    Assert.AreEqual("h264", video.Codec);
    Assert.IsFalse(video.IsInterlaced);
  }
  [TestMethod]
  public void InterlacedInterlaced()
  {
    string output =
      @"Stream #0:0: Video: h264 (High), yuv420p(tv, bt709, top first), 1920x1080
Metadata:
  title           : Dolby Atmos Audio / 7.1 / 48 kHz / 4049 kbps / 24-bit
  BPS             : 4049069
  BPS-eng         : 4049069
  DURATION        : 00:51:27.585000000
  DURATION-eng    : 00:51:27.585000000
  NUMBER_OF_FRAMES: 3705102
  NUMBER_OF_FRAMES-eng: 3705102
  NUMBER_OF_BYTES : 1562730694
  NUMBER_OF_BYTES-eng: 1562730694
  _STATISTICS_WRITING_APP: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_APP-eng: mkvmerge v9.4.2 ('So High') 64bit
  _STATISTICS_WRITING_DATE_UTC: 2016-11-03 04:12:51
  _STATISTICS_WRITING_DATE_UTC-eng: 2016-11-03 04:12:51
  _STATISTICS_TAGS: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES
  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES";
    var video = VideoInfoHelper.ParseVideoStream(Logger, output, output);
    Assert.AreEqual("h264", video.Codec);
    Assert.IsTrue(video.IsInterlaced);
  }
}

#endif