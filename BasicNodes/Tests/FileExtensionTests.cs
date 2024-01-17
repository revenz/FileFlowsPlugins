#if(DEBUG)

namespace BasicNodes.Tests
{
    using FileFlows.BasicNodes.File;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileExtensionTests
    {
        FileFlows.Plugin.NodeParameters Args;

        [TestInitialize]
        public void TestStarting()
        {
            Args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", new TestLogger(), false, string.Empty, null);;

        }

        [TestMethod]
        public void FileExtension_Matches_Single()
        {
            FileExtension pm = new FileExtension();
            pm.Extensions = new string[] { "mkv" };
            var result = pm.Execute(Args);
            Assert.AreEqual(1, result);
        }
        [TestMethod]
        public void FileExtension_Matches_Mutlitple()
        {
            FileExtension pm = new FileExtension();
            pm.Extensions = new string[] { "avi", "divx", "mkv", "mpg" };
            var result = pm.Execute(Args);
            Assert.AreEqual(1, result);
        }
        [TestMethod]
        public void FileExtension_Matches_Mutlitple_Period()
        {
            FileExtension pm = new FileExtension();
            pm.Extensions = new string[] { ".avi", ".divx", ".mkv", ".mpg" };
            var result = pm.Execute(Args);
            Assert.AreEqual(1, result);
        }
        [TestMethod]
        public void FileExtension_Matches_Mutlitple_UpperPeriod()
        {
            FileExtension pm = new FileExtension();
            pm.Extensions = new string[] { ".AVI", ".DIVX", ".MKV", ".MPG" };
            var result = pm.Execute(Args);
            Assert.AreEqual(1, result);
        }
        [TestMethod]
        public void FileExtension_NoMatches_Mutlitple()
        {
            FileExtension pm = new FileExtension();
            pm.Extensions = new string[] { "avi", "divx", "mpg" };
            var result = pm.Execute(Args);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void FileExtension_NoExtensions()
        {
            FileExtension pm = new FileExtension();
            pm.Extensions = new string[] {  };
            var result = pm.Execute(Args);
            Assert.AreEqual(-1, result);

            FileExtension pm2 = new FileExtension();
            pm.Extensions = null;
            result = pm.Execute(Args);
            Assert.AreEqual(-1, result);
        }
    }
}

#endif