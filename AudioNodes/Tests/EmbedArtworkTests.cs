#if(DEBUG)

using FileFlows.AudioNodes;
using FileFlows.AudioNodes.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudioNodes.Tests;

[TestClass]
public class EmbedArtworkTests : AudioTestBase
{

    [TestMethod]
    public void SingleArtwork()
    {
        var logger = new TestLogger();
        const string file = "/home/john/Music/unprocessed/Aqua/Aquarium (1997)/Aqua - Aquarium - 03 - Barbie Girl.flac";
        var args = new NodeParameters(file, logger, false, string.Empty, new LocalFileService())
        {
            LibraryFileName = file
        };
        
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"/home/john/Music/temp";
        var ele = new EmbedArtwork();
        var output = ele.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void CovertArtwork()
    {
        var logger = new TestLogger();
        //const string file = "/home/john/Music/unprocessed/Aqua/Aquarium (1997)/zombie.mp3";
        const string file =
            "/home/john/Music/unprocessed/Aqua/Aquarius (2000)/Aqua - Aquarius - 01 - Cartoon Heroes.flac";
        var args = new NodeParameters(file, logger, false, string.Empty, new LocalFileService())
        {
            LibraryFileName = file
        };
        
        args.GetToolPathActual = (string tool) => ffmpeg;
        args.TempPath = @"/home/john/Music/temp";
        var ele = new EmbedArtwork();
        var output = ele.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void Ogg()
        => ConvertAudio(new ConvertToOGG() { CustomExtension = "mp4" });
    [TestMethod]
    public void AAC()
        => ConvertAudio(new ConvertToAAC());
    [TestMethod]
    public void MP3()
        => ConvertAudio(new ConvertToMP3());
    // [TestMethod] public void Flac()
    //     => ConvertAudio(new ConvertToFlac());
    
    
    void ConvertAudio(Node convertNode)
    {
        //const string file = "/home/john/Music/unprocessed/Aqua/Aquarium (1997)/zombie.mp3";
        const string file =
            "/home/john/Music/unprocessed/Aqua/Aquarius (2000)/Aqua - Aquarius - 01 - Cartoon Heroes.flac";
        var args = GetNodeParameters(file);

        var audioFile = new AudioFile();
        audioFile.PreExecute(args);
        audioFile.Execute(args);

        convertNode.PreExecute(args);
        var result = convertNode.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(1, result);
        logger.Clear();
        
        var ele = new EmbedArtwork();
        var output = ele.Execute(args);

        log = logger.ToString();
        TestContext.WriteLine(log);
        Assert.AreEqual(1, output);
        System.IO.File.Move(args.WorkingFile,
            FileHelper.Combine(FileHelper.GetDirectory(args.WorkingFile),
                convertNode.GetType().Name + FileHelper.GetExtension(args.WorkingFile)), true);
    }
}

#endif