#if(DEBUG)

namespace BasicNodes.Tests
{
    using FileFlows.BasicNodes.File;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileSizeTests
    {
        FileFlows.Plugin.NodeParameters Args;

        [TestInitialize]
        public void TestStarting()
        {
            Args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            Args.Logger = new TestLogger();

        }

        [TestMethod]
        public void FileSize_LessThanLower()
        {
            FileSize pm = new FileSize();
            pm.Lower = 5;
            var result = pm.TestSize(Args, (5 * 1024 * 1024) - 1);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void FileSize_EqualToLower()
        {
            FileSize pm = new FileSize();
            pm.Lower = 5;
            var result = pm.TestSize(Args, 5 * 1024 * 1024);
            Assert.AreEqual(1, result);
        }
        [TestMethod]
        public void FileSize_GreaterThanUpper()
        {
            FileSize pm = new FileSize();
            pm.Upper = 5;
            var result = pm.TestSize(Args, (5 * 1024 * 1024) + 1);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void FileSize_EqualToUpper()
        {
            FileSize pm = new FileSize();
            pm.Upper = 5;
            var result = pm.TestSize(Args, 5 * 1024 * 1024);
            Assert.AreEqual(1, result);
        }
    }
}

#endif