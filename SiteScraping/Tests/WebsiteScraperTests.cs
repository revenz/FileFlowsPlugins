#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.SiteScraping.Tests;

[TestClass]
public class WebsiteScraperTests
{
    [TestMethod]
    public void WebsiteScrapperTest()
    {
        var logger = new TestLogger();
        string file = "/home/john/Pictures/scrapped/urls.txt";
        var temp = "/home/john/Pictures/scrapped/temp";
        if (Directory.Exists(temp) == false)
            Directory.CreateDirectory(temp);
        var args = new NodeParameters(file, logger, false, string.Empty, null)
        {
            TempPath = temp
        };

        var node = new Scraping.WebImageScraper();
        node.MinHeight = 600;
        node.MinWidth = 600;
        node.MaxDepth = 2;
        var result = node.Execute(args);
        var log = logger.ToString();
        File.WriteAllText(Path.Combine(temp, "test.log"), log);
        Assert.AreEqual(1, result);
    }

}

#endif