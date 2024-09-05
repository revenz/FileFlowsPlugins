#if(DEBUG)

namespace FileFlows.AudioNodes.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class AudioFileNormalizationTests : AudioTestBase
{
    [TestMethod]
    public void AudioFileNormalization_Mp3()
    {
        var args = GetAudioNodeParameters();
        
        var audioFile = new AudioFile();
        audioFile.PreExecute(args);
        audioFile.Execute(args); // need to read the Audio info and set it

        AudioFileNormalization node = new();
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void AudioFileNormalization_ConvertFlacToMp3()
    {
        var args = GetAudioNodeParameters(AudioFlac);

        var audioFile = new AudioFile();
        audioFile.PreExecute(args);
        audioFile.Execute(args); // need to read the Audio info and set it0

        ConvertToMP3 convertNode = new();
        convertNode.PreExecute(args);
        int output = convertNode.Execute(args);

        AudioFileNormalization normalNode = new();
        normalNode.PreExecute(args);
        output = normalNode.Execute(args);

        Assert.AreEqual(1, output);
    }
}

#endif