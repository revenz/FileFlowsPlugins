#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.BasicNodes.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class VariableMatchTests
{
    FileFlows.Plugin.NodeParameters Args;

    private string TestVariable = @"Batman
/bobby/i
/Bobby Drake/
";

    [TestInitialize]
    public void TestStarting()
    {
        Args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", new TestLogger(), false, string.Empty, null);;
        Args.GetToolPathActual = (arg) => TestVariable;

    }

    [TestMethod]
    public void VariableMatch_Match()
    {
        VariableMatch vm = new VariableMatch();
        vm.Variable = new FileFlows.Plugin.ObjectReference() { Name = "test" };
        vm.Input = "bobby drake";
        vm.PreExecute(Args);
        var result = vm.Execute(Args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void VariableMatch_Match2()
    {
        VariableMatch vm = new VariableMatch();
        vm.Variable = new FileFlows.Plugin.ObjectReference() { Name = "test" };
        vm.Input = "BOBBY Two";
        vm.PreExecute(Args);
        var result = vm.Execute(Args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void VariableMatch_NoMatch()
    {
        VariableMatch vm = new VariableMatch();
        vm.Variable = new FileFlows.Plugin.ObjectReference() { Name = "test" };
        vm.Input = "Robert Drake";
        vm.PreExecute(Args);
        var result = vm.Execute(Args);
        Assert.AreEqual(2, result);
    }
    
    
    
    
    
    
    

    [TestMethod]
    public void VariableMatch_Match_New()
    {
        VariableMatch vm = new VariableMatch();
        vm.VariableName = "test";
        vm.Input = "bobby drake";
        vm.PreExecute(Args);
        var result = vm.Execute(Args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void VariableMatch_Match2_New()
    {
        VariableMatch vm = new VariableMatch();
        vm.VariableName = "test";
        vm.Input = "BOBBY Two";
        vm.PreExecute(Args);
        var result = vm.Execute(Args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void VariableMatch_NoMatch_New()
    {
        VariableMatch vm = new VariableMatch();
        vm.VariableName = "test";
        vm.Input = "Robert Drake";
        vm.PreExecute(Args);
        var result = vm.Execute(Args);
        Assert.AreEqual(2, result);
    }
}


#endif