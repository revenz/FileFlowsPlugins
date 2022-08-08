#if(DEBUG)

using FileFlows.ImageNodes.Images;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace FileFlows.ImageNodes.Tests;

[TestClass]
public class ImageNodesTests
{
    string TestImage1;
    string TestImage2;
    string TempDir;
    string TestCropImage1, TestCropImage2, TestCropImage3, TestCropImageNoCrop;

    public ImageNodesTests()
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows)
        {
            TestImage1 = @"D:\videos\pictures\image1.jpg";
            TestImage2 = @"D:\videos\pictures\image2.png";
            TempDir = @"D:\videos\temp";
            TestCropImage1 = @"D:\images\testimages\crop01.jpg";
            TestCropImage2 = @"D:\images\testimages\crop02.jpg";
            TestCropImage3 = @"D:\images\testimages\crop03.jpg";
            TestCropImageNoCrop = @"D:\images\testimages\nocrop.jpg";
        }
        else
        {
            TestImage1 = "/home/john/Pictures/fileflows.png";
            TestImage2 = "/home/john/Pictures/36410427.png";
            TempDir = "/home/john/src/temp/";
        }
    }

    [TestMethod]
    public void ImageNodes_Basic_ImageFormat()
    {
        var args = new NodeParameters(TestImage1, new TestLogger(), false, string.Empty)
        {
            TempPath = TempDir
        };

        var node = new ImageFormat();
        node.Format = IMAGE_FORMAT_GIF;
        Assert.AreEqual(1, node.Execute(args));
    }

    [TestMethod]
    public void ImageNodes_Basic_Resize()
    {
        var args = new NodeParameters(TestImage1, new TestLogger(), false, string.Empty)
        {
            TempPath = TempDir
        };

        var node = new ImageResizer();
        node.Width = 1000;
        node.Height = 500;
        node.Mode = ResizeMode.Fill;
        Assert.AreEqual(1, node.Execute(args));
    }

    [TestMethod]
    public void ImageNodes_Basic_Resize_Percent()
    {
        var args = new NodeParameters(TestImage1, new TestLogger(), false, string.Empty)
        {
            TempPath = TempDir
        };

        var imgFile = new ImageFile();
        imgFile.Execute(args);
        int width = imgFile.GetImageInfo(args).Width;
        int height = imgFile.GetImageInfo(args).Height;

        var node = new ImageResizer();
        node.Width = 200;
        node.Height = 50;
        node.Percent = true;
        node.Mode = ResizeMode.Fill;
        Assert.AreEqual(1, node.Execute(args));
        var img = node.GetImageInfo(args);
        Assert.IsNotNull(img);
        Assert.AreEqual(width * 2, img.Width);
        Assert.AreEqual(height / 2, img.Height);
    }

    [TestMethod]
    public void ImageNodes_Basic_Flip()
    {
        var args = new NodeParameters(TestImage2, new TestLogger(), false, string.Empty)
        {
            TempPath = TempDir
        };

        var node = new ImageFlip();
        node.Vertical = false;
        Assert.AreEqual(1, node.Execute(args));
    }
    
    
    [TestMethod]
    public void ImageNodes_Basic_Rotate()
    {
        var args = new NodeParameters(TestImage2, new TestLogger(), false, string.Empty)
        {
            TempPath = TempDir
        };

        var node = new ImageRotate();
        node.Angle = 270;
        Assert.AreEqual(1, node.Execute(args));
    }


    [TestMethod]
    public void ImageNodes_Basic_AutoCrop_01()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(TestCropImage1, logger, false, string.Empty)
        {
            TempPath = TempDir
        };

        var node = new AutoCropImage();
        int result = node.Execute(args);
        
        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ImageNodes_Basic_AutoCrop_02()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(TestCropImage2, logger, false, string.Empty)
        {
            TempPath = TempDir
        };

        var node = new AutoCropImage();
        node.Threshold = 95;
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ImageNodes_Basic_AutoCrop_03()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(TestCropImage3, logger, false, string.Empty)
        {
            TempPath = TempDir
        };

        var node = new AutoCropImage();
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ImageNodes_Basic_AutoCrop_NoCrop()
    {
        var logger = new TestLogger();
        var args = new NodeParameters(TestCropImageNoCrop, logger, false, string.Empty)
        {
            TempPath = TempDir
        };

        var node = new AutoCropImage();
        int result = node.Execute(args);

        string log = logger.ToString();
        Assert.AreEqual(2, result);
    }
}

#endif