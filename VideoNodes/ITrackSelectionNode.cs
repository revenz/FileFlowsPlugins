using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlows.VideoNodes;

/// <summary>
/// Interface for track selection
/// </summary>
public interface ITrackSelectionNode
{

    [TextVariable(12)]
    public string Title { get; set; }

    [TextVariable(13)]
    public string Codec { get; set; }

    [Boolean(16)]
    public bool NotMatching { get; set; }
}

public interface ITrackSelectionLanguage : ITrackSelectionNode
{

    [TextVariable(14)]
    public string Language { get; set; }
}

/// <summary>
/// Interface for track selection with stream option
/// </summary>
public interface ITrackStreamSelectionNode
{

    [Select(nameof(StreamTypeOptions), 11)]
    public string StreamType { get; set; }
    private static List<ListOption> _StreamTypeOptions;
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new ListOption { Label = "Audio", Value = "Audio" },
                    new ListOption { Label = "Video", Value = "Video" },
                    new ListOption { Label = "Subtitle", Value = "Subtitle" }
                };
            }
            return _StreamTypeOptions;
        }
    }

    [ConditionEquals(nameof(Stream), "Video", inverse: true)]
    [TextVariable(14)]
    public string Language { get; set; }


    [ConditionEquals(nameof(Stream), "Audio")]
    [NumberFloat(17)]
    public float Channels { get; set; }
}



/// <summary>
/// Interface for audio track selection
/// </summary>
public interface ITrackAudioSelectionNode : ITrackSelectionLanguage
{

    [NumberFloat(17)]
    public float Channels { get; set; }
}