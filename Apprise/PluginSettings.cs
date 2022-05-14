namespace FileFlows.Apprise;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// The plugin settings for Apprise
/// </summary>
public class PluginSettings:IPluginSettings
{
    /// <summary>
    /// Gets or sets the URL of the Apprise server
    /// </summary>
    [Text(1)]
    [Required]
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the endpoint of the Apprise server
    /// </summary>
    [Text(2)]
    [Required]
    public string Endpoint { get; set; } = string.Empty;
}
