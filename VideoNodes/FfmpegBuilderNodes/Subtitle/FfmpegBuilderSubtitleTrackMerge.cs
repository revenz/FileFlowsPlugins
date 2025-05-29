using System.IO;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Merges a subtitle into the FFmpeg Builder model
/// </summary>
public class FfmpegBuilderSubtitleTrackMerge : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/subtitle-track-merge";
    /// <inheritdoc />
    public override string Icon => "fas fa-comment-medical";
    /// <inheritdoc />
    public override int Outputs => 2;

    /// <summary>
    /// Subtitles to include
    /// </summary>
    [Checklist(nameof(Options), 1)]
    [Required]
    public List<string> Subtitles { get; set; }

    private static List<ListOption>? _Options;
    /// <summary>
    /// Options for the subtitles 
    /// </summary>
    public static List<ListOption> Options
    {
        get
        {
            if (_Options == null)
            {
                _Options = new List<ListOption>
                {
                    new () { Value = "ass", Label = "ass: Advanced SubStation Alpha"},
                    new () { Value = "idx", Label = "idx: IDX"},
                    new () { Value = "srt", Label = "srt: SubRip subtitle"},
                    new () { Value = "ssa", Label = "ssa: SubStation Alpha"},
                    new () { Value = "sub", Label = "sub: SubStation Alpha"},
                    new () { Value = "sup", Label = "sup: SubPicture"},
                    new () { Value = "txt", Label = "txt: Raw text subtitle"}                        
                };
            }
            return _Options;
        }
    }

    /// <summary>
    /// Gets or sets if the source directory should be used or the working directory
    /// </summary>
    [Boolean(2)]
    [DefaultValue(true)]
    public bool UseSourceDirectory { get; set; } = true;

    /// <summary>
    /// Gets or sets if the subtitle must match the filename
    /// </summary>
    [Boolean(3)]
    public bool MatchFilename { get; set; }
    
    /// <summary>
    /// Gets or sets the pattern to use
    /// </summary>
    [TextVariable(4)]
    [ConditionEquals(nameof(MatchFilename), false)]
    public string Pattern { get; set; }
    
    /// <summary>
    /// Gets or sets the title of the subtitle
    /// </summary>
    [TextVariable(5)]
    public string Title { get; set; }
    
    /// <summary>
    /// Gets or sets the language
    /// </summary>
    [TextVariable(6)]
    public string Language { get; set; }
    
    /// <summary>
    /// Gets or sets if the subtitle is forced
    /// </summary>
    [Boolean(7)]
    public bool Forced { get; set; }
    
    /// <summary>
    /// Gets or sets if the subtitle is to be marked as the default
    /// </summary>
    [Boolean(8)]
    public bool Default { get; set; }

    /// <summary>
    /// Gets or sets if the subtitle should be deleted afterwards
    /// </summary>
    [Boolean(9)]
    public bool DeleteAfterwards { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var dir = UseSourceDirectory ? FileHelper.GetDirectory(args.LibraryFileName) : args.TempPath;
        if (args.FileService.DirectoryExists(dir).Is(true) == false)
        {
            args.Logger?.ILog("Directory does not exist: " + dir);
            return 2;
        }
        foreach(var sub in Subtitles)
        {
            args.Logger.ILog("Add Subtitle Extension: " + sub);
        }

        int count = 0;
        var files = args.FileService.GetFiles(dir);
        if (files.IsFailed)
        {
            args.Logger?.ILog("Failed getting files: "+ files.Error);
            return 2;
        }

        string? pattern = args.ReplaceVariables(Pattern ?? string.Empty, stripMissing: true).EmptyAsNull();
        
        foreach (var file in files.ValueOrDefault ?? new string[] {})
        {
            string ext = FileHelper.GetExtension(file).TrimStart('.');
            if (string.IsNullOrEmpty(ext) || ext.Length < 2)
                continue;
            if (Subtitles.Contains(ext.ToLowerInvariant()) == false)
                continue;

            string language = string.Empty;
            bool forced = false;

            if (MatchFilename)
            {
                string lang1, lang2;
                bool matchesOriginal = FilenameMatches(args.FileName, file, out lang1, out bool forced1);
                bool matchesWorking = FilenameMatches(args.WorkingFile, file, out lang2, out bool forced2);

                if (matchesOriginal == false && matchesWorking == false)
                    continue;

                if (string.IsNullOrEmpty(lang1) == false)
                    language = lang1;
                if (string.IsNullOrEmpty(lang2) == false)
                    language = lang2;
                forced = forced1 || forced2;
            }
            else if (pattern != null)
            {
                string filename = new FileInfo(file).Name;
                if (Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) == false)
                {
                    args.Logger?.ILog("Does not match pattern: " + filename);
                    continue;
                }
                args.Logger?.ILog("Matches pattern: " + filename);
            }

            language = language?.EmptyAsNull() ?? args.ReplaceVariables(Language ?? string.Empty, stripMissing: true);
            forced |= Forced;

            string subTitle = args.ReplaceVariables(Title ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? language ?? string.Empty;
            
            language = LanguageHelper.GetIso2Code(language.Split(' ').First()); // remove any SDH etc
            args.Logger.ILog("Adding file: " + file + " [" + ext + "]" + (string.IsNullOrEmpty(language) == false ? " (Language: " + language + ")" : ""));
            this.Model.InputFiles.Add(new InputFile(file) { DeleteAfterwards = this.DeleteAfterwards });


            this.Model.SubtitleStreams.Add(new FfmpegSubtitleStream
            {
                Title = subTitle,
                Language = string.IsNullOrEmpty(language) ? null : Regex.Replace(language, @" \([\w]+\)$", string.Empty).Trim(),
                IsDefault = Default,
                IsForced = forced,
                Stream = new SubtitleStream()
                {
                    InputFileIndex = this.Model.InputFiles.Count - 1,
                    TypeIndex = 0,
                    Language = language,
                    Forced = forced,
                    Title = subTitle,
                    Default = Default,
                    Codec = ext,
                    IndexString = (this.Model.InputFiles.Count - 1) + ":s:0"
                },
                
            });
            ++count;
        }
        args.Logger.ILog("Subtitles added: " + count);
        if (count > 0)
            this.Model.ForceEncode = true;
        return count > 0 ? 1 : 2;
    }

    /// <summary>
    /// Checks if the filename matches
    /// </summary>
    /// <param name="input">the input file</param>
    /// <param name="other">the other file to check</param>
    /// <param name="languageCode">the language code found in the subtitle</param>
    /// <param name="forced">if the subtitle is detected as forced</param>
    /// <returns>true if it matches, otherwise false</returns>
    internal bool FilenameMatches(string input, string other, out string languageCode, out bool forced)
    {
        languageCode = string.Empty;
        forced = false;
        
        string inputName = FileHelper.GetShortFileNameWithoutExtension(input);
        string otherName = FileHelper.GetShortFileNameWithoutExtension(other);
        string testString = otherName;
        if (otherName.StartsWith(inputName))
            testString = otherName[inputName.Length..].TrimStart('.', '-', '_', ' ');
            
        
        if (inputName.ToLowerInvariant().Equals(otherName.ToLowerInvariant()))
            return true;

        bool closedCaptions = HasSection(ref testString, "closedcaptions") || HasSection(ref testString, "cc");
        forced = HasSection(ref testString, "forced");


        if(Regex.IsMatch(testString, @"(\.[a-zA-Z]{2,3}){1,2}$"))
        {
            string stripLang = Regex.Replace(testString, @"(\.[a-zA-Z]{2,3}){1,2}$", string.Empty).Replace("  ", " ").Trim();

            var rgxLanguage = new Regex("(?<=(\\.))(" + string.Join("|", LanguageCodes.Codes.Keys) + ")");
            if (rgxLanguage.IsMatch(testString))
            {
                string key = rgxLanguage.Match(testString).Value;
                languageCode = LanguageCodes.Codes.GetValueOrDefault(key, key);
            }

            if (string.IsNullOrEmpty(languageCode) == false)
            {
                if (Regex.IsMatch(testString, @"\.hi(\.|$)"))
                    languageCode += " (HI)";
                if (closedCaptions || Regex.IsMatch(testString, @"\.cc(\.|$)"))
                    languageCode += " (CC)";
                if (Regex.IsMatch(testString, @"\.sdh(\.|$)"))
                    languageCode += " (SDH)";
            }


            if (inputName.ToLowerInvariant().Equals(stripLang.ToLowerInvariant()))
                return true;
        }

        if (Regex.IsMatch(testString, @"\([a-zA-Z]{2,3}\)"))
        {
            string stripLang = Regex.Replace(testString, @"\([a-zA-Z]{2,3}\)", string.Empty).Replace("  ", " ").Trim();

            var rgxLanguage = new Regex("(?<=(\\())(" + string.Join("|", LanguageCodes.Codes.Keys) + ")(?!=\\))");
            if (rgxLanguage.IsMatch(testString))
            {
                string key = rgxLanguage.Match(testString).Value;
                languageCode = LanguageCodes.Codes.GetValueOrDefault(key, key);
            }


            if (string.IsNullOrEmpty(languageCode) == false)
            {
                if (other.ToLowerInvariant().Contains("(hi)"))
                    languageCode += " (HI)";
                else if (closedCaptions || other.ToLowerInvariant().Contains("(cc)")|| other.ToLowerInvariant().Contains("closedcaptions"))
                    languageCode += " (CC)";
                else if (other.ToLowerInvariant().Contains("(sdh)"))
                    languageCode += " (SDH)";
            }
            if (inputName.ToLowerInvariant().Equals(stripLang.ToLowerInvariant()))
                return true;
        }

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            var iso2 = LanguageHelper.GetIso2Code(testString.ToUpper());
            if(iso2 != testString.ToUpper())
                languageCode = iso2; // since this should be lowered case if known
        }

        if (otherName.StartsWith(inputName, StringComparison.InvariantCultureIgnoreCase))
            return true;

        return false;

        bool HasSection(ref string input, string section)
        {
            var matchDot = new Regex(@"\." + Regex.Escape(section) + @"(\.|$)", RegexOptions.IgnoreCase);
            if (matchDot.IsMatch(input))
            {
                input = matchDot.Replace(input, string.Empty)
                    .Replace("..", ".")
                    .Replace("  ", " ")
                    .Replace("--", "-")
                    .TrimExtra(".-");
                return true;
            }
            var matchBracket = new Regex(@"\(" + Regex.Escape(section) + @"(\)|$)", RegexOptions.IgnoreCase);
            if (matchBracket.IsMatch(input))
            {
                input = matchBracket.Replace(input, string.Empty)
                    .Replace("..", ".")
                    .Replace("  ", " ")
                    .Replace("--", "-")
                    .TrimExtra(".-");
                return true;
            }
            var matchHyphen = new Regex(@"\-" + Regex.Escape(section) + @"(\-|$)", RegexOptions.IgnoreCase);
            if (matchHyphen.IsMatch(input))
            {
                input = matchHyphen.Replace(input, string.Empty)
                    .Replace("..", ".")
                    .Replace("  ", " ")
                    .Replace("--", "-")
                    .TrimExtra(".-");
                return true;
            }
            return false;

        }
    }
}