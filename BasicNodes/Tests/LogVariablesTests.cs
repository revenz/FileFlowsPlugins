#if(DEBUG)

using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileFlows.BasicNodes.Logging;

namespace BasicNodes.Tests;

/// <summary>
/// Tests the LogVariables flow element
/// </summary>
[TestClass]
public class LogVariablesTests: TestBase
{
    [TestMethod]
    public void Test_NonRecursive_VariablesLogging()
    {
        // Arrange
        var variables = new Dictionary<string, object>
        {
            { "Name", "John" },
            { "Age", 30 },
            { "Hobbies", new List<string> { "Reading", "Traveling" } },
            { "NullValue", null }
        };

        // Act
        string result = LogVariables.GetVariablesString(variables, recursive: false);
        Logger.Raw(result);

        // Assert
        string expected = "Name: John\nAge: 30\nHobbies: [Reading, Traveling]\nNullValue: null\n";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Test_Recursive_VariablesLogging()
    {
        // Arrange
        var variables = new Dictionary<string, object>
        {
            { "User", new {
                    Name = "John",
                    Details = new { Age = 30, Address = "123 Street" }
                }
            },
            { "Hobbies", new List<string> { "Reading", "Traveling" } }
        };

        // Act
        string result = LogVariables.GetVariablesString(variables, recursive: true);
        Logger.Raw(result);

        // Assert
        string expected = "User.Name: John\nUser.Details.Age: 30\nUser.Details.Address: 123 Street\nHobbies: [Reading, Traveling]\n";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Test_Recursive_WithDictionary()
    {
        // Arrange
        var variables = new Dictionary<string, object>
        {
            { "Config", new Dictionary<string, object>
                {
                    { "MaxRetries", 3 },
                    { "Timeout", "30s" }
                }
            }
        };

        // Act
        string result = LogVariables.GetVariablesString(variables, recursive: true);
        Logger.Raw(result);

        // Assert
        string expected = "Config.MaxRetries: 3\nConfig.Timeout: 30s\n";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Test_NullValueHandling()
    {
        // Arrange
        var variables = new Dictionary<string, object>
        {
            { "NullEntry", null }
        };

        // Act
        string result = LogVariables.GetVariablesString(variables, recursive: false);
        Logger.Raw(result);

        // Assert
        string expected = "NullEntry: null\n";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Test_EnumerableHandling()
    {
        // Arrange
        var variables = new Dictionary<string, object>
        {
            { "Numbers", new[] { 1, 2, 3 } },
            { "EmptyList", new List<string>() }
        };

        // Act
        string result = LogVariables.GetVariablesString(variables, recursive: false);
        Logger.Raw(result);

        // Assert
        string expected = "Numbers: [1, 2, 3]\nEmptyList: []\n";
        Assert.AreEqual(expected, result);
    }
}

#endif
