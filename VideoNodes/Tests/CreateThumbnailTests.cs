#if(DEBUG)

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileFlows.VideoNodes;
using Moq;

namespace VideoNodes.Tests;

/// <summary>
/// Thumbnail tests
/// </summary>
[TestClass]
public class CreateThumbnailTests : VideoTestBase
{
    private Mock<IImageHelper> _imageHelper;
    private int ImageDarkness = 50;

    /// <inheritdoc />
    protected override void TestStarting()
    {
        base.TestStarting();
        _imageHelper = new Mock<IImageHelper>();
        _imageHelper.Setup(x => x.CalculateImageDarkness(It.IsAny<string>())).Returns(() => ImageDarkness);
    }

    /// <summary>
    /// Tests creating a basic thumbnail
    /// </summary>
    [TestMethod]
    public void BasicThumbnail()
    {
        var args = GetVideoNodeParameters(VideoMkv);
        args.ImageHelper = _imageHelper.Object;

        VideoFile vf = new();
        vf.PreExecute(args);
        vf.Execute(args);

        CreateThumbnail element = new();
        element.PreExecute(args);
        element.Width = 320;
        element.Height = 240;
        element.SkipBlackFrames = false;
        int output = element.Execute(args);

        Assert.AreEqual(1, output);
    }

}

#endif