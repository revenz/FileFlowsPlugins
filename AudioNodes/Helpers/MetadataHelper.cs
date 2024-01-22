namespace FileFlows.AudioNodes.Helpers;

/// <summary>
/// Provides methods for working with audio metadata.
/// </summary>
public class MetadataHelper
{
    /// <summary>
    /// Gets an array of metadata parameters based on the provided AudioTags.
    /// </summary>
    /// <param name="audioInfo">The audio metadata.</param>
    /// <returns>An array of metadata parameters.</returns>
    public static string[] GetMetadataParameters(AudioInfo audioInfo)
    {
        if (audioInfo == null)
            return new string[] { };
        var parameters = new List<string>();

        AddMetadataParameter(parameters, "TITLE", audioInfo.Title);
        AddMetadataParameter(parameters, "ARTIST", audioInfo.Artist);
        AddMetadataParameter(parameters, "ALBUM", audioInfo.Album);
        AddMetadataParameter(parameters, "GENRE", string.Join(";", audioInfo?.Genres ?? new string[] { }));

        if (audioInfo.Date.Year > 1900)
        {
            if (audioInfo.Date.Month == 1 && audioInfo.Date.Day == 1)
                AddMetadataParameter(parameters, "DATE", audioInfo.Date.Year.ToString());
            else
                AddMetadataParameter(parameters, "DATE", audioInfo.Date.ToString("yyyy-M-d"));
        }

        if (audioInfo.Track > 0 && audioInfo.TotalTracks > 0)
            AddMetadataParameter(parameters, "TRACK", audioInfo.Track + "/" + audioInfo.TotalTracks);
        else if(audioInfo.Track > 0)
            AddMetadataParameter(parameters, "TRACK", audioInfo.Track.ToString());
        
        if(audioInfo.Disc > 0)
            AddMetadataParameter(parameters, "DISC", audioInfo.Disc.ToString());
        if(audioInfo.TotalDiscs > 0)
            AddMetadataParameter(parameters, "TOTALDISCS", audioInfo.TotalDiscs.ToString());
        AddMetadataParameter(parameters, "comment", "Created by FileFlows\nhttps://fileflows.com");

        return parameters.ToArray();
    }

    /// <summary>
    /// Adds a metadata parameter to the list if the value is valid.
    /// </summary>
    /// <param name="parameters">The list of parameters.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    private static void AddMetadataParameter(List<string> parameters, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value) == false && value != "0")
        {
            parameters.AddRange(new[] { "-metadata", $"{key}={EscapeForShell(value)}" });
        }
    }

    /// <summary>
    /// Escapes a string for use in a command-line shell.
    /// </summary>
    /// <param name="input">The input string to escape.</param>
    /// <returns>The escaped string.</returns>
    private static string EscapeForShell(string input)
    {
        return input.Replace("\"", "\\\"");
    }
}