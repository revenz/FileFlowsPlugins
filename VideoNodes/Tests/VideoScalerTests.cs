#if(DEBUG)

namespace VideoNodes.Tests
{
    using FileFlows.VideoNodes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class VideoScalerTests
    {
        [TestMethod]
        public void VideoScaler_Resolution_Tests()
        {
            const string file = @"D:\videos\problemfile\sample fileflows.mkv";            
            VideoScaler node = new();

            foreach (var test in new[]
            {
                // 480p
                (600, 640, 2),
                (640, 640, 2),
                (700, 640, 2),
                (599, 640, -1),
                (701, 640, -1),
                
                // 720p
                (1280, 1280, 2),
                (1220, 1280, 2),
                (1340, 1280, 2),
                (1219, 1280, -1),
                (1341, 1280, -1),
                
                // 1080p
                (1860, 1920, 2),
                (1920, 1920, 2),
                (1980, 1920, 2),
                (1859, 1920, -1),
                (1981, 1920, -1),
                
                // 4k
                (3780, 3840, 2),
                (3840, 3840, 2),
                (3900, 3840, 2),
                (3779, 3840, -1),
                (3901, 3840, -1),
            })
            {
                node.Resolution = test.Item2 + ":-2";

                var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
                args.Parameters.Add("VideoInfo", new VideoInfo
                {
                    VideoStreams = new List<VideoStream>
                    {
                        new VideoStream
                        {
                            Width = test.Item1
                        }
                    }
                });

                int output = node.Execute(args);
                Assert.AreEqual(test.Item3, output);
            }
        }

        [TestMethod]
        public void VideoScaler_Force_Tests()
        {
            const string file = @"D:\videos\problemfile\sample fileflows.mkv";
            VideoScaler node = new();

            foreach (var test in new[]
            {
                // 480p
                (600, 640, 2),
                (640, 640, 2),
                (700, 640, 2),
                (599, 640, -1),
                (701, 640, -1),
                
                // 720p
                (1280, 1280, 2),
                (1220, 1280, 2),
                (1340, 1280, 2),
                (1219, 1280, -1),
                (1341, 1280, -1),
                
                // 1080p
                (1860, 1920, 2),
                (1920, 1920, 2),
                (1980, 1920, 2),
                (1859, 1920, -1),
                (1981, 1920, -1),
                
                // 4k
                (3780, 3840, 2),
                (3840, 3840, 2),
                (3900, 3840, 2),
                (3779, 3840, -1),
                (3901, 3840, -1),
            })
            {
                node.Resolution = test.Item2 + ":-2";
                node.Force = true;

                var args = new FileFlows.Plugin.NodeParameters(file, new TestLogger(), false, string.Empty);
                args.Parameters.Add("VideoInfo", new VideoInfo
                {
                    VideoStreams = new List<VideoStream>
                    {
                        new VideoStream
                        {
                            Width = test.Item1
                        }
                    }
                });

                int output = node.Execute(args);
                Assert.AreEqual(-1, output);
            }
        }
    }
}


#endif