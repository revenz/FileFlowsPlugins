using System.Security.Cryptography;

namespace ChecksumNodes
{
    public class SHA512:Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Logic;

        public override string Icon => "fas fa-file-contract";

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public SHA512()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "Checksum", "267dc7710d5938c6dc362dd24b2f3b13e4a75a1f4338c0d0a39786afd67491f13be0525f46b1bcdb7be934248870210e73166f9103063b9a6a986a94dae77d4e" },
                { "SHA512", "267dc7710d5938c6dc362dd24b2f3b13e4a75a1f4338c0d0a39786afd67491f13be0525f46b1bcdb7be934248870210e73166f9103063b9a6a986a94dae77d4e" }
            };
        }

        public override int Execute(NodeParameters args)
        {
            using var hasher = System.Security.Cryptography.SHA512.Create();
            DateTime start = DateTime.Now;
            using var stream = File.OpenRead(args.WorkingFile);
            var hash = hasher.ComputeHash(stream);
            string hashStr = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            args.Logger?.ILog("SHA512 of working file: " + hashStr);
            args.Logger?.ILog("Time taken to compute hash: " + (DateTime.Now - start));
            args.UpdateVariables(new Dictionary<string, object>
            {
                { "SHA512", hashStr },
                { "Checksum", hashStr },
            });
            return 1;
        }
    }
}
