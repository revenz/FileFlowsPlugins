#if(DEBUG)
namespace CollectionNodes.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DataCollectionTests
    {
        [TestMethod]
        public void DataCollection_NotIn()
        {
            var logger =new TestLogger();
            var args = new NodeParameters(@"D:\videos\injustice.mkv", logger, false, "");
            string dbFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sqlite");
            var dc = new DataCollection()
            {
                Value = "Checksum",
                DatabaseFile = dbFile,
                UpdateIfDifferent = true,
            };
            args.Variables.Add("Checksum", "batman");
            var output = dc.Execute(args);
            Assert.AreEqual(1, output);
        }

        [TestMethod]
        public void DataCollection_InAndSame()
        {
            var logger = new TestLogger();
            var args = new NodeParameters(@"D:\videos\injustice.mkv", logger, false, "");
            string dbFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sqlite");
            var dc = new DataCollection()
            {
                Value = "Checksum",
                DatabaseFile = dbFile,
                UpdateIfDifferent = true,
            };
            args.Variables.Add("Checksum", "batman");
            var output = dc.Execute(args);
            Assert.AreEqual(1, output);

            var output2 = dc.Execute(args);
            Assert.AreEqual(2, output2);
        }

        [TestMethod]
        public void DataCollection_InAndNotSame()
        {
            var logger = new TestLogger();
            var args = new NodeParameters(@"D:\videos\injustice.mkv", logger, false, "");
            string dbFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sqlite");
            var dc = new DataCollection()
            {
                Value = "Checksum",
                DatabaseFile = dbFile,
                UpdateIfDifferent = true,
            };
            args.Variables.Add("Checksum", "batman");
            var output = dc.Execute(args);
            Assert.AreEqual(1, output);

            args.Variables["Checksum"] = "joker";
            var output3 = dc.Execute(args);
            Assert.AreEqual(3, output3);
        }
    }
}
#endif