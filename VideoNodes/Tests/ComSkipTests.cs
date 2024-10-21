#if(DEBUG)

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileFlows.VideoNodes;
using FileFlows.VideoNodes.Helpers;
using Moq;

namespace VideoNodes.Tests;

/// <summary>
/// Comskip tests
/// </summary>
[TestClass]
public class ComSkipTests: VideoTestBase
{
    /// <summary>
    /// Tests a variable comskip content is used
    /// </summary>
    [TestMethod]
    public void VariableComskipVariable()
    {
        var args = GetVideoNodeParameters(VideoMkv);
        args.Variables["comskipini"] = "this\nis\na\ncomskip.ini";
        var comskipFile =ComskipHelper.GetComskipIniFile(args, VideoMkv);
        Assert.IsTrue(File.Exists(comskipFile));
        var contents = File.ReadAllText(comskipFile);
        Assert.AreEqual("this\nis\na\ncomskip.ini", contents);
    }
    
    /// <summary>
    /// Tests a variable comskip file is used
    /// </summary>
    [TestMethod]
    public void VariableComskipFile()
    {
        var args = GetVideoNodeParameters(VideoMkv);
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ini");
        File.WriteAllText(file, "this\nis\na\ncomskip.ini");
        args.Variables["comskipini"] = file;
        var comskipFile =ComskipHelper.GetComskipIniFile(args, VideoMkv);
        Assert.AreEqual(file, comskipFile);
        var contents = File.ReadAllText(comskipFile);
        Assert.AreEqual("this\nis\na\ncomskip.ini", contents);
    }
    
    
    /// <summary>
    /// Tests a default comskip ini file is used
    /// </summary>
    [TestMethod]
    public void DefaultComskipFile()
    {
        var args = GetVideoNodeParameters(VideoMkv);
        args.GetToolPathActual = (tool) => null;
        var comskipFile =ComskipHelper.GetComskipIniFile(args, VideoMkv);
        Assert.IsTrue(File.Exists(comskipFile));
        var contents = File.ReadAllText(comskipFile);
        Assert.AreEqual("detect_method=111\t\t\t;1=black frame, 2=logo, 4=scene change, 8=fuzzy logic, 16=closed captions, 32=aspect ration, 64=silence, 128=cutscenes, 255=all", contents.Split('\n')[0]);
    }
}

#endif