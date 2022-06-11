#if(DEBUG)

using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace VideoNodes.Tests;

[TestClass]
public abstract class TestBase
{
    public string TestPath { get; private set; }
    public string TempPath { get; private set; }
    public string FfmpegPath { get; private set; }

    [TestInitialize]
    public void TestInitialize()
    {
        if (File.Exists("../../../test.settings.dev.json"))
        {
            LoadSettings("../../../test.settings.dev.json");
        }
        else if (File.Exists("../../../test.settings.json"))
        {
            LoadSettings("../../../test.settings.json");
        }
        this.TestPath = this.TestPath?.EmptyAsNull() ?? @"C:\videos\testfiles";
        this.TempPath = this.TestPath?.EmptyAsNull() ?? @"C:\videos\temp";
        this.FfmpegPath = this.FfmpegPath?.EmptyAsNull() ?? @"C:\utils\ffmpeg\ffmpeg.exe";
        if (Directory.Exists(this.TempPath) == false)
            Directory.CreateDirectory(this.TempPath);
    }

    private void LoadSettings(string filename)
    {
        try
        {
            string json = File.ReadAllText(filename);
            var settings = JsonSerializer.Deserialize<TestSettings>(json);
            this.TestPath = settings.TestPath;
            this.TempPath = settings.TempPath;
            this.FfmpegPath = settings.FfmpegPath;
        }
        catch (Exception) { }
    }

    protected virtual void TestStarting()
    {

    }

    protected string TestFile_BasicMkv => Path.Combine(TestPath, "basic.mkv");
    protected string TestFile_Tag => Path.Combine(TestPath, "tag.mp4");
    protected string TestFile_Pgs => Path.Combine(TestPath, "pgs.mkv");
    protected string TestFile_TwoPassNegInifinity => Path.Combine(TestPath, "audio_normal_neg_infinity.mkv");
    protected string TestFile_4k_h264mov => Path.Combine(TestPath, "4k_h264.mov");

    protected string TestFile_50_mbps_hd_h264 => Path.Combine(TestPath, "50-mbps-hd-h264.mkv");
    protected string TestFile_120_mbps_4k_uhd_hevc_10bit => Path.Combine(TestPath, "120-mbps-4k-uhd-hevc-10bit.mkv");

    private class TestSettings
    {
        public string TestPath { get; set; }
        public string TempPath { get; set; }
        public string FfmpegPath { get; set; }
    }
}

#endif