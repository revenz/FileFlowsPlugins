#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.Comic.Tests;


[TestClass]
public abstract class TestBase
{
    /// <summary>
    /// The test context instance
    /// </summary>
    private TestContext testContextInstance;

    /// <summary>
    /// Gets or sets the test context
    /// </summary>
    public TestContext TestContext
    {
        get => testContextInstance;
        set => testContextInstance = value;
    }

    public string TempPath = "/home/john/Comics/temp";

    [TestInitialize]
    public void TestInitialize()
    {
        if (Directory.Exists(this.TempPath) == false)
            Directory.CreateDirectory(this.TempPath);
    }
}

#endif