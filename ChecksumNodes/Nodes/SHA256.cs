using System.Security.Cryptography;

namespace ChecksumNodes
{
    public class SHA256:Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Logic;

        /// <summary>
        /// Get the help URL
        /// </summary>
        public override string HelpUrl => "https://fileflows.com/docs/plugins/checksum-nodes/sha256";
        public override string Icon => "fas fa-file-contract";

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public SHA256()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "Checksum", "BD9B74A682CD757611805F86371A5A277B2941FA42345CE4C87A7C9E28244C2C" },
                { "SHA256", "BD9B74A682CD757611805F86371A5A277B2941FA42345CE4C87A7C9E28244C2C" }
            };
        }

        public override int Execute(NodeParameters args)
        {
            string hashStr = ComputeHash(args.WorkingFile);
            args.Logger?.ILog("SHA256 of working file: " + hashStr);
            args.Logger?.ILog("Time taken to compute hash: " + (DateTime.Now - start)); 
            args.UpdateVariables(new Dictionary<string, object>
            {
                { "SHA256", hashStr },
                { "Checksum", hashStr },
            });
            return 1;
        }

        private static string ComputeHash(string filePath)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            DateTime start = DateTime.Now;
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
