using System.Security.Cryptography;

namespace ChecksumNodes
{
    public class MD5:Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string Icon => "fas fa-file-contract";
        /// <summary>
        /// Get the help URL
        /// </summary>
        public override string HelpUrl => "https://fileflows.com/docs/plugins/checksum-nodes/md5";

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public MD5()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "Checksum", "4A4566696CC81C6053EC708975767498" },
                { "MD5", "4A4566696CC81C6053EC708975767498" }
            };
        }

        public override int Execute(NodeParameters args)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            DateTime start = DateTime.Now;
            using var stream = File.OpenRead(args.WorkingFile);
            var hash = md5.ComputeHash(stream);
            string hashStr = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            args.Logger?.ILog("MD5 of working file: " + hashStr);
            args.Logger?.ILog("Time taken to compute hash: " + (DateTime.Now - start)); 
            args.UpdateVariables(new Dictionary<string, object>
            {
                { "MD5", hashStr },
                { "Checksum", hashStr },
            });
            return 1;
        }
    }
}
