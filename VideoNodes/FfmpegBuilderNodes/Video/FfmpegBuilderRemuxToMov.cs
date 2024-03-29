﻿namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToMov : FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/remux-to-mov";

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "mov";
        return 1;
    }
}