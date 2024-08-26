
#if(DEBUG)

namespace ChecksumNodes.Tests;

using PluginTestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ChecksumTests : TestBase
{
    [TestMethod]
    public void Checksum_MD5_Large()
    {
        var args = new NodeParameters(TempFile, Logger, false, "", new LocalFileService());
        var output = new MD5().Execute(args);
        Assert.IsTrue(args.Variables.ContainsKey("MD5"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["MD5"] as string));
    }

    [TestMethod]
    public void Checksum_SHA1_Large()
    {
        var args = new NodeParameters(TempFile, Logger, false, "", new LocalFileService());
        var output = new SHA1().Execute(args);
        Assert.IsTrue(args.Variables.ContainsKey("SHA1"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["SHA1"] as string));
    }

    [TestMethod]
    public void Checksum_SHA256_Large()
    {
        var args = new NodeParameters(TempFile, Logger, false, "", new LocalFileService());
        var output = new SHA256().Execute(args);
        Assert.IsTrue(args.Variables.ContainsKey("SHA256"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["SHA256"] as string));
    }

    [TestMethod]
    public void Checksum_SHA512_Large()
    {
        var args = new NodeParameters(TempFile, Logger, false, "", new LocalFileService());
        var output = new SHA512().Execute(args);
        Assert.IsTrue(args.Variables.ContainsKey("SHA512"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(args.Variables["SHA512"] as string));
    }
}

#endif