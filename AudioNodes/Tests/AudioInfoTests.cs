#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using AudioNodes.Tests;
using FileFlows.AudioNodes.Helpers;
using Moq;

namespace FileFlows.AudioNodes.Tests;
[TestClass]
public class AudioInfoTests: AudioTestBase
{

    [TestMethod]
    public void AudioInfo_SplitTrack()
    {
        var args = GetNodeParameters();
        var af = new AudioFile();
        af.PreExecute(args);
        var result = af.Execute(args); // need to read the Audio info and set it

        Assert.AreEqual(1, result);

        var AudioInfo = args.Parameters["AudioInfo"] as AudioInfo;

        Assert.AreEqual(8, AudioInfo.Track);
    }

    [TestMethod]
    public void AudioInfo_NormalTrack()
    {
        var args = GetNodeParameters();
        var AudioInfo = new AudioInfoHelper(FFprobe, FFprobe, Logger).Read(args.WorkingFile);

        Assert.AreEqual(8, AudioInfo.Value.Track);
    }

    [TestMethod]
    public void AudioInfo_GetMetaData()
    {
        var args = GetNodeParameters();

        // load the variables
        var audioFile = new AudioFile();
        audioFile.PreExecute(args);
        Assert.AreEqual(1, audioFile.Execute(args));

        var audio = new AudioInfoHelper(FFmpeg, FFprobe, Logger).Read(args.WorkingFile).Value;

        string folder = args.ReplaceVariables("{audio.ArtistThe} ({audio.Year})");
        Assert.AreEqual($"{audio.Artist} ({audio.Date.Year})", folder);

        string fname = args.ReplaceVariables("{audio.Artist} - {audio.Album} - {audio.Track|##} - {audio.Title}");
        Assert.AreEqual($"{audio.Artist} - {audio.Album} - {audio.Track:00} - {audio.Title}", fname);
    }

    [TestMethod]
    public void AudioInfo_FileNameMetadata()
    {
        var audio = new AudioInfo();

        string file =
            "/media/Meat Loaf/Bat out of Hell II- Back Into Hell… (1993)/03 - I’d Do Anything for Love (but I Won’t Do That).mp3";

        new AudioInfoHelper(FFmpeg, FFprobe, Logger).ParseFileNameInfo(file, audio);

        Assert.AreEqual("Meat Loaf", audio.Artist);
        Assert.AreEqual("Bat out of Hell II- Back Into Hell…", audio.Album);
        Assert.AreEqual(1993, audio.Date.Year);
        Assert.AreEqual("I’d Do Anything for Love (but I Won’t Do That)", audio.Title);
        Assert.AreEqual(3, audio.Track);
    }

    [TestMethod]
    public void AudioInfo_Bitrate()
    {
        var args = GetNodeParameters(AudioOgg);

        // load the variables
        var audioFile = new AudioFile();
        audioFile.PreExecute(args);
        Assert.AreEqual(1, audioFile.Execute(args));
        
        // convert to 192
        var convert = new ConvertAudio();
        convert.Bitrate = 192;
        convert.SkipIfCodecMatches = false;
        convert.Codec = "mp3";
        convert.PreExecute(args);
        int result = convert.Execute(args);
        Assert.AreEqual(1, result);

        var audio = new AudioInfoHelper(FFmpeg, FFprobe, Logger).Read(args.WorkingFile).Value;
        Assert.AreEqual(192 * 1024, audio.Bitrate);

        var md = new Dictionary<string, object>();
        convert.SetAudioInfo(args, audio, md);
        
        Assert.AreEqual((192 * 1024).ToString(), md["audio.Bitrate"].ToString());
        
        // converting again should skip
        convert = new();
        convert.SkipIfCodecMatches = false;
        convert.Codec = "mp3";
        convert.Bitrate = 192;
        convert.PreExecute(args);
        result = convert.Execute(args);
        Assert.AreEqual(2, result);
    }
    
    [TestMethod]
    public void AudioFormatInfoTest()
    {
        string ffmpegOutput = @"{
            ""format"": {
                ""filename"": ""Aqua - Aquarium - 03 - Barbie Girl.flac"",
                ""nb_streams"": 1,
                ""nb_programs"": 0,
                ""format_name"": ""flac"",
                ""format_long_name"": ""raw FLAC"",
                ""start_time"": ""0.000000"",
                ""duration"": ""197.906667"",
                ""size"": ""25955920"",
                ""bit_rate"": ""1049218"",
                ""probe_score"": 100,
                ""tags"": {
                    ""TITLE"": ""Barbie Girl"",
                    ""ARTIST"": ""Aqua"",
                    ""ALBUM"": ""Aquarium"",
                    ""track"": ""3"",
                    ""DATE"": ""1997"",
                    ""GENRE"": ""Eurodance"",
                    ""TOTALTRACKS"": ""11"",
                    ""disc"": ""1"",
                    ""TOTALDISCS"": ""1""
                }
            }
        }";

        var result = FFprobeAudioInfo.Parse(ffmpegOutput);
        Assert.IsFalse(result.IsFailed);
        var audioFormatInfo = result.Value;

        Assert.AreEqual(1049218, audioFormatInfo.Bitrate);
        Assert.AreEqual("Barbie Girl", audioFormatInfo.Tags?.Title);
        Assert.AreEqual("Aqua", audioFormatInfo.Tags?.Artist);
        Assert.AreEqual("3", audioFormatInfo.Tags?.Track);
    }
}

#endif