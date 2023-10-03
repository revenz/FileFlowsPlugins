#if(DEBUG)
using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VideoNodes.Tests;

[TestClass]
public class VideoIsInterlacedTests
{
    [TestMethod]
    public void NotInterlaced()
    {
        var logger = new TestLogger();
        
        string ffmpegOutput = @"
      NUMBER_OF_BYTES : 3632640
      _STATISTICS_WRITING_APP: mkvmerge v60.0.0 ('Are We Copies?') 64-bit
      _STATISTICS_WRITING_DATE_UTC: 2022-12-07 21:09:16
      DURATION        : 00:00:45.438000000
      encoder         : Lavc60.3.100 pcm_s16le
frame=    1 fps=0.0 q=-0.0 size=N/A time=00:00:00.63 bitrate=N/A speed=12.1x    
frame=  196 fps=0.0 q=-0.0 size=N/A time=00:00:08.83 bitrate=N/A speed=15.9x    
frame=  403 fps=382 q=-0.0 size=N/A time=00:00:17.50 bitrate=N/A speed=16.6x    
frame=  607 fps=390 q=-0.0 size=N/A time=00:00:25.98 bitrate=N/A speed=16.7x    
frame=  807 fps=392 q=-0.0 size=N/A time=00:00:34.33 bitrate=N/A speed=16.7x    
frame= 1012 fps=395 q=-0.0 size=N/A time=00:00:42.84 bitrate=N/A speed=16.7x    
frame= 1089 fps=397 q=-0.0 Lsize=N/A time=00:00:45.40 bitrate=N/A speed=16.5x    
video:510kB audio:25542kB subtitle:0kB other streams:0kB global headers:0kB muxing overhead: unknown
[Parsed_idet_0 @ 0x5618bf1fba40] Repeated Fields: Neither:  1089 Top:     0 Bottom:     0
[Parsed_idet_0 @ 0x5618bf1fba40] Single frame detection: TFF:     0 BFF:     0 Progressive:   374 Undetermined:   715
[Parsed_idet_0 @ 0x5618bf1fba40] Multi frame detection: TFF:     0 BFF:     0 Progressive:  1089 Undetermined:     0";
        
        bool interlaced = VideoIsInterlaced.IsVideoInterlaced(logger, ffmpegOutput, 10);
        var log = logger.ToString();
        
        Assert.IsFalse(interlaced);
        Assert.IsTrue(log.Contains("Total Progressive Frames: " + (374 + 1089)));
    }
    
    
    [TestMethod]
    public void IsInterlaced()
    {
        var logger = new TestLogger();
        
        string ffmpegOutput = @"
      NUMBER_OF_BYTES : 3632640
      _STATISTICS_WRITING_APP: mkvmerge v60.0.0 ('Are We Copies?') 64-bit
      _STATISTICS_WRITING_DATE_UTC: 2022-12-07 21:09:16
      DURATION        : 00:00:45.438000000
      encoder         : Lavc60.3.100 pcm_s16le
frame=    1 fps=0.0 q=-0.0 size=N/A time=00:00:00.63 bitrate=N/A speed=12.1x    
frame=  196 fps=0.0 q=-0.0 size=N/A time=00:00:08.83 bitrate=N/A speed=15.9x    
frame=  403 fps=382 q=-0.0 size=N/A time=00:00:17.50 bitrate=N/A speed=16.6x    
frame=  607 fps=390 q=-0.0 size=N/A time=00:00:25.98 bitrate=N/A speed=16.7x    
frame=  807 fps=392 q=-0.0 size=N/A time=00:00:34.33 bitrate=N/A speed=16.7x    
frame= 1012 fps=395 q=-0.0 size=N/A time=00:00:42.84 bitrate=N/A speed=16.7x    
frame= 1089 fps=397 q=-0.0 Lsize=N/A time=00:00:45.40 bitrate=N/A speed=16.5x    
video:510kB audio:25542kB subtitle:0kB other streams:0kB global headers:0kB muxing overhead: unknown
[Parsed_idet_0 @ 0x5618bf1fba40] Repeated Fields: Neither:  1089 Top:     0 Bottom:     0
[Parsed_idet_0 @ 0x5618bf1fba40] Single frame detection: TFF:     420 BFF:     0 Progressive:   374 Undetermined:   715
[Parsed_idet_0 @ 0x5618bf1fba40] Multi frame detection: TFF:     65 BFF:     0 Progressive:  1089 Undetermined:     0";
        
        bool interlaced = VideoIsInterlaced.IsVideoInterlaced(logger, ffmpegOutput, 10);
        var log = logger.ToString();
        
        Assert.IsTrue(interlaced);
        Assert.IsTrue(log.Contains("Total Progressive Frames: " + (374 + 1089)));
        Assert.IsTrue(log.Contains("Total Interlaced Frames: " + (420 + 65)));
    }
}

#endif