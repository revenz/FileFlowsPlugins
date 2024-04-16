#if(DEBUG)

using FileFlows.BasicNodes.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class MatchesTests
{
    [TestMethod]    
    public void Matches_Math()
    {
        var logger = new TestLogger();
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Size}", "<=100KB"),
            new("{file.Size}", ">100KB"),
            new("{file.Size}", ">10MB"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Size"] = 120_000; // 120KB

        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(2, result);
    }

    [TestMethod]    
    public void Matches_String()
    {
        var logger = new TestLogger();
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Name}", "DontTriggerThis"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "TriggerThis";

        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(3, result);
    }
    [TestMethod]    
    public void Matches_NoMatch()
    {
        var logger = new TestLogger();
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Name}", "DontTriggerThis"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "Nothing";

        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(4, result);
    }
    
    [TestMethod]    
    public void Matches_Regex()
    {
        var logger = new TestLogger();
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Name}", ".*batman.*"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "Superman vs Batman (2017)";

        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(2, result);
    }
    
    [TestMethod]    
    public void Matches_True()
    {
        var logger = new TestLogger();
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "true"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = true;

        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(2, result);
    }
    
    
    [TestMethod]    
    public void Matches_True_1()
    {
        var logger = new TestLogger();
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "true"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = 1;

        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(2, result);
    }
    
    [TestMethod]    
    public void Matches_False()
    {
        var logger = new TestLogger();
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "false"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = false;

        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(2, result);
    }
    
    
    [TestMethod]    
    public void Matches_False_0()
    {
        var logger = new TestLogger();
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "false"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = 0;

        var result = ele.Execute(args);
        var log = logger.ToString();
        Assert.AreEqual(2, result);
    }
}


#endif