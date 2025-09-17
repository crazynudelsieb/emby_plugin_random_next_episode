using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using NextEpisodePlugin.Services;
using MediaBrowser.Model.Users;

namespace NextEpisodePlugin
{
    public class NextEpisodeRecommendationProvider
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;
        private readonly NextEpisodeService _nextEpisodeService;

        public NextEpisodeRecommendationProvider(ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogManager logManager)
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _userDataManager = userDataManager;
            _logger = logManager.GetLogger(GetType().Name);
            _nextEpisodeService = new NextEpisodeService(_libraryManager, _userManager, _userDataManager, _logger);
        }

        public Episode GetRecommendedNextEpisode(Guid userId)
        {
            return _nextEpisodeService.GetRandomNextEpisode(userId.ToString());
        }

        public List<Episode> GetContinueWatchingEpisodes(Guid userId, int limit = 10)
        {
            try
            {
                var user = _userManager.GetUserById(userId);
                if (user == null) return new List<Episode>();

                var startedSeries = GetStartedButNotFinishedSeries(user);
                var continueWatchingEpisodes = new List<Episode>();

                foreach (var series in startedSeries.Take(limit))
                {
                    var nextEpisode = GetNextUnwatchedEpisode(series, user);
                    if (nextEpisode != null)
                    {
                        continueWatchingEpisodes.Add(nextEpisode);
                    }
                }

                return continueWatchingEpisodes;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting continue watching episodes", ex);
                return new List<Episode>();
            }
        }

        private List<Series> GetStartedButNotFinishedSeries(User user)
        {
            var query = new InternalItemsQuery(user)
            {
                IncludeItemTypes = new[] { "Series" },
                Recursive = true
            };

            var allSeries = _libraryManager.GetItemList(query).Cast<Series>().ToList();
            var startedSeries = new List<Series>();

            foreach (var series in allSeries)
            {
                var episodes = GetAllEpisodes(series);
                
                if (episodes.Any())
                {
                    var hasWatchedEpisodes = episodes.Any(ep => 
                    {
                        var epUserData = _userDataManager.GetUserData(user, ep);
                        return epUserData.Played;
                    });

                    var hasUnwatchedEpisodes = episodes.Any(ep => 
                    {
                        var epUserData = _userDataManager.GetUserData(user, ep);
                        return !epUserData.Played;
                    });

                    if (hasWatchedEpisodes && hasUnwatchedEpisodes)
                    {
                        startedSeries.Add(series);
                    }
                }
            }

            return startedSeries.OrderBy(s => Guid.NewGuid()).ToList(); // Random order
        }

        private Episode GetNextUnwatchedEpisode(Series series, User user)
        {
            var episodes = GetAllEpisodes(series)
                .OrderBy(ep => ep.ParentIndexNumber ?? 0)
                .ThenBy(ep => ep.IndexNumber ?? 0)
                .ToList();

            foreach (var episode in episodes)
            {
                var userData = _userDataManager.GetUserData(user, episode);
                if (!userData.Played)
                {
                    return episode;
                }
            }

            return null;
        }

        private List<Episode> GetAllEpisodes(Series series)
        {
            var query = new InternalItemsQuery
            {
                Parent = series,
                IncludeItemTypes = new[] { "Episode" },
                Recursive = true
            };

            return _libraryManager.GetItemList(query).Cast<Episode>().ToList();
        }
    }
}