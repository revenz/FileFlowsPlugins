using System.ComponentModel;
using System.Xml;
using FileFlows.ComicNodes.Helpers;
using FileHelper = FileFlows.Plugin.Helpers.FileHelper;

namespace FileFlows.ComicNodes.Comics;

/// <summary>
/// Creates comic info xml for a file
/// </summary>
public class CreateComicInfo : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;

    /// <inheritdoc />
    public override string Icon => "fas fa-user-secret";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/comic-nodes/create-comic-info";

    /// <summary>
    /// Gets or sets if comics are put into publisher folders
    /// </summary>
    [Boolean(1)]
    public bool Publisher { get; set; }
    
    /// <summary>
    /// Gets or sets if the file should be renamed
    /// </summary>
    [Boolean(2)]
    public bool RenameFile { get; set; }
    
    /// <summary>
    /// Gets or sets how many digits the issues should use when renaming
    /// </summary>
    [NumberInt(3)]
    [DefaultValue(3)]
    [ConditionEquals(nameof(RenameFile), true)]
    public int IssueDigits { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var localFileResult = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFileResult.Failed(out string error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }

        var localFile = localFileResult.Value;

        var infoResult = GetInfo(args.Logger, args.LibraryFileName, args.LibraryPath, Publisher);
        if (infoResult.Failed(out error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }

        var info = infoResult.Value;
        args.Logger?.ILog("Got ComicInfo from filename");

        var newMetadata = new Dictionary<string, object?>
            {
                { nameof(info.Title), info.Title },
                { nameof(info.Series), info.Series },
                { nameof(info.Publisher), info.Publisher },
                { nameof(info.Number), info.Number },
                { nameof(info.Count), info.Count },
                { nameof(info.Volume), info.Volume },
                { nameof(info.AlternateSeries), info.AlternateSeries },
                { nameof(info.AlternateNumber), info.AlternateNumber },
                { nameof(info.AlternateCount), info.AlternateCount },
                { nameof(info.Summary), info.Summary },
                { nameof(info.Notes), info.Notes },
                { nameof(info.ReleaseDate), info.ReleaseDate },
                { nameof(info.Tags), info.Tags?.Any() == true ? string.Join(", ", info.Tags) : null }
            }.Where(x => x.Value != null)
            .ToDictionary(x => x.Key, x => x.Value);
        
        if (args.Metadata != null)
        {
            foreach (var key in args.Metadata.Keys)
            {
                if (newMetadata.ContainsKey(key))
                    continue;
                newMetadata[key] = args.Metadata[key];
            }
        }

        var result = args.ArchiveHelper.FileExists(localFile, "comicinfo.xml");
        if (result.Failed(out error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }

        if (result.Value && RenameFile == false)
        {
            args.Logger?.ILog("comicinfo.xml already present");
            return 2;
        }

        if (result.Value == false)
        {
            string xml = SerializeToXml(info);
            if (string.IsNullOrWhiteSpace(xml))
            {
                args.FailureReason = "Failed to serialize to XML";
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }

            args.Logger?.ILog("Created XML of ComicInfo");

            string comicInfoFile = FileHelper.Combine(args.TempPath, "comicinfo.xml");
            args.Logger?.ILog("comicinfo.xml created: " + comicInfoFile);
            File.WriteAllText(comicInfoFile, xml);

            if (args.ArchiveHelper.AddToArchive(localFile, comicInfoFile).Failed(out error))
            {
                args.FailureReason = error;
                args.Logger?.ELog(error);
                return -1;
            }

            args.Logger?.ILog("Added comicinfo.xml to archive");
            if (localFile != args.WorkingFile &&
                args.FileService.FileMove(localFile, args.WorkingFile).Failed(out error))
            {
                args.Logger?.ELog("Failed to move saved file: " + error);
            }
        }

        if (RenameFile)
        {
            var newNameResult = GetNewName(info, FileHelper.GetExtension(args.WorkingFile), IssueDigits);
            if (newNameResult.Failed(out error))
            {
                args.Logger?.WLog(error);
                return 1;
            }
            string path = FileHelper.GetDirectory(args.WorkingFile);
            string newFile = FileHelper.Combine(path, newNameResult.Value);
            args.Logger?.ILog("New file name: " + newFile);
            if (args.FileService.FileMove(args.WorkingFile, newFile).Failed(out error))
            {
                args.Logger?.WLog("Failed ot rename file: " + error);
            }
            
            args.SetWorkingFile(newFile);
        }
        
        return 1;
    }

    /// <summary>
    /// Gets the name of the book
    /// </summary>
    /// <param name="info">the comic info</param>
    /// <param name="extension">the extension to use</param>
    /// <param name="issueDigits">the number of digits to pad by</param>
    /// <returns>the new name</returns>
    internal static Result<string> GetNewName(ComicInfo info, string extension, int issueDigits)
    {
        if (info.Number == null && info.Volume?.StartsWith("Volume") == false)
            return Result<string>.Fail("No issue number found, cannot rename");
        if (string.IsNullOrWhiteSpace(info.Series))
            return Result<string>.Fail("No series found, cannot rename");

        string name = info.Series;
        if (info.Number != null)
        {
            if (issueDigits > 0)
            {
                if (info.Volume == "Annual")
                {
                    name += " - Annual " + info.Number;
                }
                else if (info.Number > 1960)
                {
                    name += " - " + info.Number;
                }
                else
                {
                    // Pad the number with leading zeros based on the specified number of digits
                    string paddedNumber = info.Number.Value.ToString().PadLeft(issueDigits, '0');

                    // Add the padded number to the name
                    name += $" - #{paddedNumber}";

                    // Optionally add information about the count
                    if (info.Count > 0)
                    {
                        name += $" (of {info.Count})";
                    }
                }
            }
            else
                name += $" - #{info.Number + (info.Count > 0 ? $" (of {info.Count})" : "")}";
            
        }
        else
            name += " - " + info.Volume;
            
        if (string.IsNullOrWhiteSpace(info.Title) == false)
            name += " - " + info.Title;
        
        if(info.Tags?.Any() == true)
            name += " " + string.Join(" ", info.Tags.Select(x => "(" + x + ")"));

        return name += "." + extension.TrimStart('.');
    }

    /// <summary>
    /// Gets the comic book info from its file name
    /// </summary>
    /// <param name="logger">the logger to log with</param>
    /// <param name="libraryFile">the library filename to use</param>
    /// <param name="libraryPath">the library path</param>
    /// <param name="publisher">if there is a publisher folder</param>
    /// <returns>the comic info</returns>
    public static Result<ComicInfo> GetInfo(ILogger? logger, string libraryFile, string libraryPath, bool publisher)
    {
        // Publisher / Series (year?) / Title - #number (of #)- Issue Title.extension
        ComicInfo info = new();
        info.Series = FileHelper.GetDirectoryName(libraryFile);
        if (info.Series.ToLowerInvariant() is "annuals" or "specials")
        {
            // go up one more directory
            info.Series = FileHelper.GetDirectoryName(FileHelper.GetDirectory(libraryFile));
        }

        var yearMatch = Regex.Match(info.Series, @"\((?<year>(19|20)\d{2})\)");
        int? year = null;
        bool yearInFolder = yearMatch.Success;

        if (yearInFolder)
        {
            year = int.Parse(yearMatch.Groups["year"].Value);
            info.Volume = year.ToString();
            // info.Series = Regex.Replace(info.Series, @"\((19|20)\d{2}\)", "").Trim();
        }

        string relative = libraryFile[(libraryPath.Length + 1)..];

        info.Publisher = publisher ? relative.Replace("\\", "/").Split('/').First() : null;
        string shortname = FileHelper.GetShortFileName(libraryFile);
        info.Tags = GetTags(ref shortname);
        shortname = shortname[..shortname.LastIndexOf('.')];
        (shortname, int? year2) = ExtractYear(shortname);
        year ??= year2;
        // Title - #number (of #) - Issue Title 
        var parts = shortname.Split(" - ");
        if (parts.Length < 2)
        {
            parts = shortname.Split('#');
            if (parts.Length < 2)
            {
                // remove any junk
                shortname = Regex.Replace(shortname, @"\s*\([^)]*\)\s*", " ").Trim();
                var lastChanceMatch = Regex.Match(shortname, @"(\d)+$");
                if(lastChanceMatch.Success)
                {
                    info.Number = int.Parse(lastChanceMatch.Value);
                    return info;
                }

                if (shortname.ToLowerInvariant().Contains("annual") && year != null)
                {
                    info.Volume = "Annual";
                    info.Number = year.Value;
                    return info;
                }

                if (year != null && yearInFolder == false)
                {
                    info.Number = year;
                    return info;
                }
                return Result<ComicInfo>.Fail("Invalid filename: " + shortname);
            }

            parts[1] = '#' + parts[1];
        }

        var issueNumberMatch = Regex.Match(parts[1], @"(^|#)(?<first>\d+)(?:\s+of\s+[#]?(?<second>\d+))?");

        if (issueNumberMatch.Success)
        {
            info.Number = int.Parse(issueNumberMatch.Groups["first"].Value);
            info.Count = issueNumberMatch.Groups["second"].Success
                ? int.Parse(issueNumberMatch.Groups["second"].Value)
                : null;
        }
        else
        {
            var volMatch = Regex.Match(parts[1], @"\b[Vv](?:olume|ol)?\s*(\d+)\b", RegexOptions.IgnoreCase);
            if (volMatch.Success)
            {
                info.Volume = "Volume " + int.Parse(volMatch.Groups[1].Value);
            }
            else
            {
                logger?.WLog("Issue number not found in: " + parts[1]);
            }
        }

        if (parts.Length > 2)
        {
            info.Title = Regex.Replace(parts[2], @"\s*\([^)]*\)\s*", " ").Trim();
        }

        return info;
    }

    /// <summary>
    /// Extracts the year from a file
    /// </summary>
    /// <param name="shortname">the name of the file</param>
    /// <returns>the new name and the year if found</returns>
    private static (string Name, int? Year) ExtractYear(string shortname)
    {
        int? year = null;

        // Regular expression to match the year in the specified formats
        Match match = Regex.Match(shortname, @"(?:(?:19|20)\d{2})|\((?:19|20)\d{2}\)|-(?:\s)?(?:19|20)\d{2}(?:\s)?-");

        if (match.Success)
        {
            // Extract the matched year
            year = int.Parse(match.Value.Trim('(', ')', '-', ' '));

            // Remove the matched year from the input string
            shortname = shortname.Remove(match.Index, match.Length).Trim();
        }
        else
        {
            match = Regex.Match(shortname, @"'(\d{2})\b");

            if (match.Success)
            {
                // Extract the matched year
                int number = int.Parse(match.Groups[1].Value);
                // Determine the year
                year = number > 50 ? 1900 + number : 2000 + number;
                // Remove the matched year from the input string
                shortname = shortname.Remove(match.Index, match.Length).Trim();
            }
        }

        return (shortname, year);
    }


    /// <summary>
    /// Gets the tag from the filename
    /// </summary>
    /// <param name="input">the input filename</param>
    /// <returns>the tags</returns>
    static string[] GetTags(ref string input)
    {
        List<string> extracted = new List<string>();
        string pattern = @"\[(.*?)\]";

        MatchCollection matches = Regex.Matches(input, pattern);
        foreach (Match match in matches)
        {
            string extractedString = match.Groups[1].Value;
            extracted.Add(extractedString);
        }

        // Remove the substrings inside square brackets
        input = Regex.Replace(input, pattern, "").Trim();

        return extracted.ToArray();
    }


    /// <summary>
    /// Serializes a ComicInfo object to XML according to a specific schema.
    /// </summary>
    /// <param name="comicInfo">The ComicInfo object to serialize.</param>
    public static string SerializeToXml(ComicInfo comicInfo)
    {
        using var stream = new MemoryStream();
        using var writer = XmlWriter.Create(stream, new()
        {
            Indent = true,
            Encoding = Encoding.UTF8
        });
        writer.WriteStartDocument();
        writer.WriteStartElement("ComicInfo", "http://www.w3.org/2001/XMLSchema");

        WriteXmlElement(writer, "Title", comicInfo.Title);
        WriteXmlElement(writer, "Series", comicInfo.Series);
        WriteXmlElement(writer, "Number", comicInfo.Number);
        WriteXmlElement(writer, "Count", comicInfo.Count);
        WriteXmlElement(writer, "Volume", comicInfo.Volume);
        WriteXmlElement(writer, "AlternateSeries", comicInfo.AlternateSeries);
        WriteXmlElement(writer, "AlternateNumber", comicInfo.AlternateNumber);
        WriteXmlElement(writer, "AlternateCount", comicInfo.AlternateCount);
        WriteXmlElement(writer, "Summary", comicInfo.Summary);
        WriteXmlElement(writer, "Notes", comicInfo.Notes);
        if (comicInfo.Tags?.Any() == true)
            WriteXmlElement(writer, "Tags", string.Join(",", comicInfo.Tags));

        if (comicInfo.ReleaseDate != null)
        {
            WriteXmlElement(writer, "Year", comicInfo.ReleaseDate.Value.Year.ToString());
            WriteXmlElement(writer, "Month", comicInfo.ReleaseDate.Value.Month.ToString());
            WriteXmlElement(writer, "Day", comicInfo.ReleaseDate.Value.Day.ToString());
        }

        writer.WriteEndElement(); // Close ComicInfo
        writer.WriteEndDocument();
        writer.Flush();
        
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Writes an XML element to the XML writer with proper encoding.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="name">The name of the XML element.</param>
    /// <param name="value">The value of the XML element.</param>
    static void WriteXmlElement(XmlWriter writer, string name, object? value)
    {
        if (value == null)
            return;

        writer.WriteStartElement(name);
        writer.WriteValue(value);
        writer.WriteEndElement();
    }
}