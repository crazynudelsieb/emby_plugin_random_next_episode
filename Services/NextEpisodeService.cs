using System;
using System.Linq;
using System.Collections.Generic;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Users;

namespace NextEpisodePlugin.Services
{
    /// <summary>
    /// Core service for finding next episodes from partially watched TV series
    /// </summary>
    public class NextEpisodeService
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;

        public NextEpisodeService(ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger)
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _userDataManager = userDataManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets a random next episode from user's partially watched series
        /// </summary>
        /// <param name="userId">User ID string</param>
        /// <returns>Next episode to watch, or null if none found</returns>
        public Episode GetRandomNextEpisode(string userId)
        {
            try
            {
                _logger.Info($"Finding random next episode for user: {userId}");

                // Convert string userId to Guid
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    _logger.Error($"Invalid user ID format: {userId}");
                    return null;
                }

                var user = _userManager.GetUserById(userGuid);
                if (user == null)
                {
                    _logger.Error($"User not found: {userId}");
                    return null;
                }

                var startedSeries = GetStartedButNotFinishedSeries(user);
                
                if (!startedSeries.Any())
                {
                    _logger.Info("No started series found");
                    return null;
                }

                var random = new Random();
                var randomSeries = startedSeries[random.Next(startedSeries.Count)];
                
                var nextEpisode = GetNextUnwatchedEpisode(randomSeries, user);
                
                if (nextEpisode != null)
                {
                    _logger.Info($"Found next episode: {nextEpisode.Name} from series: {randomSeries.Name}");
                }
                else
                {
                    _logger.Info($"No next episode found for series: {randomSeries.Name}");
                }

                return nextEpisode;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting random next episode", ex);
                return null;
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

            return startedSeries;
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