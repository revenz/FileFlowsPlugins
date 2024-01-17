#if(DEBUG)

using System.IO;

namespace BasicNodes.Tests
{
    using FileFlows.BasicNodes.File;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileSizeCompareTests
    {
        private string CreateFile(int size)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());  
            File.WriteAllText(tempFile, new string('a', size));
            return tempFile;

        }
        [TestMethod]
        public void FileSize_Shrunk()
        {
            string tempFile = CreateFile(2);
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(tempFile, logger, false, string.Empty, null);

            string wfFile = CreateFile(1);
            args.SetWorkingFile(wfFile);

            FileSizeCompare node = new();
            int result = node.Execute(args);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void FileSize_Grew()
        {
            string tempFile = CreateFile(2);
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(tempFile, logger, false, string.Empty, null);

            string wfFile = CreateFile(20);
            args.SetWorkingFile(wfFile);

            FileSizeCompare node = new ();
            int result = node.Execute(args);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void FileSize_SameSize()
        {
            string tempFile = CreateFile(2);
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(tempFile, logger, false, string.Empty, null);

            string wfFile = CreateFile(2);
            args.SetWorkingFile(wfFile);

            FileSizeCompare node = new();
            int result = node.Execute(args);
            Assert.AreEqual(2, result);
        }


        [TestMethod]
        public void FileSize_Shrunk_OriginalDeleted()
        {
            string tempFile = CreateFile(2);
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(tempFile, logger, false, string.Empty, null);
            Assert.IsTrue(args.WorkingFileSize > 0);
            File.Delete(tempFile);

            string wfFile = CreateFile(1);
            args.SetWorkingFile(wfFile);

            FileSizeCompare node = new();
            int result = node.Execute(args);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void FileSize_Grew_OriginalDeleted()
        {
            string tempFile = CreateFile(2);
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(tempFile, logger, false, string.Empty, null);
            File.Delete(tempFile);

            string wfFile = CreateFile(20);
            args.SetWorkingFile(wfFile);

            FileSizeCompare node = new();
            int result = node.Execute(args);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void FileSize_SameSize_OriginalDeleted()
        {
            string tempFile = CreateFile(2);
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(tempFile, logger, false, string.Empty, null);
            File.Delete(tempFile);

            string wfFile = CreateFile(2);
            args.SetWorkingFile(wfFile);

            FileSizeCompare node = new();
            int result = node.Execute(args);
            Assert.AreEqual(2, result);
        }
    }
}

#endif