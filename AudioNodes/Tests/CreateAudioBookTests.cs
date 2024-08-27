#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.AudioNodes.Tests;


[TestClass]
public class CreateAudioBookTests : AudioTestBase
{
    /// <summary>
    /// Book Dir
    /// </summary>
    protected static readonly string BookDir = ResourcesTestFilesDir + "/Book";
    
    [TestMethod]
    public void CreateAudioBookTest()
    {
        var location = new System.IO.FileInfo(this.GetType().Assembly.Location).Directory.FullName;
        var bookDir = System.IO.Path.Combine(location, "Tests/Resources/Book");
        RunTest(bookDir);
    }
    
    private void RunTest(string folder, int expected = 1)
    {
        CreateAudioBook node = new ();
        var args = GetAudioNodeParameters(folder, isDirectory: true);

        node.PreExecute(args);
        int output = node.Execute(args);

        Assert.AreEqual(expected, output);
        
    }
}
#endif