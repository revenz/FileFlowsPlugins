#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VideoFile = FileFlows.VideoNodes.VideoFile;

namespace VideoNodes.Tests;

[TestClass]
public class SubtitleExtractorTests: TestBase
{
    [TestMethod]
    public void SubtitleExtractor_Extension_Test()
    {
        string file = TestFile_BasicMkv;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        foreach (string ext in new[] { String.Empty, ".srt", ".sup" })
        {
            SubtitleExtractor node = new();
            node.OutputFile = Path.Combine(TempPath, "subtitle.en" + ext);
            node.Language = "eng";

            var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
            args.GetToolPathActual = (string tool) => FfmpegPath;
            args.TempPath = TempPath;

            Assert.AreEqual(1, new VideoFile().Execute(args));

            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }
    }

    [TestMethod]
    public void SubtitleExtractor_Pgs_Test()
    {
        string file = TestFile_Pgs;
        var vi = new VideoInfoHelper(FfmpegPath, new TestLogger());
        var vii = vi.Read(file);

        foreach (string ext in new[] { string.Empty, ".srt", ".sup" })
        {
            SubtitleExtractor node = new();
            node.ForcedOnly = true;
            node.OutputFile = Path.Combine(TempPath, "subtitle.en" + ext);
            node.Language = "eng";

            var args = new NodeParameters(file, new TestLogger(), false, string.Empty, null);;
            args.GetToolPathActual = (string tool) => FfmpegPath;
            args.TempPath = TempPath;

            var vf = new VideoFile();
            vf.PreExecute(args);
            Assert.AreEqual(1, vf.Execute(args));

            int output = node.Execute(args);

            Assert.AreEqual(1, output);
        }
    }
    
    
    [TestMethod]
    public void Webvtt_Extract()
    {
        string file = TestFile_Webvtt;

        var args = new NodeParameters(file, new TestLogger(), false, string.Empty, new LocalFileService());;
        args.GetToolPathActual = (string tool) => FfmpegPath;
        args.TempPath = TempPath;
        
        var vf = new VideoFile();
        vf.PreExecute(args);
        Assert.AreEqual(1, vf.Execute(args));

        SubtitleExtractor extractor = new();
        extractor.OutputFile = Path.Combine(TempPath, "subtitle.srt");
        extractor.ExtractAll = true;
        extractor.PreExecute(args);
        int output = extractor.Execute(args);

        Assert.AreEqual(1, output);
    }
}



#endif