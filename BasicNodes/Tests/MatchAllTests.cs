#if(DEBUG)

using FileFlows.BasicNodes.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class MatchAllTests: TestBase
{
    [TestMethod]
    public void MatchesAll_AllMatch()
    {
        MatchesAll ele = new();
        ele.MatchConditions = new()
        {
            new("{file.Size}", "<=100KB"),
            new("{file.Size}", ">50KB")
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Size"] = 80_000; // 80KB

        var result = ele.Execute(args);
        Assert.AreEqual(1, result); // All conditions match
    }

    [TestMethod]
    public void MatchesAll_NotAllMatch()
    {
        MatchesAll ele = new();
        ele.MatchConditions = new()
        {
            new("{file.Size}", "<=100KB"),
            new("{file.Size}", ">100KB")
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Size"] = 80_000; // 80KB

        var result = ele.Execute(args);
        Assert.AreEqual(2, result); // Not all conditions match
    }

    [TestMethod]
    public void MatchesAll_MatchFailString()
    {
        MatchesAll ele = new();
        ele.MatchConditions = new()
        {
            new("{file.Size}", "<=100KB"),
            new("{file.Size}", ">50KB")
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Size"] = "Invalid String";

        var result = ele.Execute(args);
        Assert.AreEqual(2, result); // Should fail because the value is not numeric
    }

    [TestMethod]
    public void MatchesAll_True()
    {
        MatchesAll ele = new();
        ele.MatchConditions = new()
        {
            new("{file.Deleted}", "true"),
            new("{file.Size}", ">0")
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = true;
        args.Variables["file.Size"] = 50_000;

        var result = ele.Execute(args);
        Assert.AreEqual(1, result); // Both conditions pass
    }

    [TestMethod]
    public void MatchesAll_False()
    {
        MatchesAll ele = new();
        ele.MatchConditions = new()
        {
            new("{file.Deleted}", "true"),
            new("{file.Size}", "<0")
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Deleted"] = true;
        args.Variables["file.Size"] = 50_000;

        var result = ele.Execute(args);
        Assert.AreEqual(2, result); // Second condition fails
    }

    [TestMethod]
    public void MatchesAll_Regex()
    {
        MatchesAll ele = new();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "/.*batman.*/"),
            new("{file.Name}", "/.*superman.*/")
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "Superman vs Batman (2017)";

        var result = ele.Execute(args);
        Assert.AreEqual(1, result); // Both regex conditions pass
    }

    [TestMethod]
    public void MatchesAll_NoMatch()
    {
        MatchesAll ele = new();
        ele.MatchConditions = new()
        {
            new("{file.Name}", "triggerthis"),
            new("{file.Deleted}", "true")
        };
        var args = new FileFlows.Plugin.NodeParameters(null, Logger,
            false, string.Empty, new LocalFileService());
        args.Variables["file.Name"] = "Nothing";
        args.Variables["file.Deleted"] = false;

        var result = ele.Execute(args);
        Assert.AreEqual(2, result); // Both conditions fail
    }
    
}

#endif