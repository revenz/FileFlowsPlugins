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
            var extracted = element.ExtractedSubtitles;

            Assert.AreEqual(1, output);
        }
    }
    
    [TestMethod]
    public void SubtitleExtractor_Extension_Test2()
    {
        var args = GetVideoNodeParameters(VideoSubtitles);
        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));
        
        foreach (string ext in new[] { string.Empty, ".srt", ".sup", ".ass" })
        {
            Logger.ILog("Extracting Extension: " + ext);
            SubtitleExtractor element = new();
            element.OutputFile = Path.Combine(TempPath, "subtitle.en" + ext);
            element.Language = "eng";

            element.PreExecute(args);
            int output = element.Execute(args);
            var extracted = element.ExtractedSubtitles;

            Assert.AreEqual(1, output);
        }
    }
    
    [TestMethod]
    public void French_Not_Canda()
    {
        var args = GetVideoNodeParameters(VideoSubtitles);
        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));
        
        SubtitleExtractor element = new();
        element.Title = "^(?!.*canad).*$";
        element.OutputFile = Path.Combine(TempPath, "subtitle");
        element.Language = "fre";
        element.ExtractAll = true;

        element.PreExecute(args);
        int output = element.Execute(args);
        Assert.AreEqual(1, output);
        
        string log = Logger.ToString();
        bool onlyOne = log.Contains("Extracted 1 subtitle");
        Assert.IsTrue(onlyOne);
        Assert.IsTrue(log.Contains("Title 'French (France)' does match"));
    }
    
    [TestMethod]
    public void French_Not_France()
    {
        var args = GetVideoNodeParameters(VideoSubtitles);
        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));
        
        SubtitleExtractor element = new();
        element.Title = "^(?!.*france).*$";
        element.OutputFile = Path.Combine(TempPath, "subtitle");
        element.Language = "fre";
        element.ExtractAll = true;

        element.PreExecute(args);
        int output = element.Execute(args);
        Assert.AreEqual(1, output);
        
        string log = Logger.ToString();
        bool onlyOne = log.Contains("Extracted 1 subtitle");
        Assert.IsTrue(onlyOne);
        Assert.IsTrue(log.Contains("Title 'French (Canada)' does match"));
    }
}



#endif