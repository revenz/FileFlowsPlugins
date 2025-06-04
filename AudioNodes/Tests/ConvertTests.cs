#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
namespace FileFlows.AudioNodes.Tests;

[TestClass]
public class ConvertTests : AudioTestBase
{
    [TestMethod]
    public void Convert_FlacToAac()
    {
        foreach (var codec in new[] { "MP3", "aac"} )
        {
            foreach (int quality in new[] { 0, 10 })
            {
                ConvertAudio node = new();
                node.Codec = codec;
                node.Bitrate = quality + 10;
                node.HighEfficiency = true;
                var args = GetAudioNodeParameters(AudioFlac);
                var af = new AudioFile();
                Assert.IsTrue(af.PreExecute(args));
                af.Execute(args); // need to read the Audio info and set it
                Assert.IsTrue(node.PreExecute(args));
                int output = node.Execute(args);

                Assert.AreEqual(1, output);
                var fi = new FileInfo(args.WorkingFile);
                File.Move(args.WorkingFile, Path.Combine(fi.DirectoryName, quality + fi.Extension), true);
            }
        }
    }

    [TestMethod]
    public void Convert_FlacToMp3()
    {
        var args = GetAudioNodeParameters(AudioFlacMetadata);
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it

        var info = args.Parameters["AudioInfo"] as AudioInfo;
        Assert.IsNotNull(info);
        Assert.AreEqual("Test Artist", info.Artist);
        Assert.AreEqual("Test Album", info.Album);
        Assert.AreEqual("Unit Test Song", info.Title);
        Assert.AreEqual(2023, info.Date.Year);
        Assert.AreEqual("Electronic", info.Genres.FirstOrDefault());
        ConvertToMP3 node = new();
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
        var updatedInfo = args.Parameters["AudioInfo"] as AudioInfo;
        Assert.IsNotNull(updatedInfo);
        Assert.AreEqual("Test Artist", updatedInfo.Artist);
        Assert.AreEqual("Test Album", updatedInfo.Album);
        Assert.AreEqual("Unit Test Song", updatedInfo.Title);
        Assert.AreEqual(2023, updatedInfo.Date.Year);
        Assert.AreEqual("Electronic", updatedInfo.Genres.FirstOrDefault());
    }
    
    [TestMethod]
    public void Convert_FlacToAacMetadata()
    {
        var args = GetAudioNodeParameters(AudioFlacMetadata);
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it

        var info = args.Parameters["AudioInfo"] as AudioInfo;
        Assert.IsNotNull(info);
        Assert.AreEqual("Test Artist", info.Artist);
        Assert.AreEqual("Test Album", info.Album);
        Assert.AreEqual("Unit Test Song", info.Title);
        Assert.AreEqual(2023, info.Date.Year);
        Assert.AreEqual("Electronic", info.Genres.FirstOrDefault());
        ConvertToAAC node = new();
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
        var updatedInfo = args.Parameters["AudioInfo"] as AudioInfo;
        Assert.IsNotNull(updatedInfo);
        Assert.AreEqual("Test Artist", updatedInfo.Artist);
        Assert.AreEqual("Test Album", updatedInfo.Album);
        Assert.AreEqual("Unit Test Song", updatedInfo.Title);
        Assert.AreEqual(2023, updatedInfo.Date.Year);
        Assert.AreEqual("Electronic", updatedInfo.Genres.FirstOrDefault());
    }
    
    [TestMethod]
    public void Convert_FlacToAlac()
    {
        var args = GetAudioNodeParameters(AudioFlac);
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it
        
        ConvertToALAC node = new();
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void Convert_Mp3ToFlac()
    {
        var args = GetAudioNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it
        
        ConvertToFLAC node = new();
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void Convert_Mp3ToWAV()
    {
        var args = GetAudioNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it
        
        ConvertToWAV node = new();
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void Convert_Mp3ToOgg()
    {
        var args = GetAudioNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it

        var ele = new ConvertAudio();
        ele.Codec = "ogg";
        ele.Bitrate = 320;
        ele.PreExecute(args);
        int output = ele.Execute(args);

        Assert.AreEqual(1, output);
    }


    [TestMethod]
    public void Convert_AacHighEfficient()
    {
        var args = GetAudioNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it
        
        ConvertToAAC ele = new();
        ele.HighEfficiency = true;
        ele.Bitrate = 192;
        ele.PreExecute(args);
        int output = ele.Execute(args);

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void Convert_Mp3ToMp3_Bitrate()
    {
        var args = GetAudioNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it
        
        ConvertToMP3 ele = new();
        ele.PreExecute(args);
        ele.Bitrate = 64; 
        int output = ele.Execute(args);
        TestContext.WriteLine(Logger.ToString());

        Assert.AreEqual(1, output);
    }
    
    [TestMethod]
    public void Convert_Mp3ToMp3_Bitrate_Variable()
    {
        var args = GetAudioNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it
        
        ConvertToMP3 ele = new();
        ele.PreExecute(args);
        ele.Bitrate = 3; 
        int output = ele.Execute(args);
        TestContext.WriteLine(Logger.ToString());

        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void Convert_Mp3_AlreadyMp3()
    {
        var args = GetAudioNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it
        
        ConvertAudio node = new();
        node.SkipIfCodecMatches = true;
        node.Codec = "mp3";

        node.Bitrate = 192;
        int output = node.Execute(args);

        Assert.AreEqual(2, output);
    }

    [TestMethod]
    public void Convert_TwoPass()
    {
        var args = GetAudioNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        af.Execute(args); // need to read the Audio info and set it
        
        ConvertToAAC node = new();
        node.Normalize = true;
        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(1, output);
    }
}

#endif