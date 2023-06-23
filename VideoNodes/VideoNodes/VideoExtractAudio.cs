namespace FileFlows.VideoNodes;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

public class VideoExtractAudio : AudioSelectionEncodingNode
{
    public override int Outputs => 2;

    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/video-extract-audio";

    public override string Icon => "fas fa-file-audio";

    [File(1)]
    public string OutputFile { get; set; }

    [Boolean(2)]
    public bool SetWorkingFile { get; set; }
    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;


    [DefaultValue("mp3")]
    [Select(nameof(CodecOptions), 3)]
    public string OutputCodec { get; set; }

    private static List<ListOption> _CodecOptions;
    public static List<ListOption> CodecOptions
    {
        get
        {
            if (_CodecOptions == null)
            {
                _CodecOptions = new List<ListOption>
                {
                    new ListOption { Label = "AAC", Value = "aac"},
                    new ListOption { Label = "AC3", Value = "ac3"},
                    new ListOption { Label = "EAC3", Value = "eac3" },
                    new ListOption { Label = "MP3", Value = "mp3"},
                };
            }
            return _CodecOptions;
        }
    }

    [Select(nameof(BitrateOptions), 4)]
    public int OutputBitrate { get; set; }


    public VideoExtractAudio()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "ExtractedAudioFile", "/path/to/audio.mp3" }
        };
    }

    public override int Execute(NodeParameters args)
    {
        try
        {
            VideoInfo videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return -1;

            var track = GetTrack(videoInfo);
            if (track == null)
            {
                args.Logger.WLog("Unable to find matching audio track to extract");
                return 2;
            }

            string outputFile = GetOutputFile(args);
            var parameters = GetAudioTrackParameters(track);

            var extracted = ExtractAudio(args, FFMPEG, parameters, outputFile);
            if(extracted)
            {
                args.UpdateVariables(new Dictionary<string, object>
                {
                    { "ExtractedAudioFile", outputFile }
                });
                if (SetWorkingFile)
                    args.SetWorkingFile(OutputFile, dontDelete: true);

                return 1;
            }

            return 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed extracted audio track: " + ex.Message);
            return 2;
        }
    }

    private string GetOutputFile(NodeParameters args)
    {
        string outputfile = OutputFile;
        if (string.IsNullOrEmpty(outputfile) == false)
        {
            outputfile = args.ReplaceVariables(outputfile, true);
        }
        else
        {
            var file = new FileInfo(args.FileName);
            outputfile = file.FullName.Substring(0, file.FullName.LastIndexOf(file.Extension));
        }
        outputfile = args.MapPath(outputfile);

        if (string.IsNullOrWhiteSpace(OutputCodec))
            OutputCodec = "mp3";

        if (Regex.IsMatch(outputfile, @"\.[a-zA-Z0-9]{3,6}$") == false)
        {
            outputfile += "." + OutputCodec;
        }
        return outputfile;
    }

    internal string[] GetAudioTrackParameters(AudioStream source)
    {
        if (OutputBitrate == 0)
        {
            return new[]
            {
                "-map", $"0:a:{source.TypeIndex}",
                "-c:a:0",
                OutputCodec
            };
        }
        return new[]
        {
            "-map", $"0:a:{source.TypeIndex}",
            "-c:a:0",
            OutputCodec,
            "-b:a:0", OutputBitrate + "k"
        };
    }

    internal bool ExtractAudio(NodeParameters args, string ffmpegExe, string[] parameters, string output)
    {
        if (File.Exists(output))
        {
            args.Logger?.ILog("File already exists, deleting file: " + output);
            File.Delete(output);
        }
        var argList = new [] { "-i", args.WorkingFile }.Union(parameters).Union(new [] { output }).ToArray();

        // -y means it will overwrite a file if output already exists
        var result = args.Process.ExecuteShellCommand(new ExecuteArgs
        {
            Command = ffmpegExe,
            ArgumentList = argList
        }).Result;

        var of = new FileInfo(output);
        if (result.ExitCode != 0)
        {
            args.Logger?.ELog("FFMPEG process failed to extract audio track");
            args.Logger?.ILog("Unexpected exit code: " + result.ExitCode);
            args.Logger?.ILog(result.StandardOutput ?? String.Empty);
            args.Logger?.ILog(result.StandardError ?? String.Empty);
            if (of.Exists && of.Length == 0)
            {
                // delete the output file if it created an empty file
                try
                {
                    of.Delete();
                }
                catch (Exception) { }
            }
            return false;
        }
        return of.Exists;
    }
}
