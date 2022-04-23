//namespace FileFlows.VideoNodes.FfmpegBuilderNodes
//{
//    public class FfmpegBuilderVideoMetadata : FfmpegBuilderNode
//    {
//        public override int Outputs => 2;

//        public override int Execute(NodeParameters args)
//        {
//            base.Init(args);

//            if (args?.Variables?.ContainsKey("VideoMetadata") != true)
//            {
//                args.Logger?.ILog("VideoMetadata not found in variables");
//                return 2;
//            }
//            var md = VideoMetadata.Convert(args.Variables["VideoMetadata"]);
//            if (string.IsNullOrEmpty(md?.Title))
//            {
//                args.Logger?.ILog("Failed to load VideoMetadata");
//                return 2;
//            }

//            if (md.Year > 1920 && md.Year < 2100)
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "year=" + md.Year });
//            if (md.ReleaseDate > new DateTime(1920, 1, 1) && md.ReleaseDate < new DateTime(2100, 1, 1))
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "date=" + md.ReleaseDate.ToString("yyyy-MM-dd HH:mm:ss") });

//            if (string.IsNullOrEmpty(md.Description) == false)
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "description=" + md.Description });

//            if (string.IsNullOrEmpty(md?.ArtJpeg) == false)
//                Model.MetadataParameters.AddRange(new[] { "-attach", md.ArtJpeg, "-metadata:s:t:0", "mimetype=image/jpg", "-metadata:s:t:0", "filename=cover.jpg" });

//            if (md.Genres?.Any() == true)
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "genre=" + string.Join(", ", md.Genres) });
//            if (md.Directors?.Any() == true)
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "directors=" + string.Join(", ", md.Directors) });
//            if (md.Writers?.Any() == true)
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "writers=" + string.Join(", ", md.Writers) });
//            if (md.Actors?.Any() == true)
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "cast=" + string.Join(", ", md.Actors) });

//            if (md.Season != null && md.Season >= 0 && md.Episode != null && md.Episode > 0) 
//            {
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "show=" + md.Title });
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "season_number=" + md.Season });
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "episode_id=" + md.Episode });
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "title=" + md.Subtitle});
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "album=Season " + md.Season });
//            }
//            else
//            {
//                // movie
//                Model.MetadataParameters.AddRange(new[] { "-metadata", "title=" + md.Title });

//            }
//            Model.MetadataParameters.AddRange(new[] { "-metadata", "encoder=FileFlows" });

//            //Model.MetadataParameters.Add("");
//            //Model.MetadataParameters.AddRange(new[] { "-map_metadata", (Model.InputFiles.Count - 1).ToString() });
//            return 1;
//        }
//    }
//}
