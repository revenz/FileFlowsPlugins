#if(DEBUG)

namespace BasicNodes.Tests
{
    using FileFlows.BasicNodes.Functions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PatternMatchTests
    {
        [TestMethod]    
        public void PatternMatch_Extension()
        {
            PatternMatch pm = new PatternMatch();
            pm.Pattern = @"\.mkv$";
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid().ToString()}.mkv", dontDelete: true); 

            var result = pm.Execute(args);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void PatternMatch_NotMatch()
        {
            PatternMatch pm = new PatternMatch();
            pm.Pattern = @"\.mkv$";
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.avi");
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid().ToString()}.avi", dontDelete: true);

            var result = pm.Execute(args);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void PatternMatch_BadExpression()
        {
            PatternMatch pm = new PatternMatch();
            pm.Pattern = @"[-$";
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.avi");
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid().ToString()}.avi", dontDelete: true);

            var result = pm.Execute(args);
            Assert.AreEqual(-1, result);
        }
    }
}

#endif