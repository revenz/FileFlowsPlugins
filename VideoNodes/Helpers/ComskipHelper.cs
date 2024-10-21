using System.IO;

namespace FileFlows.VideoNodes.Helpers;

/// <summary>
/// Helper class for comskip
/// </summary>
public class ComskipHelper
{
    /// <summary>
    /// Runs comksip against a video file and creates a EDL file
    /// </summary>
    /// <param name="args">the NodeParameters</param>
    /// <param name="file">the video file to run comskip against</param>
    /// <returns>the ELD filename</returns>
    public static Result<string> RunComskip(NodeParameters args, string file)
    {
        if (System.IO.File.Exists(file) == false)
            return Result<string>.Fail("File does not exist");
        try
        {
            var csIni = GetComskipIniFile(args, file);
            
            var comskip = args.GetToolPath("comskip")?.EmptyAsNull() ?? (OperatingSystem.IsWindows()
                ? "comskip.exe"
                : "comskip");
            
            
            string edl = FileHelper.ChangeExtension(file, "txt");

            var result = args.Execute(new()
            {
                Command = comskip,
                ArgumentList = new[]
                {
                    "--ini=" + csIni,
                    file
                }
            });

            if (File.Exists(edl) == false)
                return Result<string>.Fail("Failed to create EDL file");

            string edlContent = File.ReadAllText(edl);
            args.Logger?.ILog(new string('-', 30) + "\n" + edlContent + new string('-', 30));
            return edl;
        }
        catch (Exception ex)
        {
            return Result<string>.Fail("Failed running comskip: " + ex.Message);
        }
    }


    internal static string GetComskipIniFile(NodeParameters args, string file)
    {
        var csIniFile =
            args.Variables.FirstOrDefault(x => x.Key.Equals("comskipini", StringComparison.InvariantCultureIgnoreCase))
                .Value?.ToString();
        
        var csIni = csIniFile?.EmptyAsNull() ?? args.GetToolPath("comskip.ini");
        if (string.IsNullOrWhiteSpace(csIni) == false)
        {
            if (csIni.IndexOf('\n') > 0)
            {
                // csini is the contents of the csini, make a file
                args.Logger?.ILog("Using comskip.ini variable contents");
                var tempFile = FileHelper.Combine(args.TempPath, "comskip.ini");
                File.WriteAllText(tempFile, csIni);
                return tempFile;
            }
            args.Logger?.ILog("Using comskip.ini file variable");
            return csIni;
        }

        var path = FileHelper.Combine(FileHelper.GetDirectory(file), "comskip.ini");
        if (File.Exists(path))
        {
            args.Logger?.ILog("Using comskip.ini found with input file.");
            return path;
        }

        args.Logger?.ILog("Using default comskip.ini file");
        csIni = FileHelper.Combine(args.TempPath, "comskip.ini");
        
        // create the default ini file
        File.WriteAllText(csIni, @"detect_method=111			;1=black frame, 2=logo, 4=scene change, 8=fuzzy logic, 16=closed captions, 32=aspect ration, 64=silence, 128=cutscenes, 255=all
validate_silence=1			; Default, set to 0 to force using this clues if selected above.
validate_uniform=1			; Default, set to 0 to force using this clues (like pure white frames) if blackframe is selected above.
validate_scenechange=1		; Default, set to 0 to force using this clues if selected above.
verbose=10				;show a lot of extra info, level 5 is also OK, set to 0 to disable
max_brightness=60      			;frame not black if any pixels checked are greater than this (scale 0 to 255)
test_brightness=40      		;frame not pure black if any pixels checked are greater than this, will check average brightness (scale 0 to 255)
max_avg_brightness=25			;maximum average brightness for a dim frame to be considered black (scale 0 to 255) 0 means autosetting
max_commercialbreak=600 		;maximum length in seconds to consider a segment a commercial break
min_commercialbreak=25			;minimum length in seconds to consider a segment a commercial break
max_commercial_size=125			;maximum time in seconds for a single commercial or multiple commercials if no breaks in between
min_commercial_size=4   		;mimimum time in seconds for a single commercial
min_show_segment_length=125 	; any segment longer than this will be scored towards show.
non_uniformity=500			; Set to 0 to disable cutpoints based on uniform frames
max_volume=500				; any frame with sound volume larger than this will not be regarded as black frame
min_silence=12				; Any deep silence longer than this amount  of frames is a possible cutpoint
ticker_tape=0				; Amount of pixels from bottom to ignore in all processing 
logo_at_bottom=0			; Set to 1 to search only for logo at the lower half of the video, do not combine with subtitle setting
punish=0					; Compare to average for sum of 1=brightness, 2=uniform 4=volume, 8=silence, 16=schange, set to 0 to disable
punish_threshold=1.3		; Multiply when amount is above average * punish_threshold
punish_modifier=2			; When above average * threshold multiply score by this value
intelligent_brightness=0 		; Set to 1 to use a USA specific algorithm to tune some of the settings, not adviced outside the USA
logo_percentile=0.92			; if more then this amount of logo is found then logo detection will be disabled
logo_threshold=0.75
punish_no_logo=1			; Default, set to 0 to avoid show segments without logo to be scored towards commercial
aggressive_logo_rejection=0
connect_blocks_with_logo=1		; set to 1 if you want successive blocks with logo on the transition to be regarded as connected, set to 0 to disable
logo_filter=0               ; set the size of the filter to apply to bad logo detection, 4 seems to be a good value.
cut_on_ar_change=1			; set to 1 if you want to cut also on aspect ratio changes when logo is present, set to 2 to force cuts on aspect ratio changes. set to 0 to disable
delete_show_after_last_commercial=0	; set to 1 if you want to delete the last block if its a show and after a commercial
delete_show_before_or_after_current=0	; set to 1 if you want to delete the previous and the next show in the recording, this can lead to the deletion of trailers of next show
delete_block_after_commercial=0	;set to max size of block in seconds to be discarded, set to 0 to disable 
remove_before=0				; amount of seconds of show to be removed before ALL commercials
remove_after=0				; amount of seconds of show to be removed after ALL commercials
shrink_logo=5				; Reduce the duration of the logo with this amount of seconds
after_logo=0		; set to number of seconds after logo disappears comskip should start to search for silence to insert an additional cutpoint
padding=0
ms_audio_delay=5
volume_slip=20
max_repair_size=200			; Will repair maximum 200 missing MPEG frames in the timeline, set to 0 to disable repairing for players that don't use PTS. 
disable_heuristics=4		bit pattern for disabling heuristics, adding 1 disables heristics 1, adding 2 disables heristics 2, adding 4 disables heristics 3, 255  disables all heuristics 
delete_logo_file=0			; set to 1 if you want comskip to tidy up after finishing
output_framearray=0			; create a big excel file for detailed analysis, set to 0 to disable
output_videoredo=0
output_womble=0
output_mls=0			; set to 1 if you want MPeg Video Wizard bookmark file output
output_cuttermaran=0
output_mpeg2schnitt=0
output_mpgtx=0
output_dvrcut=0
output_zoomplayer_chapter=0
output_zoomplayer_cutlist=0
output_edl=1
output_edlx=0
output_vcf=0
output_bsplayer=0
output_btv=0				; set to 1 if you want Beyond TV chapter cutlist output
output_projectx=0			; set to 1 if you want ProjectX cutlist output (Xcl)
output_avisynth=0
output_vdr=0				; set to 1 if you want XBMC to skipping commercials
output_demux=0				; set to 1 if you want comskip to demux the mpeg file while scanning
sage_framenumber_bug=0
sage_minute_bug=0
live_tv=0					; set to 1 if you use parallelprocessing and need the output while recording
live_tv_retries=4			; change to 16 when using live_tv in BTV, used for mpeg PS and TS
dvrms_live_tv_retries=300			; only used for dvr_ms
standoff=0					; change to 8000000 when using live_tv in BTV
cuttermaran_options=""cut=\""true\"" unattended=\""true\"" muxResult=\""false\"" snapToCutPoints=\""true\"" closeApp=\""true\""""
mpeg2schnitt_options=""mpeg2schnitt.exe /S /E /R25  /Z %2 %1""
avisynth_options=""LoadPlugin(\""MPEG2Dec3.dll\"") \nMPEG2Source(\""%s\"")\n""
dvrcut_options=""dvrcut \""%s.dvr-ms\"" \""%s_clean.dvr-ms\"" ""
windowtitle=""Comskip - %s""");
        return csIni;
    }
}