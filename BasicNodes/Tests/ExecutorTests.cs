#if(DEBUG)

namespace BasicNodes.Tests
{
    using FileFlows.BasicNodes.File;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExecutorTests
    {
        [TestMethod]
        public void Executor_OutputVariable()
        {
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);

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
}

#endif