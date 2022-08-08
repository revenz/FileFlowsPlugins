using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlows.VideoNodes.Helpers;

/// <summary>
/// Helper for Subtitles
/// </summary>
internal class SubtitleHelper
{
    /// <summary>
    /// Tests if a subtitle is an image based subtitle
    /// </summary>
    /// <param name="codec">the subtitle codec</param>
    /// <returns>true if the subtitle is an image based subtitle</returns>
    internal static bool IsImageSubtitle(string codec)
        => Regex.IsMatch(codec.Replace("_", ""), "dvbsub|dvdsub|pgs|xsub", RegexOptions.IgnoreCase);
}
