#if(DEBUG)

using System.Runtime.InteropServices;
using FileFlows.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileFlows.Plugin.Helpers;
using FileFlows.Plugin.Services;
using Moq;

namespace PluginTestLibrary;

/// <summary>
/// Base class for the tests
/// </summary>
[TestClass]
public abstract class TestBase
{
    /// <summary>
    /// The test context instance
    /// </summary>
    private TestContext testContextInstance = null!;

    protected TestLogger Logger = new();
    protected Mock<IFileService> MockFileService = new();

    /// <summary>
    /// Gets or sets the test context
    /// </summary>
    public TestContext TestContext
    {
        get => testContextInstance;
        set => testContextInstance = value;
    }

    /// <summary>
    /// A File created for the test
    /// </summary>
    public string TempFile { get; private set; } = null!;

    /// <summary>
    /// A path in the temp directory created for the test
    /// </summary>
    public string TempPath { get; private set; } = null!;

    public readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    [TestInitialize]
    public void TestInitialize()
    {
        FileHelper.DontChangeOwner = true;
        FileHelper.DontSetPermissions = true;
        Logger.Writer = (msg) => TestContext.WriteLine(msg);


        TempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        System.IO.Directory.CreateDirectory(TempPath);
        TempFile = System.IO.Path.Combine(TempPath, Guid.NewGuid() + ".txt");
        System.IO.File.WriteAllText(TempFile, Guid.NewGuid().ToString());

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

    public string GetPluginSettingsJson(string plugin)
    {
        if (Environment.GetEnvironmentVariable("Docker") == "1")
            return File.ReadAllText("/plugin-settings/" + plugin + ".json");

        return File.ReadAllText("../../../settings.json");
    }

    /// <summary>
    /// Gets the node parameters for a file
    /// </summary>
    /// <param name="filename">the name of the file</param>
    /// <param name="isDirectory">if the file is a directory</param>
    /// <returns>the node parameters</returns>
    protected NodeParameters GetNodeParameters(string filename, bool isDirectory =false)
    {
        string tempPath = TempPath;
        string libPath = Path.Combine(tempPath, "media");
        if (Directory.Exists(libPath) == false)
            Directory.CreateDirectory(libPath);
        var args = new NodeParameters(filename, Logger, isDirectory, libPath, new LocalFileService())
        {
            LibraryFileName = filename,
            TempPath = tempPath
        };
        return args;
    }

}

#endif