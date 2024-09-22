using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoNodes.Tests;

namespace FileFlows.VideoNodes.Tests.FfmpegBuilderTests;

/// <summary>
/// Tests for FFmpeg Builder: Aspect Ratio
/// </summary>
[TestClass]
[TestCategory("Slow")]
public class FFmpegBuilder_AspectRatioTests : VideoTestBase
{
    NodeParameters args;

    /// <summary>
    /// Sets up the test environment before each test.
    /// Initializes video parameters and executes the video file setup.
    /// </summary>
    private  void InitVideo(string file)
    {
        args = GetVideoNodeParameters(file);
        VideoFile vf = new VideoFile();
        vf.PreExecute(args);
        vf.Execute(args);

        FfmpegBuilderStart ffStart = new();
        ffStart.PreExecute(args);
        Assert.AreEqual(1, ffStart.Execute(args));
    }

    /// <summary>
    /// Checks if the video aspect ratio matches the provided width and height ratio.
    /// Logs the results and asserts conditions.
    /// </summary>
    /// <param name="expectedWidth">The expected width ratio.</param>
    /// <param name="expectedHeight">The expected height ratio.</param>
    private void CheckAspectRatio(int expectedWidth, int expectedHeight)
    {
        var videoInfo = (VideoInfo)args.Parameters[VideoNode.VIDEO_INFO];
        Assert.IsNotNull(videoInfo, "Video info is null.");

        var videoStream = videoInfo.VideoStreams[0];
        int width = videoStream.Width;
        int height = videoStream.Height;

        double actualAspectRatio = (double)width / height;
        double expectedAspectRatio = (double)expectedWidth / expectedHeight;
        Logger.ILog($"Current dimensions: {width}x{height}");

        Logger.ILog($"Current aspect ratio: {actualAspectRatio:F4}, Expected: {expectedAspectRatio:F4}");
        Assert.AreEqual(expectedAspectRatio, actualAspectRatio, 0.01, "Aspect ratio does not match the expected value.");
    }

    /// <summary>
    /// Executes the aspect ratio test with the specified parameters.
    /// </summary>
    /// <param name="aspectRatio">The desired aspect ratio (e.g., "4:3", "16:9").</param>
    /// <param name="adjustmentMode">The adjustment mode to use (e.g., "Crop").</param>
    /// <param name="expectedWidth">The expected width ratio.</param>
    /// <param name="expectedHeight">The expected height ratio.</param>
    /// <param name="file">The video file to test.</param>
    private void ExecuteAspectRatioTest(string aspectRatio, string adjustmentMode, int expectedWidth, int expectedHeight, string? file = null)
    {
        file ??= Video4by7;
        InitVideo(file);
        
        var ffAspectRatio = new FfmpegBuilderAspectRatio
        {
            AspectRatio = aspectRatio,
            AdjustmentMode = adjustmentMode
        };

        if (aspectRatio == "Custom")
        {
            ffAspectRatio.CustomWidth = expectedWidth; // Set custom width
            ffAspectRatio.CustomHeight = expectedHeight; // Set custom height
        }

        ffAspectRatio.PreExecute(args);
        ffAspectRatio.Execute(args);

        var ffExecutor = new FfmpegBuilderExecutor();
        ffExecutor.PreExecute(args);
        int result = ffExecutor.Execute(args);

        Assert.AreEqual(1, result);
        CheckAspectRatio(expectedWidth, expectedHeight);
    }

    /// <summary>
    /// Test to verify the video scaler with a 4:3 aspect ratio using cropping.
    /// </summary>
    [TestMethod]
    public void FfmpegBuilder_AspectRatio_4by3_Crop()
    {
        ExecuteAspectRatioTest("4:3", "Crop", 4, 3);
    }

    /// <summary>
    /// Test to verify the video scaler with a 16:9 aspect ratio using cropping.
    /// </summary>
    [TestMethod]
    public void FfmpegBuilder_AspectRatio_16by9_Crop()
    {
        ExecuteAspectRatioTest("16:9", "Crop", 16, 9);
    }

    /// <summary>
    /// Test to verify the video scaler with a 16:9 aspect ratio using stretching.
    /// </summary>
    [TestMethod]
    public void FfmpegBuilder_AspectRatio_16by9_Stretch()
    {
        ExecuteAspectRatioTest("16:9", "Stretch", 16, 9, file: Video4by3);
    }

    /// <summary>
    /// Test to verify the video scaler with a 16:9 aspect ratio using black bars.
    /// </summary>
    [TestMethod]
    public void FfmpegBuilder_AspectRatio_16by9_AddBlackBars()
    {
        ExecuteAspectRatioTest("16:9", "AddBlackBars", 16, 9, file: Video4by3);
    }

    /// <summary>
    /// Test to verify the video scaler with a 21:9 aspect ratio using cropping.
    /// </summary>
    [TestMethod]
    public void FfmpegBuilder_AspectRatio_21by9_Crop()
    {
        ExecuteAspectRatioTest("21:9", "Crop", 21, 9);
    }

    /// <summary>
    /// Test to verify the video scaler with a custom aspect ratio using cropping.
    /// Modify as needed to fit the custom requirements.
    /// </summary>
    [TestMethod]
    public void FfmpegBuilder_AspectRatio_CustomAspectRatio_Crop()
    {
        ExecuteAspectRatioTest("Custom", "Crop", 4, 5);
    }
}