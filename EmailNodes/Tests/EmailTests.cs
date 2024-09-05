// #if(DEBUG)
//
// using FileFlows.Communication;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using PluginTestLibrary;
//
// namespace EmailNodes.Tests;
//
// [TestClass]
// public class VideoInfoHelperTests : TestBase
// {
//     [TestMethod]
//     public void Email_TemplateTest()
//     {
//         var args = GetNodeParameters(TempFile);
//         string test = Guid.NewGuid().ToString("N");
//         args.Variables.Add("TestParameter", test);
//         var node = new SendEmail();
//         node.Body = @"Hello {{TestParameter}}!";
//         string body = node.RenderBody(args);
//         Assert.AreEqual($"Hello {test}!", body);
//     }
//
//     [TestMethod]
//     public void Email_TemplateTest2()
//     {
//         var args = GetNodeParameters(TempFile);
//         string test = Guid.NewGuid().ToString("N");
//         args.Variables.Add("TestParameter", test);
//         var node = new SendEmail();
//         node.Body = @"Hello {{if file.Create.Year > 2021 }}from 2022{{ else }}from before 2022{{ end }}";
//         string body = node.RenderBody(args);
//         Assert.AreEqual($"Hello from before 2022", body);
//     }
//     
//     [TestMethod]
//     public void Email_TemplateTest3()
//     {
//         var args = GetNodeParameters(TempFile);
//         string test = Guid.NewGuid().ToString("N");
//         args.Variables.Add("TestParameter", test);
//         var node = new SendEmail();
//         node.Body = @"Hello {{ if file.Size > 1_000_000 }}greater than 1million{{else}}small file{{end}}";
//         string body = node.RenderBody(args);
//         Assert.AreEqual($"Hello greater than 1million", body);
//     }
// }
//
//
// #endif