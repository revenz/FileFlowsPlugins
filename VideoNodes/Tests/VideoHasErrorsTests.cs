#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VideoNodes.Tests;

[TestClass]
public class VideoHasErrorsTests : VideoTestBase
{
    [TestMethod]
    public void VideoHasErrors_Video()
    {
        var args = GetVideoNodeParameters(VideoCorrupt);

        VideoFile vf = new();
        vf.PreExecute(args);
        vf.Execute(args);
        
        VideoHasErrors element = new();
        element.PreExecute(args);
        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }
}


#endif