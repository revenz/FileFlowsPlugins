namespace FileFlows.VideoNodes;

/// <summary>
/// Tests if a videos bitrate is greater than a given amount
/// </summary>
public class VideoBitrateGreaterThan: VideoNode
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets the help URL 
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-bitrate-greather-than";
    /// <summary>
    /// Gets the icon for this flow element
    /// </summary>
    public override string Icon => "fas fa-video";

    /// <summary>
    /// Gets or sets the bitrate in KBps to test
    /// </summary>
    [NumberInt(1)]
    [Range(1, 10_000_000)]
    [DefaultValue(5_000)]
    public int Bitrate { get; set; }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        VideoInfo videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
        {
            args.Logger?.ELog($"No video info found, run the Video File flow element first.");
            return -1;
        }

        // get the first video stream, likely the only one
        var video = videoInfo.VideoStreams.FirstOrDefault();
        if (video == null)
        {
            args.Logger?.ELog($"No video stream detected");
            return -1; // no video streams detected
        }
        
        // get the video stream
        var bitrate = video.Bitrate;
 
        if(bitrate < 1)
        {
            // video stream doesn't have bitrate information
            // need to use the overall bitrate
            var overall = videoInfo.Bitrate;
            if (overall < 1)
            {
                args.Logger.WLog("No bitrate information found in the video");
                return 0; // couldn't get overall bitrate either
            }

            // overall bitrate includes all audio streams, so we try and subtract those
            double calculated = overall;
            if(videoInfo.AudioStreams?.Any() == true) // check there are audio streams
            {
                foreach(var audio in videoInfo.AudioStreams)
                {
                    if(audio.Bitrate > 0)
                        calculated -= audio.Bitrate;
                    else
                    {
                        // audio doesn't have bitrate either, so we just subtract 5% of the original bitrate
                        // this is a guess, but it should get us close
                        calculated -= (overall * 0.05);
                    }
                }
            }
            bitrate = (int)calculated;
            args.Logger?.ILog($"Estimated Video Bitrate: {bitrate} BPS / {bitrate / 1000} KBps");
        }
 
        // check if the bitrate is over the maximum bitrate
        if ((Bitrate * 1000) > bitrate)
        {
            args.Logger?.ILog($"Bitrate '{Bitrate} KBps' is greater than '{bitrate / 1000} KBps'");
            return 1; // it is, so call output 1
        }

        args.Logger?.ILog($"Bitrate '{Bitrate} KBps' is not greater than '{bitrate / 1000} KBps'");
        return 2; // it isn't so call output 2
    }
}