#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.AudioNodes.Tests;

[TestClass]
public class AudioInfoTests: AudioTestBase
{

    [TestMethod]
    public void AudioInfo_SplitTrack()
    {
        var args = GetAudioNodeParameters();
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
        var args = GetAudioNodeParameters();
        var AudioInfo = new AudioInfoHelper(FFprobe, FFprobe, Logger).Read(args.WorkingFile);

        Assert.AreEqual(8, AudioInfo.Value.Track);
    }

    [TestMethod]
    public void AudioInfo_GetMetaData()
    {
        var args = GetAudioNodeParameters();

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
        var args = GetAudioNodeParameters(AudioOgg);

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

    [TestMethod]
    public void FFmpegParse_DateYYMMDD_Test()
    {
        string output = @"[mov,mp4,m4a,3gp,3g2,mj2 @ 0x556b9ec1aa80] stream 0, timescale not set
Input #0, mov,mp4,m4a,3gp,3g2,mj2, from '/media/Storage/old songs m4a_flac/Diversity/Tom Budin & DLMT - Right Now.m4a':
  Metadata:
    major_brand     : M4A
    minor_version   : 512
    compatible_brands: M4A isomiso2
    title           : Tom Budin & DLMT - Right Now
    artist          : Diversity
    date            : 20180114
    encoder         : Lavf61.1.100
    comment         : https://www.youtube.com/watch?v=xxb-7pBN_gA
    description     : Stream/Download:
                    : ➥ https://fanlink.to/sf023
                    :
                    : DLMT
                    : • https://soundcloud.com/dlmtmusic
                    : • https://www.facebook.com/dlmtmusic
                    : • https://twitter.com/dlmtmusic
                    : • https://www.instagram.com/dlmtmusic/
                    :
                    : Tom Budin
                    : • https://soundcloud.com/tombudinmusic
                    : • https://www.facebook.com/TomBudinMusic/
                    : • https://www.youtube.com/user/tombudinmusic
                    : • https://twitter.com/TomBudinMusic
                    :
                    : Strange Fruits
                    : • https://strangefruits.net/
                    : • https://soundcloud.com/strangefruitsmusic
                    : • https://www.facebook.com/strangefruitsmusic/
                    : • https://www.youtube.com/channel/UCnMuO9UobvCU-dHp4o7OSWQ
                    : • https://www.instagram.com/strangefruitsmusic/
                    :
                    :
                    :
                    : Artwork by Koyoriin
                    : • https://www.pixiv.net/member.php?id=12576068
                    : • http:/facebook.com/koyorinart
                    : • http://twitter.com/koyoriin
                    : • http://koyoriin.tumblr.com
                    : • http://instagram.com/koyori_n
                    :
                    : ➥ https://www.pixiv.net/member_illust.php?mode=medium&illust_id=60596169
                    :
                    :
                    :
                    : Diversity
                    : • http://diversity.moe
                    : • http://diversity.moe/youtube
                    : • http://diversity.moe/facebook
                    : • http://diversity.moe/instagram
                    : • http://diversity.moe/twitter
                    : • http://diversity.moe/spotify
                    : • http://diversity.moe/soundcloud
                    :
                    : Diversity's Spotify Playlist:
                    : • https://open.spotify.com/user/diversity.recordings/playlist/04PjsPIpNYKHNnm3UdUDKY
                    :
                    : Merchandise:
                    : • http://diversity.moe/merchandise
                    :
                    :
                    : Copyright/Claims/Issues: info@diversityrecordings.com
    synopsis        : Stream/Download:
                    : ➥ https://fanlink.to/sf023
                    :
                    : DLMT
                    : • https://soundcloud.com/dlmtmusic
                    : • https://www.facebook.com/dlmtmusic
                    : • https://twitter.com/dlmtmusic
                    : • https://www.instagram.com/dlmtmusic/
                    :
                    : Tom Budin
                    : • https://soundcloud.com/tombudinmusic
                    : • https://www.facebook.com/TomBudinMusic/
                    : • https://www.youtube.com/user/tombudinmusic
                    : • https://twitter.com/TomBudinMusic
                    :
                    : Strange Fruits
                    : • https://strangefruits.net/
                    : • https://soundcloud.com/strangefruitsmusic
                    : • https://www.facebook.com/strangefruitsmusic/
                    : • https://www.youtube.com/channel/UCnMuO9UobvCU-dHp4o7OSWQ
                    : • https://www.instagram.com/strangefruitsmusic/
                    :
                    :
                    :
                    : Artwork by Koyoriin
                    : • https://www.pixiv.net/member.php?id=12576068
                    : • http:/facebook.com/koyorinart
                    : • http://twitter.com/koyoriin
                    : • http://koyoriin.tumblr.com
                    : • http://instagram.com/koyori_n
                    :
                    : ➥ https://www.pixiv.net/member_illust.php?mode=medium&illust_id=60596169
                    :
                    :
                    :
                    : Diversity
                    : • http://diversity.moe
                    : • http://diversity.moe/youtube
                    : • http://diversity.moe/facebook
                    : • http://diversity.moe/instagram
                    : • http://diversity.moe/twitter
                    : • http://diversity.moe/spotify
                    : • http://diversity.moe/soundcloud
                    :
                    : Diversity's Spotify Playlist:
                    : • https://open.spotify.com/user/diversity.recordings/playlist/04PjsPIpNYKHNnm3UdUDKY
                    :
                    : Merchandise:
                    : • http://diversity.moe/merchandise
                    :
                    :
                    : Copyright/Claims/Issues: info@diversityrecordings.com
  Duration: 00:03:12.09, start: 0.000000, bitrate: 442 kb/s
  Stream #0:0[0x1](eng): Audio: aac (LC) (mp4a / 0x6134706D), 48000 Hz, stereo, fltp, 421 kb/s (default)
    Metadata:
      handler_name    : SoundHandler
      vendor_id       : [0][0][0][0]
  Stream #0:1[0x0]: Video: png, rgb24(pc, gbr/unknown/unknown), 1280x720, 90k tbr, 90k tbn (attached pic)";
        var helper = new AudioInfoHelper(FFmpeg, FFprobe, Logger);
        var mi = new AudioInfo();
        helper.ParseFFmpegOutput(mi, output, "/media/Storage/old songs m4a_flac/Diversity/Tom Budin & DLMT - Right Now.m4a");
        
        Assert.AreEqual(2018, mi.Date.Year);
        Assert.AreEqual(1, mi.Date.Month);
        Assert.AreEqual(14, mi.Date.Day);
    }
}

#endif