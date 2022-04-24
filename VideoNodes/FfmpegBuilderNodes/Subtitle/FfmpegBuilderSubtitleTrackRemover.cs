namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderSubtitleTrackRemover : FfmpegBuilderNode
{
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Subtitle-Track-Remover";

    public override string Icon => "fas fa-comment";

    public override int Outputs => 2; 


    [TextVariable(1)]
    public string Pattern { get; set; }

    [Boolean(2)]
    public bool NotMatching { get; set; }

    [Boolean(3)]
    public bool UseLanguageCode { get; set; }

    public override int Execute(NodeParameters args)
    {
        this.Init(args);
        bool removing = false;
        var regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
        foreach(var stream in Model.SubtitleStreams)
        {
            string str = UseLanguageCode ? stream.Stream.Language : stream.Stream.Title;
            bool matches = false;
            if (string.IsNullOrEmpty(str))
                matches = false; // doesn't match since its empty
            else 
                matches = regex.IsMatch(str);                

            if (NotMatching)
                matches = !matches;
            if (matches)
            {
                stream.Deleted = true;
                removing = true;
            }
        }
        return removing ? 1 : 2;
    }
}
