using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Node that removes metadata from a file
/// </summary>
public class FfmpegBuilderMetadataRemover : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the Help URL for this node
    /// </summary>
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/metadata-remover";

    /// <summary>
    /// Gets the icon for this node
    /// </summary>
    public override string Icon => "fas fa-remove-format";

    /// <summary>
    /// Gets the number of outputs for this node
    /// </summary>
    public override int Outputs => 1;

    /// <summary>
    /// Gets or sets if should run against video tracks
    /// </summary>
    [Boolean(1)]
    public bool Video { get; set; }

    /// <summary>
    /// Gets or sets if should run against audio tracks
    /// </summary>
    [Boolean(2)]
    public bool Audio { get; set; }

    /// <summary>
    /// Gets or sets if should run against subtitle tracks
    /// </summary>
    [Boolean(3)]
    public bool Subtitle { get; set; }

    /// <summary>
    /// Gets or sets if images should be removed
    /// </summary>
    [Boolean(4)]
    public bool RemoveImages { get; set; }

    /// <summary>
    /// Gets or sets if title should be removed
    /// </summary>
    [Boolean(5)]
    public bool RemoveTitle { get; set; }

    /// <summary>
    /// Gets or sets if language should be removed
    /// </summary>
    [Boolean(6)]
    public bool RemoveLanguage { get; set; }

    /// <summary>
    /// Gets or sets if additional metadata should be removed
    /// </summary>
    [Boolean(6)]
    public bool RemoveAdditionalMetadata { get; set; }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output number to execute next</returns>
    public override int Execute(NodeParameters args)
    {
        if (Video)
            Process(Model.VideoStreams);

        if (Audio)
            Process(Model.AudioStreams);

        if (Subtitle)
            Process(Model.SubtitleStreams);

        if (RemoveAdditionalMetadata)
        {
            Model.CustomParameters.AddRange(new[] { "-map_metadata", "-1" });
            Model.ForceEncode = true;
        }


        if (RemoveImages)
        {
            foreach (var video in Model.VideoStreams)
            {
                if (video.Stream.IsImage)
                    video.Deleted = true;
            }
        }

        return 1;
    }

    private void Process<T>(List<T> streams) where T : FfmpegStream
    {
        if (streams == null)
            return;
        foreach (var stream in streams)
            Process(stream);
    }

    private void Process(FfmpegStream stream)
    {
        if (RemoveTitle)
        {
            stream.Title = string.Empty;
        }
        
        if (stream is FfmpegAudioStream audio)
        {
            if (RemoveLanguage)
                audio.Language = string.Empty;
        }
        else if (stream is FfmpegSubtitleStream sub)
        {            
            if (RemoveLanguage)
                sub.Language = string.Empty;
        }
    }
}
