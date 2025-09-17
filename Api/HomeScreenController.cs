using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Services;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Users;
using MediaBrowser.Model.Querying;
using NextEpisodePlugin.Services;

namespace NextEpisodePlugin.Api
{
    [Route("/NextEpisodePlugin/ContinueWatching", "GET", Summary = "Gets continue watching items for home screen")]
    public class GetContinueWatching : IReturn<QueryResult<BaseItemDto>>
    {
        [ApiMember(Name = "UserId", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "query")]
        public string UserId { get; set; }

        [ApiMember(Name = "Limit", Description = "Optional limit", IsRequired = false, DataType = "int", ParameterType = "query")]
        public int? Limit { get; set; }

        [ApiMember(Name = "Fields", Description = "Optional fields", IsRequired = false, DataType = "string", ParameterType = "query")]
        public string Fields { get; set; }
    }

    [Route("/NextEpisodePlugin/PlayRandomNext", "POST", Summary = "Plays random next episode and returns playback info")]
    public class PlayRandomNext : IReturn<PlaybackInfoResponse>
    {
        [ApiMember(Name = "UserId", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "query")]
        public string UserId { get; set; }
    }

    public class PlaybackInfoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string SeriesName { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
    }

    public class BaseItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string Type { get; set; }
        public string SeriesName { get; set; }
        public int? IndexNumber { get; set; }
        public int? ParentIndexNumber { get; set; }
        public string PrimaryImageTag { get; set; }
        public bool CanPlay { get; set; }
        public string MediaType { get; set; }
    }

    public class QueryResult<T>
    {
        public T[] Items { get; set; }
        public int TotalRecordCount { get; set; }
    }

    public class HomeScreenController : IService
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ISessionManager _sessionManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;
        private readonly NextEpisodeService _nextEpisodeService;
        private readonly NextEpisodeRecommendationProvider _recommendationProvider;

        public HomeScreenController(ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IUserDataManager userDataManager, ILogManager logManager)
        {
            _libraryManager = libraryManager;
            _sessionManager = sessionManager;
            _userManager = userManager;
            _userDataManager = userDataManager;
            _logger = logManager.GetLogger(GetType().Name);
            _nextEpisodeService = new NextEpisodeService(_libraryManager, _userManager, _userDataManager, _logger);
            _recommendationProvider = new NextEpisodeRecommendationProvider(_libraryManager, _userManager, _userDataManager, logManager);
        }

        public Task<object> Get(GetContinueWatching request)
        {
            try
            {
                if (!Guid.TryParse(request.UserId, out var userGuid))
                {
                    return Task.FromResult<object>(new QueryResult<BaseItemDto>
                    {
                        Items = new BaseItemDto[0],
                        TotalRecordCount = 0
                    });
                }

                var limit = request.Limit ?? 10;
                var episodes = _recommendationProvider.GetContinueWatchingEpisodes(userGuid, limit);

                var items = episodes.Select(episode => new BaseItemDto
                {
                    Id = episode.Id.ToString(),
                    Name = episode.Name,
                    Overview = episode.Overview,
                    Type = "Episode",
                    SeriesName = episode.Series?.Name,
                    IndexNumber = episode.IndexNumber,
                    ParentIndexNumber = episode.ParentIndexNumber,
                    CanPlay = true,
                    MediaType = "Video"
                }).ToArray();

                // Add a special "Random Next Episode" item at the beginning
                if (items.Any())
                {
                    var randomItem = new BaseItemDto
                    {
                        Id = "NextEpisodePlugin-Random",
                        Name = "🎬 Play Random Next Episode",
                        Overview = "Randomly select and play the next episode from your started series",
                        Type = "NextEpisodeAction",
                        CanPlay = true,
                        MediaType = "Video"
                    };

                    var allItems = new BaseItemDto[] { randomItem }.Concat(items).ToArray();
                    
                    return Task.FromResult<object>(new QueryResult<BaseItemDto>
                    {
                        Items = allItems,
                        TotalRecordCount = allItems.Length
                    });
                }

                return Task.FromResult<object>(new QueryResult<BaseItemDto>
                {
                    Items = items,
                    TotalRecordCount = items.Length
                });
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error in GetContinueWatching", ex);
                return Task.FromResult<object>(new QueryResult<BaseItemDto>
                {
                    Items = new BaseItemDto[0],
                    TotalRecordCount = 0
                });
            }
        }

        public async Task<object> Post(PlayRandomNext request)
        {
            try
            {
                if (!Guid.TryParse(request.UserId, out var userGuid))
                {
                    return new PlaybackInfoResponse
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    };
                }

                var episode = _nextEpisodeService.GetRandomNextEpisode(request.UserId);
                
                if (episode == null)
                {
                    return new PlaybackInfoResponse
                    {
                        Success = false,
                        Message = "No next episode found for any started series"
                    };
                }

                // Find an active session for the user
                var sessions = _sessionManager.Sessions.Where(s => s.UserId == userGuid.ToString());
                
                if (sessions.Any())
                {
                    var session = sessions.First();
                    
                    var playRequest = new MediaBrowser.Model.Session.PlayRequest
                    {
                        ItemIds = new[] { Convert.ToInt64(episode.Id) },
                        PlayCommand = MediaBrowser.Model.Session.PlayCommand.PlayNow
                    };

                    await _sessionManager.SendPlayCommand(session.Id, session.Id, playRequest, CancellationToken.None);

                    return new PlaybackInfoResponse
                    {
                        Success = true,
                        Message = "Episode started playing",
                        ItemId = episode.Id.ToString(),
                        ItemName = episode.Name,
                        SeriesName = episode.Series?.Name,
                        SeasonNumber = episode.ParentIndexNumber,
                        EpisodeNumber = episode.IndexNumber
                    };
                }
                else
                {
                    return new PlaybackInfoResponse
                    {
                        Success = false,
                        Message = "No active session found for user"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error in PlayRandomNext", ex);
                return new PlaybackInfoResponse
                {
                    Success = false,
                    Message = "An error occurred while playing the next episode"
                };
            }
        }
    }
}