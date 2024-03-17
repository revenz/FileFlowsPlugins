namespace FileFlows.BasicNodes
{
    using System.ComponentModel.DataAnnotations;

    public class Plugin : FileFlows.Plugin.IPlugin
    {
        /// <inheritdoc />
        public Guid Uid => new Guid("789b5213-4ca5-42da-816e-f2117f00cd16");
        /// <inheritdoc />
        public string Name => "Basic Nodes";
        /// <inheritdoc />
        public string MinimumVersion => "1.0.4.2019";

        /// <inheritdoc />
        public string Icon => "svg:basic";

        /// <inheritdoc />
        public void Init() { }
    }
}
