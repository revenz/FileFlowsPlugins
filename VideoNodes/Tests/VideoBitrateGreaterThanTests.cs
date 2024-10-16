#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VideoNodes.Tests;

[TestClass]
public class VideoBitrateGreaterThanTests : VideoTestBase
{
    [TestMethod]
    public void IsGreaterThan_NoAudio_NoVideoBitrate()
    {
        var args = new NodeParameters(Logger)
        {
            Parameters = new()
            {
                {
                    VideoNode.VIDEO_INFO, new VideoInfo()
                    {
                        Bitrate = 10_001 * 1000,
                        VideoStreams = new ()
                        {
                            new (){}
                        }
                    }
                }
            }
        };
        
        var element = new VideoBitrateGreaterThan();
        element.Bitrate = 10_000;
        var result = element.Execute(args);
        Assert.AreEqual(1, result);
    }
    
    [TestMethod]
    public void IsNotGreaterThan_NoAudio_NoVideoBitrate()
    {
        var args = new NodeParameters(Logger)
        {
            Parameters = new()
            {
                {
                    VideoNode.VIDEO_INFO, new VideoInfo()
                    {
                        Bitrate = 10_000 * 1000,
                        VideoStreams = new ()
                        {
                            new (){}
                        }
                    }
                }
            }
        };
        
        var element = new VideoBitrateGreaterThan();
        element.Bitrate = 10_001;
        var result = element.Execute(args);
        Assert.AreEqual(2, result);
    }
    
    [TestMethod]
    public void IsNotGreaterThan_WithAudio_NoVideoBitrate()
    {
        var args = new NodeParameters(Logger)
        {
            Parameters = new()
            {
                {
                    VideoNode.VIDEO_INFO, new VideoInfo()
                    {
                        Bitrate = 5001 * 1000,
                        VideoStreams = new ()
                        {
                            new (){}
                        },
                        AudioStreams = new ()
                        {
                            new ()
                            {
                                Bitrate = 2000
                            }
                        }
                    }
                }
            }
        };
        var estimated = (5001 * 1000) - (2000) - (5001 * 0.05);
        Logger.ILog($"Test Estimated Bitrate: {estimated} BPS / {estimated / 1000} KBps");
        
        var element = new VideoBitrateGreaterThan();
        element.Bitrate = (int)(estimated / 1000f) + 1;
        var result = element.Execute(args);
        Assert.AreEqual(2, result);
    }
    
    [TestMethod]
    public void IsGreaterThan_WithAudio_NoVideoBitrate()
    {
        var args = new NodeParameters(Logger)
        {
            Parameters = new()
            {
                {
                    VideoNode.VIDEO_INFO, new VideoInfo()
                    {
                        Bitrate = 5001 * 1000,
                        VideoStreams = new ()
                        {
                            new (){}
                        },
                        AudioStreams = new ()
                        {
                            new ()
                            {
                                Bitrate = 2000
                            }
                        }
                    }
                }
            }
        };
        var estimated = Math.Round((5001 * 1000) - (2000) - (5001 * 0.05));
        Logger.ILog($"Test Estimated Bitrate: {estimated} BPS / {estimated / 1000} KBps");
        
        var element = new VideoBitrateGreaterThan();
        element.Bitrate = (int)Math.Round(estimated / 1000f) - 1;
        var result = element.Execute(args);
        Assert.AreEqual(1, result);
    }
    
    
    [TestMethod]
    public void LogTests()
    {
        var output = VideoBitrateGreaterThan.CheckBitrate(Logger, 120144000, 15000);
        Assert.AreEqual(1, output);
    }

}

#endif