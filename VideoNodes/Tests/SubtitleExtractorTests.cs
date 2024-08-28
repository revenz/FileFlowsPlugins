#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VideoFile = FileFlows.VideoNodes.VideoFile;

namespace VideoNodes.Tests;

[TestClass]
public class SubtitleExtractorTests: VideoTestBase
{
    [TestMethod]
    public void SubtitleExtractor_Extension_Test()
    {
        var args = GetVideoNodeParameters(VideoMkv);
        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));
        
        foreach (string ext in new[] { string.Empty, ".srt", ".sup" })
        {
            Logger.ILog("Extracting Extension: " + ext);
            SubtitleExtractor element = new();
            element.OutputFile = Path.Combine(TempPath, "subtitle.en" + ext);
            element.Language = "eng";

            element.PreExecute(args);
            int output = element.Execute(args);

            Assert.AreEqual(1, output);
        }
    }
}



#endif