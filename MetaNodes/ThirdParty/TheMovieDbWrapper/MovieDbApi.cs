using DM.MovieApi.MovieDb.Certifications;
using DM.MovieApi.MovieDb.Companies;
using DM.MovieApi.MovieDb.Configuration;
using DM.MovieApi.MovieDb.Discover;
using DM.MovieApi.MovieDb.Genres;
using DM.MovieApi.MovieDb.IndustryProfessions;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.People;
using DM.MovieApi.MovieDb.TV;

namespace DM.MovieApi
{
    internal class MovieDbApi : IMovieDbApi
    {

        #region Lazy Imports
#pragma warning disable 0649

        private Lazy<IApiCompanyRequest> _companyRequest;

        private Lazy<IApiConfigurationRequest> _configuration;

        private Lazy<IApiGenreRequest> _genres;

        private Lazy<IApiProfessionRequest> _industryProfessions;

        private Lazy<IApiMovieRequest> _movies;

        private Lazy<IApiMovieRatingRequest> _movieRatings;

        private Lazy<IApiTVShowRequest> _television;

        private Lazy<IApiPeopleRequest> _people;
        
        private Lazy<IApiDiscoverRequest> _discover;

#pragma warning restore 0649
        #endregion

        public IApiCompanyRequest Companies => _companyRequest.Value;

        public IApiConfigurationRequest Configuration => _configuration.Value;

        public IApiGenreRequest Genres => _genres.Value;

        public IApiProfessionRequest IndustryProfessions => _industryProfessions.Value;

        public IApiMovieRequest Movies => _movies.Value;

        public IApiMovieRatingRequest MovieRatings => _movieRatings.Value;

        public IApiTVShowRequest Television => _television.Value;

        public IApiPeopleRequest People => _people.Value;

        public IApiDiscoverRequest Discover => _discover.Value;
    }
}