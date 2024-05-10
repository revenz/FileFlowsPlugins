// #if(DEBUG)
//
// using FileFlows.ImageNodes.Images;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using System.Runtime.InteropServices;
// using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
//
// namespace FileFlows.ImageNodes.Tests;
//
// [TestClass]
// public class ImageNodesTests
// {
//     string? TestImage1;
//     string? TestImage2;
//     string? TestImageHeic;
//     string TempDir;
//     string? TestCropImage1, TestCropImage2, TestCropImage3, TestCropImage4, TestCropImageNoCrop, TestExif;
//
//     public ImageNodesTests()
//     {
//         bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
//         if (isWindows)
//         {
//             TestImage1 = @"D:\videos\pictures\image1.jpg";
//             TestImage2 = @"D:\videos\pictures\image2.png";
//             TempDir = @"D:\videos\temp";
//             TestCropImage1 = @"D:\images\testimages\crop01.jpg";
//             TestCropImage2 = @"D:\images\testimages\crop02.jpg";
//             TestCropImage3 = @"D:\images\testimages\crop03.jpg";
//             TestCropImage4 = @"D:\images\testimages\crop04.jpg";
//             TestCropImageNoCrop = @"D:\images\testimages\nocrop.jpg";
//         }
//         else
//         {
//             TestCropImage1 = "/home/john/Pictures/cropme2.jpg";
//             TestCropImage2 = "/home/john/Pictures/cropme.jpg";
//             TestCropImage3 = "/home/john/Pictures/crop.heic";
//             TestImageHeic = "/home/john/Pictures/crop.heic";
//             TestImage1 = "/home/john/Pictures/circle.jpg";
//             TestImage2 = "/home/john/Pictures/36410427.png";
//             TempDir = "/home/john/temp/";
//             TestExif = "/home/john/Pictures/exif_test.jpg";
//         }
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_ImageFormat()
//     {
//         var args = new NodeParameters(TestImage1, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageFormat();
//         node.Format = IMAGE_FORMAT_GIF;
//         Assert.AreEqual(1, node.Execute(args));
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_ImageFormat_Heic()
//     {
//         var args = new NodeParameters(TestImageHeic, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageFormat();
//         node.Format = "HEIC";
//         Assert.AreEqual(1, node.Execute(args));
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_IsLandscape_Heic()
//     {
//         var args = new NodeParameters(TestImageHeic, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var imageNode = new ImageFile();
//         imageNode.Execute(args);
//
//         var node = new ImageIsLandscape();
//         node.PreExecute(args);
//         Assert.AreEqual(1, node.Execute(args));
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_Resize()
//     {
//         var logger = new TestLogger();
//         var args = new NodeParameters(TestImage1, logger, false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageResizer();
//         node.Width = 1920;
//         node.Height = 1080;
//         node.Format = string.Empty; 
//         node.Mode = ResizeMode.Max;
//         var result = node.Execute(args);
//         var log = logger.ToString();
//         Assert.AreEqual(1, result);
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_Resize_Heic()
//     {
//         var args = new NodeParameters(TestImageHeic, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageResizer();
//         node.Width = 1000;
//         node.Height = 500;
//         node.Mode = ResizeMode.Fill;
//         Assert.AreEqual(1, node.Execute(args));
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_Resize_Percent()
//     {
//         var args = new NodeParameters(TestImage1, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var imgFile = new ImageFile();
//         imgFile.Execute(args);
//         int width = imgFile.GetImageInfo(args)?.Width ?? 0;
//         int height = imgFile.GetImageInfo(args)?.Height ?? 0;
//
//         var node = new ImageResizer();
//         node.Width = 200;
//         node.Height = 50;
//         node.Percent = true;
//         node.Mode = ResizeMode.Fill;
//         Assert.AreEqual(1, node.Execute(args));
//         var img = node.GetImageInfo(args);
//         Assert.IsNotNull(img);
//         Assert.AreEqual(width * 2, img.Width);
//         Assert.AreEqual(height / 2, img.Height);
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_Flip()
//     {
//         var args = new NodeParameters(TestImage2, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageFlip();
//         node.Vertical = false;
//         Assert.AreEqual(1, node.Execute(args));
//     }
//     
//
//     [TestMethod]
//     public void ImageNodes_Basic_Flip_Heic()
//     {
//         var args = new NodeParameters(TestImageHeic, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageFlip();
//         node.Vertical = false;
//         Assert.AreEqual(1, node.Execute(args));
//     }
//     
//     [TestMethod]
//     public void ImageNodes_Basic_Rotate()
//     {
//         var args = new NodeParameters(TestImage2, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageRotate();
//         node.Angle = 270;
//         Assert.AreEqual(1, node.Execute(args));
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_Rotate_Heic()
//     {
//         var args = new NodeParameters(TestImageHeic, new TestLogger(), false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageRotate();
//         node.Angle = 270;
//         Assert.AreEqual(1, node.Execute(args));
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_AutoCrop_01()
//     {
//         Assert.IsTrue(System.IO.File.Exists(TestCropImage1));
//         var logger = new TestLogger();
//         var args = new NodeParameters(TestCropImage1, logger, false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new AutoCropImage();
//         node.Threshold = 30;
//         node.PreExecute(args);
//         int result = node.Execute(args);
//         
//         string log = logger.ToString();
//         Assert.AreEqual(1, result);
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_AutoCrop_02()
//     {
//         var logger = new TestLogger();
//         var args = new NodeParameters(TestCropImage2, logger, false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new AutoCropImage();
//         node.Threshold = 95;
//         int result = node.Execute(args);
//
//         string log = logger.ToString();
//         Assert.AreEqual(1, result);
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_AutoCrop_03()
//     {
//         var logger = new TestLogger();
//         var args = new NodeParameters(TestCropImage3, logger, false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new AutoCropImage();
//         node.Threshold = 30;
//         node.PreExecute(args);                
//         int result = node.Execute(args);
//
//         string log = logger.ToString();
//         Assert.AreEqual(1, result);
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_AutoCrop_04()
//     {
//         var logger = new TestLogger();
//         var args = new NodeParameters(TestCropImage4, logger, false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         //var rotate = new ImageRotate();
//         //rotate.Angle = 270;
//         //rotate.PreExecute(args);
//         //rotate.Execute(args);
//
//
//         var node = new AutoCropImage();
//         node.Threshold = 70;
//         node.PreExecute(args);
//         int result = node.Execute(args);
//
//         string log = logger.ToString();
//         Assert.AreEqual(1, result);
//     }
//
//     [TestMethod]
//     public void ImageNodes_Basic_AutoCrop_NoCrop()
//     {
//         var logger = new TestLogger();
//         var args = new NodeParameters(TestCropImageNoCrop, logger, false, string.Empty, null!)
//         {
//             TempPath = TempDir
//         };
//
//         var node = new AutoCropImage();
//         node.Threshold = 50;
//         node.PreExecute(args);
//         int result = node.Execute(args);
//
//         string log = logger.ToString();
//         Assert.AreEqual(2, result);
//     }
//     
//     
//     [TestMethod]
//     public void ImageNodes_Basic_Exif()
//     {
//         var logger = new TestLogger();
//         var args = new NodeParameters(TestExif, logger, false, string.Empty, new LocalFileService())
//         {
//             TempPath = TempDir
//         };
//
//         var node = new ImageFile();
//         var result = node.Execute(args);
//         Assert.AreEqual(1, result);
//         if(node.Variables.TryGetValue("img.DateTaken", out object? oDate) == false)
//             Assert.Fail("Failed to get date time");
//         
//         if(oDate is DateTime dt == false)
//             Assert.Fail("oDate is not a DateTime");
//     }
// }
//
// #endif