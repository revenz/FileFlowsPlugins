#if(DEBUG)

using AudioNodes.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.AudioNodes.Tests;


[TestClass]
public class CreateAudioBookTests : AudioTestBase
{
    [TestMethod]
    public void CreateAudioBookTest_01()
    {
        const string folder = "/home/john/Music/unprocessed/Aqua/Aquarius (2000)";
        RunTest(folder);
    }
    
    [TestMethod]
    public void CreateAudioBookTest_02()
    {
        const string folder = @"/home/john/Music/Audio Books/Charlie and the Great Glass Elevator";
        RunTest(folder);
    }
    
    [TestMethod]
    public void CreateAudioBookTest_03()
    {
        const string folder = @"/home/john/Music/Audio Books/Scott Westerfeld - Afterworlds";
        RunTest(folder);
    }
    
    [TestMethod]
    public void CreateAudioBookTest_04()
    {
        const string folder = @"/home/john/Music/Audio Books/Small Town-Lawrence Block";
        RunTest(folder);
    }

    [TestMethod]
    public void CreateAudioBookTest_05()
    {
        const string folder = @"/home/john/Music/Audio Books/Shatter City";
        RunTest(folder, 2);
    }
    
    [TestMethod]
    public void CreateAudioBookTest_06()
    {
        const string folder = @"/home/john/Music/Audio Books/Among the Betrayed - Margaret Peterson Haddix (M4B)";
        RunTest(folder, 2);
    }
    
    private void RunTest(string folder, int expected = 1)
    {
        CreateAudioBook node = new ();
        var args = GetNodeParameters(folder, isDirectory: true);
        foreach (var file in new System.IO.DirectoryInfo(args.TempPath).GetFiles( "*.*"))
        {
            file.Delete();
        }
        
        int output = node.Execute(args);

        var log = logger.ToString();
        TestContext.WriteLine(log);
        Assert.AreEqual(expected, output);
        
    }
}
#endif