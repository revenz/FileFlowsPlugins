using System.Security.Cryptography;

namespace ChecksumNodes
{
    public class SHA1:Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Logic;

        public override string Icon => "fas fa-file-contract";
        /// <summary>
        /// Get the help URL
        /// </summary>
        public override string HelpUrl => "https://fileflows.com/docs/plugins/checksum-nodes/sha1";

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public SHA1()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "Checksum", "32B26A271530F105CBC35CB653110E1A49D019B6" },
                { "SHA1", "32B26A271530F105CBC35CB653110E1A49D019B6" }
            };
        }

        public override int Execute(NodeParameters args)
        {
            using var hasher = System.Security.Cryptography.SHA1.Create();
            DateTime start = DateTime.Now;
            using var stream = File.OpenRead(args.WorkingFile);
            var hash = hasher.ComputeHash(stream);
            string hashStr = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            args.Logger?.ILog("SHA1 of working file: " + hashStr);
            args.Logger?.ILog("Time taken to compute hash: " + (DateTime.Now - start));
            args.UpdateVariables(new Dictionary<string, object>
            {
                { "SHA1", hashStr },
                { "Checksum", hashStr },
            });
            return 1;
        }
    }
}
