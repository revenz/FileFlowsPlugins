using FileFlows.Plugin;

#if(DEBUG)

namespace BasicNodes.Tests;

using FileFlows.BasicNodes.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

[TestClass]
public class CopyTests
{
    List<KeyValuePair<string, string>> Mappings = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("/usr/local/bin/ffmpeg", @"C:\Users\username\AppData\Roaming\FileFlows\Tools\ffmpeg.exe"),
        new KeyValuePair<string, string>("/mnt/tempNAS/media/dvd/sorted", @"\\192.168.1.22\Media\dvd\sorted"),
        new KeyValuePair<string, string>("/mnt/tempNAS/media/dvd/output", @"\\192.168.1.22\Media\dvd\output"),
    };
    char DirectorySeperatorChar = System.IO.Path.DirectorySeparatorChar;

    [TestMethod]
    public void CopyTests_Dir_Mapping()
    {            
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
        args.PathMapper = s => Map(s);
        
        CopyFile node = new ();
        node.CopyFolder = true;
        node.DestinationPath = "/mnt/tempNAS/media/dvd/output";
        node.DestinationFile = "{file.Orig.FileName}p.DVD.x264.slow.CRF16{ext}";
        var result = node.Execute(args);
        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void CopyTests_Dir_DateReplacements()
    {
        var logger = new TestLogger();
        var args = new FileFlows.Plugin.NodeParameters(@"D:\videos\testfiles\bigbuckbunny_480p_30s.mp4", logger, false, string.Empty);
        args.PathMapper = s => Map(s);

        CopyFile node = new();
        node.CopyFolder = true;
        node.DestinationPath = @"D:\videos\converted";
        node.DestinationFile = "{file.Orig.FileName}-{file.Create.Month:00}-{file.Create.Year}{ext}";
        var result = node.Execute(args);
        Assert.AreEqual(1, result);
    }

    string Map(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        if (Mappings != null && Mappings.Count > 0)
        {
            // convert all \ to / for now
            path = path.Replace("\\", "/");
            foreach (var mapping in Mappings)
            {
                if (string.IsNullOrEmpty(mapping.Value) || string.IsNullOrEmpty(mapping.Key))
                    continue;
                string pattern = Regex.Escape(mapping.Key.Replace("\\", "/"));
                string replacement = mapping.Value.Replace("\\", "/");
                path = Regex.Replace(path, "^" + pattern, replacement, RegexOptions.IgnoreCase);
            }
            // now convert / to path charcter
            if (DirectorySeperatorChar != '/')
                path = path.Replace('/', DirectorySeperatorChar);
            if (path.StartsWith("//")) // special case for SMB paths
                path = path.Replace('/', '\\');
        }
        return path;
    }
}

#endif