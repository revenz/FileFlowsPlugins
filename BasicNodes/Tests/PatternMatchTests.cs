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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", new TestLogger(), false, string.Empty, null);;
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid().ToString()}.mkv", dontDelete: true); 

            var result = pm.Execute(args);
            Assert.AreEqual(1, result);
        }

        [TestMethod]    
        public void PatternMatch_Forum()
        {
            PatternMatch pm = new PatternMatch();
            pm.Pattern = @"(.*)1080p(.*)megusta(.*)";
            var args = new FileFlows.Plugin.NodeParameters(@"SAB_TV/The.Secrets.of.Hillsong.S01E01.1080p.HEVC.x265-MeGusta/The.Secrets.of.Hillsong.S01E01.1080p.HEVC.x265-MeGusta.mkv", new TestLogger(), false, string.Empty, null);;
            
            var result = pm.Execute(args);
            Assert.AreEqual(1, result);
        }
        
        [TestMethod]
        public void PatternMatch_NotMatch()
        {
            PatternMatch pm = new PatternMatch();
            pm.Pattern = @"\.mkv$";
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.avi", new TestLogger(), false, string.Empty, null);;
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid().ToString()}.avi", dontDelete: true);

            var result = pm.Execute(args);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void PatternMatch_BadExpression()
        {
            PatternMatch pm = new PatternMatch();
            pm.Pattern = @"[-$";
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.avi", new TestLogger(), false, string.Empty, null);;
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid().ToString()}.avi", dontDelete: true);

            var result = pm.Execute(args);
            Assert.AreEqual(-1, result);
        }
        [TestMethod]
        public void PatternMatch_Trailer()
        {
            PatternMatch pm = new PatternMatch();
            pm.Pattern = @"\-trailer";
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile-TRAILER.avi", new TestLogger(), false, string.Empty, null);;
            args.SetWorkingFile($@"c:\temp\{Guid.NewGuid().ToString()}.avi", dontDelete: true);

            var result = pm.Execute(args);
            Assert.AreEqual(2, result);
        }
    }
}

#endif