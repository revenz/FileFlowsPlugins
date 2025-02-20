namespace FileFlows.VideoNodes.Helpers;

/// <summary>
/// Helper class for audio channel operations.
/// </summary>
public class ChannelHelper
{
    /// <summary>
    /// Converts the number of audio channels to the nearest integer, rounding up only if the fractional part is significant.
    /// Specifically, it rounds up for values like 7.1 (to 8) or 5.1 (to 6) but avoids rounding 5.000001 to 6.
    /// </summary>
    /// <param name="channels">The number of audio channels as a floating-point number.</param>
    /// <returns>The converted number of channels as an integer.</returns>
    public static int GetNumberOfChannels(float channels)
    {
        // Tolerance for floating-point precision errors
        const float tolerance = 0.0001f;

        // Check if the fractional part is close to 0.1 or larger
        if (Math.Abs(channels - Math.Floor(channels) - 0.1f) < tolerance)
        {
            // If near .1, round up
            return (int)Math.Floor(channels) + 1;
        }

        // Otherwise, round to the nearest integer
        return (int)Math.Round(channels);
    }
    
    /// <summary>
    /// Converts the number of audio channels (as a float) into a human-readable format, 
    /// handling floating-point precision issues.
    /// </summary>
    /// <param name="channels">The number of audio channels as a float.</param>
    /// <returns>
    /// A human-readable string representation of the channels, such as "Mono", "Stereo", or "5.1".
    /// Returns <c>null</c> if the number of channels is 0.
    /// </returns>
    public static string? FormatAudioChannels(float channels)
    {
        if (channels == 0) 
            return null;

        if (Approximately(channels, 1f)) return "Mono";
        if (Approximately(channels, 2f)) return "Stereo";
        if (Approximately(channels, 2.1f)) return "2.1";
        if (Approximately(channels, 4f)) return "Quad";
        if (Approximately(channels, 4.1f)) return "4.1";
        if (channels is >= 5f and <= 6f) return "5.1"; // Covers 5, 5.1, 5.0999, 6
        if (Approximately(channels, 6.1f)) return "6.1";
        if (channels is >= 7f and <= 8f) return "7.1"; // Covers 7, 7.1, 8

        return $"{channels} channels";

        static bool Approximately(float a, float b, float epsilon = 0.05f) 
            => Math.Abs(a - b) < epsilon;
    }
}