#if(DEBUG)

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace FileFlows.AudioNodes.Tests;

[TestClass]
public abstract class TestBase
{
    /// <summary>
    /// The test context instance
    /// </summary>
    private TestContext testContextInstance;

    internal TestLogger Logger = new();

    /// <summary>
    /// Gets or sets the test context
    /// </summary>
    public TestContext TestContext
    {
        get => testContextInstance;
        set => testContextInstance = value;
    }

    public string TestPath { get; private set; }
    public string TempPath { get; private set; }
    public string FfmpegPath { get; private set; }
    
    public readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    
    protected readonly string ffmpeg = (OperatingSystem.IsLinux() ? "/usr/local/bin/ffmpeg" :  @"C:\utils\ffmpeg\ffmpeg.exe");
    protected readonly string ffprobe = (OperatingSystem.IsLinux() ? "/usr/local/bin/ffprobe" :  @"C:\utils\ffmpeg\ffprobe.exe");
    

    [TestInitialize]
    public void TestInitialize()
    {
        this.TestPath = this.TestPath?.EmptyAsNull() ?? (IsLinux ? "~/src/ff-files/test-files/audio" : @"d:\audio\testfiles");
        this.TempPath = this.TempPath?.EmptyAsNull() ?? (IsLinux ? "~/src/ff-files/temp" : @"d:\audio\temp");
        this.FfmpegPath = this.FfmpegPath?.EmptyAsNull() ?? (IsLinux ? "/usr/local/bin/ffmpeg" :  @"C:\utils\ffmpeg\ffmpeg.exe");
        
        this.TestPath = this.TestPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        this.TempPath = this.TempPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        this.FfmpegPath = this.FfmpegPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        
        if (Directory.Exists(this.TempPath) == false)
            Directory.CreateDirectory(this.TempPath);
    }

    [TestCleanup]
    public void CleanUp()
    {
        TestContext.WriteLine(Logger.ToString());
    }

    protected virtual void TestStarting()
    {

    }

    protected string TestFile_Mp3 => Path.Combine(TestPath, "mp3.mp3");
    protected string TestFile_Flac => Path.Combine(TestPath, "flac.flac");
    protected string TestFile_Wav => Path.Combine(TestPath, "wav.wav");
}

#endif