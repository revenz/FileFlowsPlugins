#if(DEBUG)

namespace BasicNodes.Tests
{
    using BasicNodes.Tools;
    using FileFlows.BasicNodes.File;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WebRequestTests
    {
        [TestMethod]
        public void WebRequest_PostJson()
        {
            var logger = new TestLogger();
            var args = new FileFlows.Plugin.NodeParameters(@"c:\test\testfile.mkv", logger, false, string.Empty);
            
            WebRequest node = new();
            node.Method = "POST";
            node.Url = "http://localhost:7096/Users/New";
            node.ContentType = "application/json";
            node.Headers = new List<KeyValuePair<string, string>>();
            node.Headers.Add(new KeyValuePair<string, string> ("X-MediaBrowser-Token", ""));
            node.Body = @$"{{ 
    ""Name"": ""{Guid.NewGuid()}""
}}";

            var result = node.Execute(args);

            string body = node.Variables["web.Body"] as string;
            Assert.IsFalse(string.IsNullOrWhiteSpace(body));

            Assert.AreEqual(1, result);
        }
    }
}

#endif