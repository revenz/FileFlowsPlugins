#if(DEBUG)

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FileFlows.BasicNodes;
using FileFlows.Plugin.Helpers;
using FileFlows.Plugin.Services;
using Moq;

namespace BasicNodes.Tests;

/// <summary>
/// Base class for the tests
/// </summary>
[TestClass]
public abstract class TestBase
{
    /// <summary>
    /// The test context instance
    /// </summary>
    private TestContext testContextInstance;

    internal TestLogger Logger = new();

    protected Mock<IFileService> MockFileService = new();

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
    
    public readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    [TestInitialize]
    public void TestInitialize()
    {
        FileHelper.DontChangeOwner = true;
        FileHelper.DontSetPermissions = true;
        Logger.Writer = (msg) => TestContext.WriteLine(msg);
        
        this.TestPath = this.TestPath?.EmptyAsNull() ?? (IsLinux ? "~/src/ff-files/test-files/videos" : @"d:\videos\testfiles");
        this.TempPath = this.TempPath?.EmptyAsNull() ?? (IsLinux ? "~/src/ff-files/temp" : @"d:\videos\temp");
        
        this.TestPath = this.TestPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        this.TempPath = this.TempPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        
        if (Directory.Exists(this.TempPath) == false)
            Directory.CreateDirectory(this.TempPath);

        TestStarting();
    }

    [TestCleanup]
    public void CleanUp()
    {
        TestCleanUp();
        TestContext.WriteLine(Logger.ToString());
    }

    protected virtual void TestStarting()
    {

    }
    protected virtual void TestCleanUp()
    {

    }

}

#endif