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
            Args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", new TestLogger(), false, string.Empty);

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
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "movie.Title", "Ghostbusters" },
                { "movie.Year", 1984 }
            };
            pm.Code = @"
if(Variables['movie.Year'] == 1984) return 2;
return 0";
            var result = pm.Execute(args);
            Assert.AreEqual(2, result);
        }
        [TestMethod]
        public void Function_UseVariables_2()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "movie.Title", "Ghostbusters" },
                { "movie.Year", 1984 }
            };
            pm.Code = @"
if(Variables['movie.Year'] == 2000) return 2;
return 0";
            var result = pm.Execute(args);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Function_UseVariables_DotNotation()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "movie.Title", "Ghostbusters" },
                { "movie.Year", 1984 }
            };
            pm.Code = @"
if(Variables.movie.Year == 1984) return 2;
return 0";
            var result = pm.Execute(args);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void Function_VariableUpdate()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "movie.Title", "Ghostbusters" },
                { "movie.Year", 1984 }
            };
            pm.Code = @"
Variables.NewItem = 1234;
Variables.movie.Year = 2001;
return 0";
            var result = pm.Execute(args);
            Assert.IsTrue(args.Variables.ContainsKey("NewItem"));
            Assert.AreEqual(1234d, args.Variables["NewItem"]);
            Assert.AreEqual(2001d, args.Variables["movie.Year"]);
        }


        [TestMethod]
        public void Function_UseVariables_Date()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "folder.Date", new DateTime(2020, 03, 01) }
            };
            pm.Code = @"
if(Variables.folder.Date.getFullYear() === 2020) return 1;
return 2";
            var result = pm.Execute(args);
            Assert.AreEqual(1, result);
        }
        [TestMethod]
        public void Function_UseVariables_MultipelDot()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger, false, string.Empty);
            args.Variables = new Dictionary<string, object>
            {
                { "folder.Date.Year", 2020 }
            };
            pm.Code = @"
if(Variables.folder.Date.Year === 2020) return 1;
return 2";
            var result = pm.Execute(args);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Function_Flow_SetParameter()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger, false, string.Empty);
            Assert.IsFalse(args.Parameters.ContainsKey("batman"));
            pm.Code = @"
Flow.SetParameter('batman', 1989);
return 1";
            var result = pm.Execute(args);
            Assert.AreEqual(1, result);
            Assert.IsTrue(args.Parameters.ContainsKey("batman"));
            Assert.AreEqual(args.Parameters["batman"].ToString(), "1989");
        }
        [TestMethod]
        public void Function_Flow_GetDirectorySize()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\sdfsdfdsvfdcxdsf.mkv", logger, false, string.Empty);
            pm.Code = @"return Flow.GetDirectorySize('C:\\temp');";
            var result = pm.Execute(args);
            Assert.IsTrue(result > 0);
        }


        [TestMethod]
        public void Function_Flow_Execute()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"D:\videos\unprocessed\The IT Crowd - 2x04 - The Dinner Party - No English.mkv", logger, false, string.Empty);
            pm.Code = @"
let result = Flow.Execute({command:'c:\\utils\\ffmpeg\\ffmpeg.exe', argumentList: ['-i', Variables.file.FullName]});
Logger.ILog('ExitCode: ' + result.exitCode);
Logger.ILog('completed: ' + result.completed);
Logger.ILog('standardOutput: ' + result.standardOutput);
if(!result.standardOutput || result.standardOutput.length < 1)
    return 3;
if(result.exitCode === 1)
	return 2;
return 0;
;";
            var result = pm.Execute(args);
            Assert.AreEqual(2, result);
        }
        [TestMethod]
        public void Function_Flow_ExecuteFfmpeg()
        {
            Function pm = new Function();
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"D:\videos\unprocessed\The IT Crowd - 2x04 - The Dinner Party - No English.mkv", logger, false, string.Empty);
            args.GetToolPathActual = (string name) => @"C:\utils\ffmpeg\ffmpeg.exe";
            args.TempPath = @"D:\videos\temp";
            pm.Code = @"
let output = Flow.TempPath + '/' + Flow.NewGuid() + '.mkv';
let ffmpeg = Flow.GetToolPath('ffmpeg');
let process = Flow.Execute({
	command: ffmpeg,
	argumentList: [
		'-i',
		Variables.file.FullName,
		'-c:v',
		'libx265',
		'-c:a',
		'copy',
		output
	]
});

if(process.standardOutput)
	Logger.ILog('Standard output: ' + process.standardOutput);
if(process.starndardError)
	Logger.ILog('Standard error: ' + process.starndardError);

if(process.exitCode !== 0){
	Logger.ELog('Failed processing ffmpeg: ' + process.exitCode);
	return -1;
}

Flow.SetWorkingFile(output);
return 1;
;";
            var result = pm.Execute(args);
            Assert.AreEqual(1, result);
        }
    }
}


#endif