using AudioNodes.Tests;

#if(DEBUG)

namespace FileFlows.AudioNodes.Tests;

using FileFlows.AudioNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

[TestClass]
public class AudioFileNormalizationTests : AudioTestBase
{
    [TestMethod]
    public void AudioFileNormalization_Mp3()
    {
        var args = GetNodeParameters();
        
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
        var args = GetNodeParameters(AudioFlac);

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