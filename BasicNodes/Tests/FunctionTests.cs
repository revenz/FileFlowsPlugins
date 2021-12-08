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
            Args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", new TestLogger());

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


        [TestMethod]
        public void Function_UseVariables()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "miYear", 1984 }
            };
            pm.Code = @"
if(Variables['miYear'] == 1984) return 2;
return 0";
            var result = pm.Execute(args);
            Assert.AreEqual(2, result);
        }
        [TestMethod]
        public void Function_UseVariables_2()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "miYear", 1984 }
            };
            pm.Code = @"
if(Variables['miYear'] == 2000) return 2;
return 0";
            var result = pm.Execute(args);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Function_UseVariables_DotNotation()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "miYear", 1984 }
            };
            pm.Code = @"
if(Variables.miYear == 1984) return 2;
return 0";
            var result = pm.Execute(args);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void Function_VariableUpdate()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger);
            args.Variables = new Dictionary<string, object>
            {
                { "miTitle", "Ghostbusters" },
                { "miYear", 1984 }
            };
            pm.Code = @"
Variables.NewItem = 1234;
Variables.miYear = 2001;
return 0";
            var result = pm.Execute(args);
            Assert.IsTrue(args.Variables.ContainsKey("NewItem"));
            Assert.AreEqual(1234d, args.Variables["NewItem"]);
            Assert.AreEqual(2001d, args.Variables["miYear"]);
        }
    }
}


#endif