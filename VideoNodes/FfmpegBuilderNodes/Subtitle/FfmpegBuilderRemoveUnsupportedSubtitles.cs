// namespace FileFlows.VideoNodes.FfmpegBuilderNodes;
//
// public class FfmpegBuilderRemoveUnsupportedSubtitles : FfmpegBuilderNode
// {
//     public override string Icon => "fas fa-comment";
//
//     public override int Outputs => 2; 
//     
//     public readonly string[] Mp4Subtitles = new[]
//     {
//         "ass",        // Advanced SubStation Alpha
//         "aqtitle",    // AQTitle
//         "cap",        // Cheetah CAP
//         "dcin",       // D-Cinema subtitles
//         "dvb_subtitle", // DVB Teletext
//         "dvd_subtitle", // DVD subtitle
//         "es",         // Enhanced Subtitle
//         "fabsubtitler", // FAB Subtitler
//         "fcpxml",     // Final Cut Pro X
//         "jacosub",    // JACOsub subtitle
//         "microdvd",   // MicroDVD subtitle
//         "mpl2",       // MPL2 subtitle
//         "mpsub",      // MPlayer subtitle
//         "bin",        // Opaque binary subtitle (internal)
//         "pjs",        // PJS (Phoenix Japanimation Society) subtitle
//         "realtext",   // RealText subtitle format
//         "sami",       // SAMI subtitle format
//         "srt",        // SubRip subtitle
//         "ssa",        // SubStation Alpha subtitle
//         "subviewer",  // SubViewer 1.0 subtitle
//         "subviewer1", // SubViewer 2.0 subtitle
//         "teletext",   // Teletext subtitle
//         "ttml",       // Timed Text Markup Language
//         "ttxt",       // TurboTitler subtitle
//         "webvtt",     // WebVTT subtitle
//         "zerog"       // ZeroG subtitle
//     };
//     
//     public readonly string[]  MkvSubtitles = new[]
//     {
//         "ass",        // Advanced SubStation Alpha
//         "ssa",        // SubStation Alpha subtitle
//         "srt",        // SubRip subtitle
//         "subrip",     // SubRip subtitle (alternative name)
//         "vtt",        // WebVTT subtitle
//         "webvtt",     // WebVTT subtitle (alternative name)
//         "smi",        // SAMI subtitle format
//         "sami",       // SAMI subtitle format (alternative name)
//         "rt",         // RealText subtitle format
//         "realtext",   // RealText subtitle format (alternative name)
//         "stl",        // EBU STL (Subtitling Data Exchange Format)
//         "ttml",       // Timed Text Markup Language
//         "ttml_legacy" // Timed Text Markup Language (legacy name)
//     };
//     
//     public readonly string[] WebMSubtitles = new[]
//     {
//         "ass",        // Advanced SubStation Alpha
//         "ssa",        // SubStation Alpha subtitle
//         "srt",        // SubRip subtitle
//         "subrip",     // SubRip subtitle (alternative name)
//         "vtt",        // WebVTT subtitle
//         "webvtt",     // WebVTT subtitle (alternative name)
//         "ttml",       // Timed Text Markup Language
//         "ttml_legacy" // Timed Text Markup Language (legacy name)
//     };
//
//
//
//
//     public override int Execute(NodeParameters args)
//     {
//         this.Init(args);
//         bool removing = false;
//         string[] unsupported = new[] { "" };
//         foreach (var stream in Model.SubtitleStreams)
//         {
//             if (unsupported.Contains(stream.Stream.Codec?.ToLower()))
//             {
//                 stream.Deleted = true;
//                 removing = true;
//             }
//         }
//         return removing ? 1 : 2;
//     }
// }
