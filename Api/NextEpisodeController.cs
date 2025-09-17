using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Services;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Users;
using NextEpisodePlugin.Services;

namespace NextEpisodePlugin.Api
{
    [Route("/NextEpisodePlugin/random", "GET", Summary = "Gets a random next episode")]
    [Route("/NextEpisodePlugin/random", "POST", Summary = "Plays a random next episode")]
    public class GetRandomNextEpisode : IReturn<NextEpisodeResponse>
    {
        [ApiMember(Name = "UserId", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "GET,POST")]
        public string UserId { get; set; }

        [ApiMember(Name = "Play", Description = "Whether to play the episode", IsRequired = false, DataType = "bool", ParameterType = "query", Verb = "POST")]
        public bool Play { get; set; }
    }

    public class NextEpisodeResponse
    {
        public string EpisodeId { get; set; }
        public string EpisodeName { get; set; }
        public string SeriesName { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class NextEpisodeController : IService
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ISessionManager _sessionManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;
        private readonly NextEpisodeService _nextEpisodeService;

        public NextEpisodeController(ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IUserDataManager userDataManager, ILogManager logManager)
        {
            _libraryManager = libraryManager;
            _sessionManager = sessionManager;
            _userManager = userManager;
            _userDataManager = userDataManager;
            _logger = logManager.GetLogger(GetType().Name);
            _nextEpisodeService = new NextEpisodeService(_libraryManager, _userManager, _userDataManager, _logger);
        }

        public Task<object> Get(GetRandomNextEpisode request)
        {
            try
            {
                var episode = _nextEpisodeService.GetRandomNextEpisode(request.UserId);
                
                if (episode == null)
                {
                    return Task.FromResult<object>(new NextEpisodeResponse
                    {
                        Success = false,
                        Message = "No next episode found for any started series"
                    });
                }

                return Task.FromResult<object>(new NextEpisodeResponse
                {
                    EpisodeId = episode.Id.ToString(),
                    EpisodeName = episode.Name,
                    SeriesName = episode.Series?.Name,
                    SeasonNumber = episode.ParentIndexNumber,
                    EpisodeNumber = episode.IndexNumber,
                    Success = true,
                    Message = "Episode found successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error in GetRandomNextEpisode", ex);
                return Task.FromResult<object>(new NextEpisodeResponse
                {
                    Success = false,
                    Message = "An error occurred while finding the next episode"
                });
            }
        }

        public async Task<object> Post(GetRandomNextEpisode request)
        {
            try
            {
                var episode = _nextEpisodeService.GetRandomNextEpisode(request.UserId);
                
                if (episode == null)
                {
                    return new NextEpisodeResponse
                    {
                        Success = false,
                        Message = "No next episode found for any started series"
                    };
                }

                if (request.Play)
                {
                    if (!Guid.TryParse(request.UserId, out var userGuid))
                    {
                        return new NextEpisodeResponse
                        {
                            Success = false,
                            Message = "Invalid user ID format"
                        };
                    }

                    var user = _userManager.GetUserById(userGuid);
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
                    }
                    else
                    {
                        return new NextEpisodeResponse
                        {
                            Success = false,
                            Message = "No active session found for user"
                        };
                    }
                }

                return new NextEpisodeResponse
                {
                    EpisodeId = episode.Id.ToString(),
                    EpisodeName = episode.Name,
                    SeriesName = episode.Series?.Name,
                    SeasonNumber = episode.ParentIndexNumber,
                    EpisodeNumber = episode.IndexNumber,
                    Success = true,
                    Message = request.Play ? "Episode started playing" : "Episode found successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error in PostRandomNextEpisode", ex);
                return new NextEpisodeResponse
                {
                    Success = false,
                    Message = "An error occurred while playing the next episode"
                };
            }
        }
    }
}