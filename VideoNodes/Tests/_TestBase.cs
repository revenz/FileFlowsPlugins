#if(DEBUG)

using System.Runtime.InteropServices;
using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.IO;

namespace VideoNodes.Tests;

[TestClass]
public abstract class TestBase
{
    private TestContext testContextInstance;

    public TestContext TestContext
    {
        get { return testContextInstance; }
        set { testContextInstance = value; }
    }

    public string TestPath { get; private set; }
    public string TempPath { get; private set; }
    public string FfmpegPath { get; private set; }
    
    public readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    [TestInitialize]
    public void TestInitialize()
    {
        if (System.IO.File.Exists("../../../test.settings.dev.json"))
        {
            LoadSettings("../../../test.settings.dev.json");
        }
        else if (System.IO.File.Exists("../../../test.settings.json"))
        {
            LoadSettings("../../../test.settings.json");
        }
        this.TestPath = this.TestPath?.EmptyAsNull() ?? (IsLinux ? "~/src/ff-files/test-files" : @"d:\videos\testfiles");
        this.TempPath = this.TempPath?.EmptyAsNull() ?? (IsLinux ? "~/src/ff-files/temp" : @"d:\videos\temp");
        this.FfmpegPath = this.FfmpegPath?.EmptyAsNull() ?? (IsLinux ? "/usr/local/bin/ffmpeg" :  @"C:\utils\ffmpeg\ffmpeg.exe");
        
        
        this.TestPath = this.TestPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        this.TempPath = this.TempPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        this.FfmpegPath = this.FfmpegPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        
        if (Directory.Exists(this.TempPath) == false)
            Directory.CreateDirectory(this.TempPath);
    }

    private void LoadSettings(string filename)
    {
        try
        {
            if (File.Exists(filename) == false)
                return;
            
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

    protected string TestFile_MovText_Mp4 => Path.Combine(TestPath, "movtext.mp4");
    protected string TestFile_BasicMkv => Path.Combine(TestPath, "basic.mkv");
    protected string TestFile_Tag => Path.Combine(TestPath, "tag.mp4");
    protected string TestFile_Sitcom => Path.Combine(TestPath, "sitcom.mkv");
    protected string TestFile_Pgs => Path.Combine(TestPath, "pgs.mkv");
    protected string TestFile_Font => Path.Combine(TestPath, "font.mkv");
    protected string TestFile_DefaultSub => Path.Combine(TestPath, "default-sub.mkv");
    protected string TestFile_ForcedDefaultSub => Path.Combine(TestPath, "sub-forced-default.mkv");
    protected string TestFile_DefaultIsForcedSub => Path.Combine(TestPath, "sub-default-is-forced.mkv");
    protected string TestFile_TwoPassNegInifinity => Path.Combine(TestPath, "audio_normal_neg_infinity.mkv");
    protected string TestFile_4k_h264mov => Path.Combine(TestPath, "4k_h264.mov");
    protected string TestFile_4k_h264mkv => Path.Combine(TestPath, "4k_h264.mkv");

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