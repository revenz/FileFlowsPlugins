using System.Diagnostics;

namespace FileFlows.ComicNodes.Helpers;

internal class UnrarCommandLine
{
    /// <summary>
    /// Uncompresses a folder
    /// </summary>
    /// <param name="args">the node paratemers</param>
    /// <param name="workingFile">the file to extract</param>
    /// <param name="destinationPath">the location to extract to</param>
    /// <param name="halfProgress">if the NodeParameter.PartPercentageUpdate should end at 50%</param>
    internal static void Extract(NodeParameters args, string workingFile, string destinationPath, bool halfProgress = true)
    {
        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 0);

        var process = new Process();
        process.StartInfo.FileName = "unrar";
        process.StartInfo.ArgumentList.Add("x");
        process.StartInfo.ArgumentList.Add("-o+");
        process.StartInfo.ArgumentList.Add(workingFile);
        process.StartInfo.ArgumentList.Add(destinationPath);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardError.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        args.Logger?.ILog("Unrar output:\n" + output);
        if (string.IsNullOrWhiteSpace(error) == false)
            args.Logger?.ELog("Unrar error:\n" + error);

        if (process.ExitCode != 0)
            throw new Exception(error?.EmptyAsNull() ?? "Failed to extract rar file");

        PageNameHelper.FixPageNames(destinationPath);

        if (args?.PartPercentageUpdate != null)
            args?.PartPercentageUpdate(halfProgress ? 50 : 100);
    }

    internal static int GetImageCount(string workingFile)
    {
        var rgxImages = new Regex(@"\.(jpeg|jpg|jpe|png|bmp|tiff|webp|gif)$", RegexOptions.IgnoreCase);

        var process = new Process();
        process.StartInfo.FileName = "unrar";
        process.StartInfo.ArgumentList.Add("list");
        process.StartInfo.ArgumentList.Add(workingFile);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardError.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception(error?.EmptyAsNull() ?? "Failed to open rar file");

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        return lines.Where(x => rgxImages.IsMatch(x.Trim())).Count();
    }
}
