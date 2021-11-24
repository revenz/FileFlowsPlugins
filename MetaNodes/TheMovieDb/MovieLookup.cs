namespace MetaNodes.TheMovieDb
{
    using System.Text.RegularExpressions;
    using DM.MovieApi;
    using DM.MovieApi.ApiResponse;
    using DM.MovieApi.MovieDb.Movies;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class MovieLookup : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 2;

        public override FlowElementType Type => FlowElementType.Logic;

        public override string Icon => "fas fa-film";

        private Dictionary<string, object> _Variables;

        public override Dictionary<string, object> Variables => _Variables;

        public MovieLookup()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "miTitle", "Batman Begins" },
                { "miYear", 2005 }
            };
        }


        [Boolean(1)]
        public bool UseFolderName { get; set; }

        public override int Execute(NodeParameters args)
        {
            var fileInfo = new FileInfo(args.FileName);
            string lookupName = UseFolderName ? fileInfo.Directory.Name : fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(fileInfo.Extension));
            lookupName = lookupName.Replace(".", " ").Replace("_", " ");

            // look for year
            string year = string.Empty;
            var match = Regex.Match(lookupName, @"((19[2-9][0-9])|(20[0-9]{2}))(?=([\.\s_\-\)\]]|$))");
            if (match.Success)
            {
                year = match.Groups[1].Value;
                lookupName = lookupName.Replace(year, "");
            }

            // remove double spaces incase they were added when removing the year
            while (lookupName.IndexOf("  ") > 0)
                lookupName = lookupName.Replace("  ", " ");

            string bearerToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIxZjVlNTAyNmJkMDM4YmZjZmU2MjI2MWU2ZGEwNjM0ZiIsInN1YiI6IjRiYzg4OTJjMDE3YTNjMGY5MjAwMDIyZCIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.yMwyT8DEK1rF1gQMKJ-ZSy-dUGxFs5T345XwBLrvrWE";

            // RegisterSettings only needs to be called one time when your application starts-up.
            MovieDbFactory.RegisterSettings(bearerToken);

            var movieApi = MovieDbFactory.Create<IApiMovieRequest>().Value;


            ApiSearchResponse<MovieInfo> response = movieApi.SearchByTitleAsync(lookupName).Result;

            // try find an exact match
            var result = response.Results.OrderBy(x =>
                {
                    if (string.IsNullOrEmpty(year) == false)
                    {
                        return year == x.ReleaseDate.Year.ToString() ? 0 : 1;
                    }
                    return 0;
                })
                .ThenBy(x => x.Title.ToLower().Trim().Replace(" ", "") == lookupName.ToLower().Trim().Replace(" ", "") ? 0 : 1)
                .ThenBy(x =>
                {
                    // do some fuzzy logic with roman numerals
                    var numMatch = Regex.Match(lookupName, @"[\s]([\d]+)$");
                    if (numMatch.Success == false)
                        return 0;
                    int number = int.Parse(numMatch.Groups[1].Value);
                    string roman = number switch
                    {
                        1 => "i",
                        2 => "ii",
                        3 => "iii",
                        4 => "iv",
                        5 => "v",
                        6 => "vi,",
                        7 => "vii",
                        8 => "viii",
                        9 => "ix",
                        10 => "x",
                        11 => "xi",
                        12 => "xii",
                        13 => "xiii",
                        _ => string.Empty
                    };
                    string ln = lookupName.Substring(0, lookupName.LastIndexOf(number.ToString())).ToLower().Trim().Replace(" ", "");
                    string softTitle = x.Title.ToLower().Replace(" ", "").Trim();
                    if (softTitle == ln + roman)
                        return 0;
                    if (softTitle.StartsWith(ln) && softTitle.EndsWith(roman))
                        return 0;
                    return 1;
                 })
                .ThenBy(x => lookupName.ToLower().Trim().Replace(" ", "").StartsWith(x.Title.ToLower().Trim().Replace(" ", "")) ? 0 : 1)
                .ThenBy(x => x.Title)
                .FirstOrDefault();

            if (result == null)
                return 2; // no match

            args.SetParameter(Globals.MOVIE_INFO, result);

            Variables["miTitle"] = result.Title;
            Variables["miYear"] = result.ReleaseDate.Year;

            args.UpdateVariables(Variables);

            return 1;

        }

    }
}