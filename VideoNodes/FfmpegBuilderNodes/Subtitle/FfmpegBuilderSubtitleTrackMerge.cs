using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderSubtitleTrackMerge : FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/subtitle-track-merge";
    
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
                    new ListOption { Value = "ass", Label = "ass: Advanced SubStation Alpha"},
                    new ListOption { Value = "idx", Label = "idx: IDX"},
                    new ListOption { Value = "srt", Label = "srt: SubRip subtitle"},
                    new ListOption { Value = "ssa", Label = "ssa: SubStation Alpha"},
                    new ListOption { Value = "sub", Label = "sub: SubStation Alpha"},
                    new ListOption { Value = "sup", Label = "sup: SubPicture"},
                    new ListOption { Value = "txt", Label = "txt: Raw text subtitle"}                        
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
    
    public override int Execute(NodeParameters args)
    {

        var dir = UseSourceDirectory ? new FileInfo(args.FileName).Directory : new DirectoryInfo(args.TempPath);
        if (dir.Exists == false)
        {
            args.Logger?.ILog("Directory does not exist: " + dir.FullName);
            return 2;
        }
        foreach(var sub in Subtitles)
        {
            args.Logger.ILog("Add Subtitle Extension: " + sub);
        }

        int count = 0;
        foreach (var file in dir.GetFiles())
        {
            string ext = file.Extension;
            if (string.IsNullOrEmpty(ext) || ext.Length < 2)
                continue;
            ext = ext.Substring(1).ToLower();// remove .
            if (Subtitles.Contains(ext) == false)
                continue;

            string language = string.Empty;

            if (MatchFilename)
            {
                string lang1, lang2;
                bool matchesOriginal = FilenameMatches(args.FileName, file.FullName, out lang1);
                bool matchesWorking = FilenameMatches(args.WorkingFile, file.FullName, out lang2);

                if (matchesOriginal == false && matchesWorking == false)
                    continue;

                if (string.IsNullOrEmpty(lang1) == false)
                    language = lang1;
                if (string.IsNullOrEmpty(lang2) == false)
                    language = lang2;
            }

            args.Logger.ILog("Adding file: " + file.FullName + " [" + ext + "]" + (string.IsNullOrEmpty(language) == false ? " (Language: " + language + ")" : ""));
            this.Model.InputFiles.Add(file.FullName);
            this.Model.SubtitleStreams.Add(new FfmpegSubtitleStream
            {
                Stream = new SubtitleStream()
                {
                    InputFileIndex = this.Model.InputFiles.Count - 1,
                    TypeIndex = 0,
                    Language = language,
                    Title = file.Name.Replace(file.Extension, ""),
                    Codec = file.Extension[1..],
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

    internal bool FilenameMatches(string input, string other, out string languageCode)
    {
        languageCode = String.Empty;
        var inputFile = new FileInfo(input);
        string inputName = inputFile.Name.Replace(inputFile.Extension, "");

        var otherFile = new FileInfo(other);
        string otherName = otherFile.Name.Replace(otherFile.Extension, "");

        if (inputName.ToLowerInvariant().Equals(otherName.ToLowerInvariant()))
            return true;

        if(Regex.IsMatch(otherName, @"(\.[a-zA-Z]{2,3}){1,2}$"))
        {
            string stripLang = Regex.Replace(otherName, @"(\.[a-zA-Z]{2,3}){1,2}$", string.Empty).Replace("  ", " ").Trim();

            var rgxLanguage = new Regex("(?<=(\\.))(" + string.Join("|", LanguageCodes.Codes.Keys) + ")");
            if (rgxLanguage.IsMatch(otherName))
            {
                string key = rgxLanguage.Match(otherName).Value;
                languageCode = LanguageCodes.Codes[key];   
            }

            if (string.IsNullOrEmpty(languageCode) == false)
            {
                if (Regex.IsMatch(otherName, @"\.hi(\.|$)"))
                    languageCode += " (HI)";
                if (Regex.IsMatch(otherName, @"\.cc(\.|$)"))
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
                languageCode = LanguageCodes.Codes[key];
            }


            if (string.IsNullOrEmpty(languageCode) == false)
            {
                if (other.ToLower().Contains("(hi)"))
                    languageCode += " (HI)";
                else if (other.ToLower().Contains("(cc)"))
                    languageCode += " (CC)";
                else if (other.ToLower().Contains("(sdh)"))
                    languageCode += " (SDH)";
            }
            if (inputName.ToLowerInvariant().Equals(stripLang.ToLowerInvariant()))
                return true;
        }

        return false;
    }
}
