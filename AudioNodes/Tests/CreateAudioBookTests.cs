#if(DEBUG)

using System.Reflection.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileFlows.AudioNodes.AudioBooks;

namespace FileFlows.AudioNodes.Tests;


[TestClass]
public class CreateAudioBookTests
{
    
    [TestMethod]
    public void CreateAudioBookTest_01()
    {
        const string folder = "/home/john/Music/Audio Books/James Dashner (2020) Maze Runner 05.5";
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
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(folder,logger, true, string.Empty);
        args.GetToolPathActual = (string tool) => @"/usr/bin/ffmpeg";
        const string tempPath= @"/home/john/Music/test";
        args.TempPath =tempPath ;
        foreach (var file in new DirectoryInfo(tempPath).GetFiles( "*.*"))
        {
            file.Delete();
        }
        
        int output = node.Execute(args);

        var log = logger.ToString();
        Assert.AreEqual(expected, output);
        
    }
}
#endif