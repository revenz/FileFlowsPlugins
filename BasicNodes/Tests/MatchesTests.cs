#if(DEBUG)

using FileFlows.BasicNodes.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class MatchesTests : TestBase
{
    [TestMethod]    
    public void Matches_Math()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Size}", "<=100KB"),
            new("{file.Size}", ">100KB"),
            new("{file.Size}", ">10MB"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Size"] = 120_000; // 120KB

        var result = ele.Execute(args);
        Assert.AreEqual(2, result);
    }
    
    
    [TestMethod]    
    public void Matches_MathFailString()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Size}", "<=100KB"),
            new("{file.Size}", ">100KB"),
            new("{file.Size}", ">10MB"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Size"] = "Some String"; // 120KB

        var result = ele.Execute(args);
        Assert.AreEqual(4, result);
    }
    
    [TestMethod]    
    public void Matches_EqualsOne()
    {
        foreach (var test in new[]
                 {
                     ((object)1, "=1"),
                     ((object)"1", "=1"),
                     ((object)1, "1"),
                     ((object)"1", "1"),
                 })
        {
            Matches ele = new();
            ele.MatchConditions = new()
            {
                new("{myVariable}", test.Item2)
            };
            var args = new FileFlows.Plugin.NodeParameters(null, Logger,
                false, string.Empty, new LocalFileService());
            args.Variables["myVariable"] = test.Item1;

            var result = ele.Execute(args);
            Assert.AreEqual(1, result);
        }
    }

    [TestMethod]    
    public void Matches_String()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "nopetriggerthis"),
            new("{file.Name}", "DontTriggerThis"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "TriggerThis";

        var result = ele.Execute(args);
        Assert.AreEqual(3, result);
    }
    
    [TestMethod]    
    public void Matches_NoMatch()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Name}", "DontTriggerThis"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "Nothing";

        var result = ele.Execute(args);
        Assert.AreEqual(4, result);
    }
    
    [TestMethod]    
    public void Matches_Regex()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Name}", ".*batman.*"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "Superman vs Batman (2017)";

        var result = ele.Execute(args);
        Assert.AreEqual(2, result);
    }
    
    [TestMethod]    
    public void Matches_True()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "true"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = true;

        var result = ele.Execute(args);
        Assert.AreEqual(2, result);
    }
    
    
    [TestMethod]    
    public void Matches_True_1()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "true"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = 1;

        var result = ele.Execute(args);
        Assert.AreEqual(2, result);
    }
    
    [TestMethod]    
    public void Matches_False()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "false"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = false;

        var result = ele.Execute(args);
        Assert.AreEqual(2, result);
    }
    
    
    [TestMethod]    
    public void Matches_False_0()
    {
        Matches ele = new ();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "false"),
            new("{file.Name}", "TriggerThis"),
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = 0;

        var result = ele.Execute(args);
        Assert.AreEqual(2, result);
    }
}


#endif