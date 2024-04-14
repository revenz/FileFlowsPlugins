using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderSubtitleTrackMerge : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/subtitle-track-merge";
    
    public override string Icon => "fas fa-comment-medical";

    public override int Outputs => 2;

    [Checklist(nameof(Options), 1)]
    [Required]
    public List<string> Subtitles { get; set; }

    private static List<ListOption> _Options;
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

    [Boolean(2)]
    [DefaultValue(true)]
    public bool UseSourceDirectory { get; set; } = true;

    [Boolean(3)]
    public bool MatchFilename { get; set; }

    [Boolean(4)]
    public bool DeleteAfterwards { get; set; }
    
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

            string subTitle = language;
            language = LanguageHelper.GetIso2Code(language.Split(' ').First()); // remove any SDH etc
            args.Logger.ILog("Adding file: " + file + " [" + ext + "]" + (string.IsNullOrEmpty(language) == false ? " (Language: " + language + ")" : ""));
            this.Model.InputFiles.Add(new InputFile(file) { DeleteAfterwards = this.DeleteAfterwards });

            if (string.IsNullOrEmpty(subTitle))
                subTitle = FileHelper.GetShortFileName(file).Replace("." + ext, "");

            this.Model.SubtitleStreams.Add(new FfmpegSubtitleStream
            {
                Title = subTitle,
                Language = string.IsNullOrEmpty(language) ? null : Regex.Replace(language, @" \([\w]+\)$", string.Empty).Trim(),                
                Stream = new SubtitleStream()
                {
                    InputFileIndex = this.Model.InputFiles.Count - 1,
                    TypeIndex = 0,
                    Language = language,
                    Forced = forced,
                    Title = subTitle,
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

    internal bool FilenameMatches(string input, string other, out string languageCode, out bool forced)
    {
        languageCode = string.Empty;
        forced = false;
        var inputFileExtension = FileHelper.GetExtension(input);
        string inputName = FileHelper.GetShortFileName(input).Replace(inputFileExtension, "");

        var otherFileExtension = FileHelper.GetExtension(other);
        string otherName = FileHelper.GetShortFileName(other).Replace(otherFileExtension, "");
        
        if (inputName.ToLowerInvariant().Equals(otherName.ToLowerInvariant()))
            return true;

        bool closedCaptions = HasSection(ref otherName, "closedcaptions") || HasSection(ref otherName, "cc");
        forced = HasSection(ref otherName, "forced");

        if(Regex.IsMatch(otherName, @"(\.[a-zA-Z]{2,3}){1,2}$"))
        {
            string stripLang = Regex.Replace(otherName, @"(\.[a-zA-Z]{2,3}){1,2}$", string.Empty).Replace("  ", " ").Trim();

            var rgxLanguage = new Regex("(?<=(\\.))(" + string.Join("|", LanguageCodes.Codes.Keys) + ")");
            if (rgxLanguage.IsMatch(otherName))
            {
                string key = rgxLanguage.Match(otherName).Value;
                languageCode = LanguageCodes.Codes.GetValueOrDefault(key, key);
            }

            if (string.IsNullOrEmpty(languageCode) == false)
            {
                if (Regex.IsMatch(otherName, @"\.hi(\.|$)"))
                    languageCode += " (HI)";
                if (closedCaptions || Regex.IsMatch(otherName, @"\.cc(\.|$)"))
                    languageCode += " (CC)";
                if (Regex.IsMatch(otherName, @"\.sdh(\.|$)"))
                    languageCode += " (SDH)";
            }


            if (inputName.ToLowerInvariant().Equals(stripLang.ToLowerInvariant()))
                return true;
        }

        if (Regex.IsMatch(otherName, @"\([a-zA-Z]{2,3}\)"))
        {
            string stripLang = Regex.Replace(otherName, @"\([a-zA-Z]{2,3}\)", string.Empty).Replace("  ", " ").Trim();

            var rgxLanguage = new Regex("(?<=(\\())(" + string.Join("|", LanguageCodes.Codes.Keys) + ")(?!=\\))");
            if (rgxLanguage.IsMatch(otherName))
            {
                string key = rgxLanguage.Match(otherName).Value;
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
