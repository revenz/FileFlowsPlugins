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
            Args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", new TestLogger(), false, string.Empty, null);;

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

        [TestMethod]
        public void FileSize_25GB()
        {
            FileSize pm = new FileSize();
            pm.Upper = 25600;
            long fileSize = 2240000000; // 2.24GB
            var result = pm.TestSize(Args, fileSize);
            Assert.AreEqual(1, result);

            pm.Upper = 25600;
            fileSize = 224000000000; // 2.24GB
            result = pm.TestSize(Args, fileSize);
            Assert.AreEqual(2, result);
        }
    }
}

#endif