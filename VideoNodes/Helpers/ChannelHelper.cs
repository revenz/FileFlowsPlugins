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
}