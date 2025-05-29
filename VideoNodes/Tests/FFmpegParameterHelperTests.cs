#if (DEBUG)

using PluginTestLibrary;
using FileFlows.VideoNodes.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VideoNodes.Tests;

[TestClass]
public class FFmpegParameterHelperTests : TestBase
{
    [TestMethod]
    public void NoReplacements_ShouldRemainUnchanged()
    {
        var args = new List<string> { "-i", "input.mp4", "-c:v", "libx264" };
        var replacements = new List<KeyValuePair<string, string>>();

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        CollectionAssert.AreEqual(args, result);
        Assert.IsFalse(Logger.Contains("Replaced"));
        Assert.IsFalse(Logger.Contains("Removed"));
    }

    [TestMethod]
    public void Replacement_WithSingleValue_ShouldReplaceCorrectly()
    {
        var args = new List<string> { "-c:v", "libx264", "-preset", "medium" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-preset medium", "-preset fast")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "-c:v", "libx264", "-preset", "fast" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Replaced FFmpeg parameter '-preset medium' with '-preset fast'");
    }

    [TestMethod]
    public void Replacement_WithMultipleValues_ShouldInsertAll()
    {
        var args = new List<string> { "-filter_complex", "oldvalue" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("oldvalue", "value1 value2 value3")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "-filter_complex", "value1", "value2", "value3" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Replaced FFmpeg parameter 'oldvalue' with 'value1 value2 value3'");
    }

    [TestMethod]
    public void Replacement_WithMultipleValues_ShouldInsertAll2()
    {
        var args = new List<string> { "-filter_complex", "oldvalue" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-filter_complex oldvalue", "-filter_complex value1 value2 value3")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "-filter_complex", "value1", "value2", "value3" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Replaced FFmpeg parameter '-filter_complex oldvalue' with '-filter_complex value1 value2 value3'");
    }

    [TestMethod]
    public void EmptyValue_ShouldBeRemoved()
    {
        var args = new List<string> { "-map", "0:a", "-c:a", "aac" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-map", "")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "0:a", "-c:a", "aac" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Removed FFmpeg parameter '-map'");
    }

    [TestMethod]
    public void EmptyValue_ShouldBeRemovedAll()
    {
        var args = new List<string> { "-map", "0:a", "-c:a", "aac", "-map", "1:a", "-map", "bob"  };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-map", "")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "0:a", "-c:a", "aac", "1:a", "bob" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Removed FFmpeg parameter '-map'");
    }
    
    [TestMethod]
    public void KeyExists_WithoutFollowingValue_ShouldStillWork()
    {
        var args = new List<string> { "-threads" }; // incomplete input
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-threads", "-threads 4")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "-threads", "4" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Replaced FFmpeg parameter '-threads' with '-threads 4'");
    }
    
    [TestMethod]
    public void Replacement_KeyWithQuotedString_ShouldReplaceCorrectly()
    {
        var args = new List<string> { "-filter_complex", "old value" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-filter_complex \"old value\"", "-filter_complex \"new value\"")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "-filter_complex", "new value" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Replaced FFmpeg parameter '-filter_complex \"old value\"' with '-filter_complex \"new value\"'");
    }

    [TestMethod]
    public void Replacement_ValueWithMultipleQuotedTokens_ShouldInsertAllAsSingleTokens()
    {
        var args = new List<string> { "-filter_complex", "oldvalue" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-filter_complex oldvalue", "-filter_complex \"value 1\" \"value 2\" \"value 3\"")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "-filter_complex", "value 1", "value 2", "value 3" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Replaced FFmpeg parameter '-filter_complex oldvalue' with '-filter_complex \"value 1\" \"value 2\" \"value 3\"'");
    }

    [TestMethod]
    public void Replacement_KeyAndValueWithMixedQuotes_ShouldReplaceCorrectly()
    {
        var args = new List<string> { "-complex_filter", "some old value" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-complex_filter \"some old value\"", "-complex_filter \"new value\" \"extra value\"")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "-complex_filter", "new value", "extra value" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Replaced FFmpeg parameter '-complex_filter \"some old value\"' with '-complex_filter \"new value\" \"extra value\"'");
    }

    [TestMethod]
    public void Replacement_KeyWithQuotesAndEmptyValue_ShouldRemoveAllTokens()
    {
        var args = new List<string> { "-filter_complex", "to remove" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-filter_complex \"to remove\"", "")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string>(); // all removed
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Removed FFmpeg parameter '-filter_complex \"to remove\"'");
    }

    [TestMethod]
    public void Replacement_ValueWithQuotesInside_ShouldKeepQuotesAsSingleToken()
    {
        var args = new List<string> { "-arg", "value" };
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("-arg value", "-arg \"quoted value with spaces\"")
        };

        var result = FFmpegParameterHelper.ApplyParameterReplacements(args, replacements, Logger);

        var expected = new List<string> { "-arg", "quoted value with spaces" };
        CollectionAssert.AreEqual(expected, result);
        StringAssert.Contains(Logger.ToString(), "Replaced FFmpeg parameter '-arg value' with '-arg \"quoted value with spaces\"'");
    }
}

#endif
