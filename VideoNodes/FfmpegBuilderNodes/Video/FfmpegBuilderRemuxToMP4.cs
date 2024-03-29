﻿namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToMP4: FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/remux-to-mp4";

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "mp4";
        return 1;
    }
}
