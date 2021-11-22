#if(DEBUG)

namespace BasicNodes.Tests
{
    using FileFlows.BasicNodes.Functions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FunctionTests
    {
        FileFlows.Plugin.NodeParameters Args;

        [TestInitialize]
        public void TestStarting()
        {
            Args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv");
            Args.Logger = new TestLogger();

        }

        [TestMethod]
        public void Function_NoCode()
        {
            Function pm = new Function();
            pm.Code = null;
            var result = pm.Execute(Args);
            Assert.AreEqual(-1, result);

            Function pm2 = new Function();
            pm2.Code = string.Empty;
            result = pm2.Execute(Args);
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void Function_BadCode()
        {
            Function pm = new Function();
            pm.Code = "let x = {";
            var result = pm.Execute(Args);
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void Function_Basic_ReturnInts()
        {
            for (int i = 0; i < 10; i++)
            {
                Function pm = new Function();
                pm.Code = "return " + i;
                var result = pm.Execute(Args);
                Assert.AreEqual(i, result);
            }
        }
    }
}

#endif