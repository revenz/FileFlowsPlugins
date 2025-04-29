namespace FileFlows.AudioNodes;

public class AudioFile : AudioNode
{
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Input;
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/audio-file";

    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    public AudioFile()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "mi.Album", "Album" },
            { "mi.Artist", "Artist" },
            { "mi.ArtistThe", "Artist, The" },
            { "mi.Bitrate", 845 },
            { "mi.Channels", 2 },
            { "mi.Codec", "flac" },
            { "mi.Date", new DateTime(2020, 05, 23) },
            { "mi.Year", 2020 },
            { "mi.Duration", 256 },
            { "mi.Encoder", "FLAC 1.2.1" },
            { "mi.Frequency", 44100 },
            { "mi.Genres", new  [] { "Pop", "Rock" } },
            { "mi.Language", "English" },
            { "mi.Title", "Song Title" },
            { "mi.Track", 2 },
            { "mi.Disc", 2 },
            { "mi.TotalDiscs", 2 }
    };
    }

    public override int Execute(NodeParameters args)
    {
        // this info is read in the base class in pre-execute, so if here, its fine.
        return 1;
        // var ffmpegExeResult = GetFFmpeg(args);
        // if (ffmpegExeResult.Failed(out string ffmpegError))
        // {
        //     args.FailureReason = ffmpegError;
        //     args.Logger?.ELog(ffmpegError);
        //     return -1;
        // }
        // string ffmpegExe = ffmpegExeResult.Value;
        //
        // var ffprobeResult = GetFFprobe(args);
        // if (ffprobeResult.Failed(out string ffprobeError))
        // {
        //     args.FailureReason = ffprobeError;
        //     args.Logger?.ELog(ffprobeError);
        //     return -1;
        // }
        // string ffprobe = ffprobeResult.Value;
        //
        //
        // if (args.FileService.FileCreationTimeUtc(args.WorkingFile).Success(out DateTime createTime))
        //     args.Variables["ORIGINAL_CREATE_UTC"] = createTime;
        // if (args.FileService.FileLastWriteTimeUtc(args.WorkingFile).Success(out DateTime writeTime))
        //     args.Variables["ORIGINAL_LAST_WRITE_UTC"] = writeTime;
        //
        //
        // try
        // {
        //     if (ReadAudioFileInfo(args, ffmpegExe, ffprobe, LocalWorkingFile))
        //         return 1;
        //     return -1;
        // }
        // catch (Exception ex)
        // {
        //     args.Logger.ELog("Failed processing AudioFile: " + ex.Message);
        //     args.FailureReason = "Failed processing AudioFile: " + ex.Message;
        //     return -1;
        // }
    }
}

