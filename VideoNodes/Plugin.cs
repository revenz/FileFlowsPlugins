namespace FileFlows.VideoNodes
{
    using System.ComponentModel.DataAnnotations;
    using FileFlows.Plugin.Attributes;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        public string Name => "Video Nodes";

        [Required]
        [File(2, "exe")]
        public string FFProbeExe { get; set; }

        public void Init()
        {
            //var context = new System.Runtime.Loader.AssemblyLoadContext(null, true);
            //context.LoadFromAssemblyPath(@"C:\Users\john\src\ViWatcher\Plugins\VideoNodes\bin\Debug\net6.0\FFMpegCore.dll");
        }
    }
}