using System.Text.RegularExpressions;

namespace FileFlows.ComicNodes.Helpers;

internal class PageNameHelper
{
    internal static void FixPageNames(string directory)
    {

        var files = new DirectoryInfo(directory).GetFiles();
        foreach (var file in files)
        {
            var numMatch = Regex.Match(file.Name, @"[\d]+");
            if (numMatch.Success == false)
                continue;
            // ensure any file that is stupidly name eg page1.jpg, page2.jpg, page10.jpg, page11.jpg etc is ordered correctly
            file.MoveTo(Path.Combine(directory, file.Name.Replace(numMatch.Value, int.Parse(numMatch.Value).ToString(new string('0', files.Length.ToString().Length)))));
        }
    }
}
