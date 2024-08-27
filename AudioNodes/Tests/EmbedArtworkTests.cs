#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.AudioNodes.Tests;

[TestClass]
public class EmbedArtworkTests : AudioTestBase
{

    [TestMethod]
    public void SingleArtwork()
    {
        var args = GetAudioNodeParameters();
        var ele = new EmbedArtwork();
        ele.PreExecute(args);
        var output = ele.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void CovertArtwork()
    {
        var args = GetAudioNodeParameters(AudioFlac);
        
        var ele = new EmbedArtwork();
        ele.PreExecute(args);
        var output = ele.Execute(args);

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
        var args = GetAudioNodeParameters(AudioFlac);

        var audioFile = new AudioFile();
        audioFile.PreExecute(args);
        audioFile.Execute(args);

        convertNode.PreExecute(args);
        var result = convertNode.Execute(args);
        Assert.AreEqual(1, result);
        
        var ele = new EmbedArtwork();
        var output = ele.Execute(args);

        Assert.AreEqual(1, output);
        System.IO.File.Move(args.WorkingFile,
            FileHelper.Combine(FileHelper.GetDirectory(args.WorkingFile),
                convertNode.GetType().Name + FileHelper.GetExtension(args.WorkingFile)), true);
    }
}

#endif