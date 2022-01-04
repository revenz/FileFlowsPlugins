using System.Security.Cryptography;

namespace ChecksumNodes
{
    public class SHA256:Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Logic;

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
            using var hasher = System.Security.Cryptography.SHA256.Create();
            DateTime start = DateTime.Now;
            using var stream = File.OpenRead(args.WorkingFile);
            var hash = hasher.ComputeHash(stream);
            string hashStr = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            args.Logger?.ILog("SHA256 of working file: " + hashStr);
            args.Logger?.ILog("Time taken to compute hash: " + (DateTime.Now - start));
            args.UpdateVariables(new Dictionary<string, object>
            {
                { "SHA256", hashStr },
                { "Checksum", hashStr },
            });
            return 1;
        }
    }
}
