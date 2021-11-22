namespace MetaNodes.TheMovieDb
{
    using System.Text.RegularExpressions;
    using DM.MovieApi;
    using DM.MovieApi.ApiResponse;
    using DM.MovieApi.MovieDb.Movies;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class MovieRenamer : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override string Icon => "fas fa-font";

        public string _Pattern = string.Empty;

        [Text(1)]
        public string? Pattern
        {
            get => _Pattern;
            set { _Pattern = value ?? ""; }
        }

        private string _DestinationPath = string.Empty;

        [Folder(2)]
        public string DestinationPath
        {
            get => _DestinationPath;
            set { _DestinationPath = value ?? ""; }
        }

        [Boolean(3)]
        public bool LogOnly { get; set; }

        public override int Execute(NodeParameters args)
        {
            if(string.IsNullOrEmpty(Pattern))
            {
                args.Logger?.ELog("No pattern specified");
                return -1;
            }
            var movieInfo = args.GetParameter<MovieInfo>(Globals.MOVIE_INFO);
            if (movieInfo == null) {
                args.Logger?.ELog("MovieInfo not found, you must execute the Movie Lookup node first");
                return -1;
            }

            string newFile = Pattern;
            // incase they set a linux path on windows or vice versa
            newFile = newFile.Replace('\\', Path.DirectorySeparatorChar);
            newFile = newFile.Replace('/', Path.DirectorySeparatorChar);

            newFile = ReplaceVariable(newFile, "Year", movieInfo.ReleaseDate.Year.ToString());
            newFile = ReplaceVariable(newFile, "Title", movieInfo.Title);
            newFile = ReplaceVariable(newFile, "Extension", args.WorkingFile.Substring(args.WorkingFile.LastIndexOf(".")+1));
            newFile = ReplaceVariable(newFile, "Ext", args.WorkingFile.Substring(args.WorkingFile.LastIndexOf(".") + 1));

            string destFolder = DestinationPath;
            if (string.IsNullOrEmpty(destFolder))
                destFolder = new FileInfo(args.WorkingFile).Directory?.FullName ?? "";

            var dest = new FileInfo(Path.Combine(destFolder, newFile));
            
            args.Logger?.ILog("Renaming file to: " + (string.IsNullOrEmpty(DestinationPath) ? "" : DestinationPath + Path.DirectorySeparatorChar) + newFile);


            if (LogOnly)
                return 1;

            return args.MoveFile(dest.FullName) ? 1 : -1;
        }

        private string ReplaceVariable(string input, string variable, string value)
        {
            return Regex.Replace(input, @"{" + Regex.Escape(variable) + @"}", value, RegexOptions.IgnoreCase);
        }
    }
}
