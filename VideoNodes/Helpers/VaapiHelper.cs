namespace FileFlows.VideoNodes.Helpers;

/// <summary>
/// Helper for Vaapi
/// </summary>
class VaapiHelper
{
    internal static bool VaapiLinux => OperatingSystem.IsLinux() && System.IO.File.Exists(VaapiRenderDevice);

    internal const string VaapiRenderDevice = "/dev/dri/renderD128";
}