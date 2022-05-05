#if(DEBUG)

using FileFlows.ImageNodes.Images;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFlows.ImageNodes.Tests;

[TestClass]
public class ImageNodesTests
{
    [TestMethod]
    public void ImageNodes_Basic_ImageFormat()
    {
        var args = new NodeParameters("/home/john/Pictures/fileflows.png", new TestLogger(), false, string.Empty)
        {
            TempPath = "/home/john/src/temp/"
        };

        var node = new ImageFormat();
        node.Format = IMAGE_FORMAT_GIF;
        Assert.AreEqual(1, node.Execute(args));
    }
    
    [TestMethod]
    public void ImageNodes_Basic_Resize()
    {
        var args = new NodeParameters("/home/john/Pictures/fileflows.png", new TestLogger(), false, string.Empty)
        {
            TempPath = "/home/john/src/temp/"
        };

        var node = new ImageResizer();
        node.Width = 1000;
        node.Height = 500;
        node.Mode = ResizeMode.Fill;
        Assert.AreEqual(1, node.Execute(args));
    }
    
    [TestMethod]
    public void ImageNodes_Basic_Flip()
    {
        var args = new NodeParameters("/home/john/Pictures/36410427.png", new TestLogger(), false, string.Empty)
        {
            TempPath = "/home/john/src/temp/"
        };

        var node = new ImageFlip();
        node.Vertical = false;
        Assert.AreEqual(1, node.Execute(args));
    }
    
    
    [TestMethod]
    public void ImageNodes_Basic_Rotate()
    {
        var args = new NodeParameters("/home/john/Pictures/36410427.png", new TestLogger(), false, string.Empty)
        {
            TempPath = "/home/john/src/temp/"
        };

        var node = new ImageRotate();
        node.Angle = 270;
        Assert.AreEqual(1, node.Execute(args));
    }
}

#endif