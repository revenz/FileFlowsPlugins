#if(DEBUG)


using System.Diagnostics;
using System.IO;
using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasicNodes.Tests;

[TestClass]
public class HasHardLinksTest : TestBase
{
    private string? testFile;


    protected override void TestCleanUp()
    {
        // Clean up the original file and any hard links created
        if (string.IsNullOrEmpty(testFile) == false && System.IO.File.Exists(testFile))
        {
            try
            {
                // Delete the original file
                System.IO.File.Delete(testFile);

                // Delete the hard links
                for (int i = 1; ; i++)
                {
                    var linkName = $"{testFile}_link{i}";
                    if (System.IO.File.Exists(linkName))
                        System.IO.File.Delete(linkName);
                    else
                        break; // Stop if the link doesn't exist
                }
            }
            catch (Exception ex)
            {
                // Log or handle the cleanup exception if necessary
                Logger.ILog($"Failed to clean up files: {ex.Message}");
            }
        }
    }
    public static void CreateHardLinkFile(string filename, int hardlinks = 1)
    {
        // Ensure the file exists
        System.IO.File.WriteAllText(filename, "Test content");

        // Create the specified number of hard links
        for (int i = 1; i <= hardlinks; i++)
        {
            string linkName = $"{filename}_link{i}";

            // Use the `ln` command to create a hard link
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ln",
                    Arguments = $"{filename} {linkName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Failed to create hard link: {error}");
            }
        }
    }
    
    [TestMethod]
    public void HasHardLink()
    {
        testFile = Path.Combine(TempFile, Guid.NewGuid().ToString());
        CreateHardLinkFile(testFile, 2);
        
        var args = new FileFlows.Plugin.NodeParameters(testFile, Logger, false, string.Empty, MockFileService.Object);
        
        HasHardLinks element = new ();
        
        var result = element.Execute(args);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void NoHardLinks()
    {
        testFile = Path.Combine(TempFile, Guid.NewGuid().ToString());
        var args = new FileFlows.Plugin.NodeParameters(testFile, Logger, false, string.Empty, MockFileService.Object);
        
        HasHardLinks element = new ();
        
        var result = element.Execute(args);
        Assert.AreEqual(2, result);
    }
}

#endif