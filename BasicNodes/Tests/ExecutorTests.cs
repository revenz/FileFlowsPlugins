#if(DEBUG)

using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using FileFlows.Plugin;
using Moq;
using PluginTestLibrary;

namespace BasicNodes.Tests;

[TestClass]
public class ExecutorTests : TestBase
{
    private Mock<IProcessHelper> mockProcessHelper = new();

    protected override void TestStarting()
    {
        mockProcessHelper.Setup(x => x.ExecuteShellCommand(It.IsAny<ExecuteArgs>())).Returns(Task.FromResult(new ProcessResult()
        {
            ExitCode = 0,
            Completed = true,
            Output = "this is the output",
            StandardOutput = "this is the standard output",
            StandardError = "this is the standard error"
        }));
    }

    [TestMethod]
    public void Executor_OutputVariable()
    {
        var args = new NodeParameters(@"c:\test\testfile.mkv", Logger, false, string.Empty, MockFileService.Object);
        args.Process = mockProcessHelper.Object;

        Executor node = new Executor();
        string file = @"D:\Videos\dummy.mkv";
        node.FileName = @"C:\utils\ffmpeg\ffmpeg.exe";
        node.Arguments = "-i \"" + file + "\"";
        node.OutputVariable = "ExecOutput";
        var result = node.Execute(args);
        Assert.IsTrue(args.Variables.ContainsKey("ExecOutput"));
        string output = args.Variables["ExecOutput"] as string;
        Assert.IsNotNull(output);
    }
}

#endif