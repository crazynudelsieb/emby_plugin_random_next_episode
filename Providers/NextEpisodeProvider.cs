using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using NextEpisodePlugin.Services;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Users;

namespace NextEpisodePlugin.Providers
{
    public class NextEpisodeProvider
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;
        private readonly NextEpisodeService _nextEpisodeService;

        public NextEpisodeProvider(ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogManager logManager)
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _userDataManager = userDataManager;
            _logger = logManager.GetLogger(GetType().Name);
            _nextEpisodeService = new NextEpisodeService(_libraryManager, _userManager, _userDataManager, _logger);
        }

        public BaseItem GetNextEpisodeItem(Guid userId)
        {
            try
            {
                var episode = _nextEpisodeService.GetRandomNextEpisode(userId.ToString());
                
                if (episode == null)
                    return null;

                // Create a virtual item that represents "Play Next Episode"
                var virtualItem = new Folder
                {
                    Name = $"▶ Continue {episode.Series?.Name}",
                    Overview = $"Next: {episode.Name} (S{episode.ParentIndexNumber}E{episode.IndexNumber})",
                    Id = Guid.NewGuid(),
                    DateCreated = DateTime.UtcNow
                };

                // Set a provider ID to link back to the actual episode
                virtualItem.SetProviderId("NextEpisodePlugin", episode.Id.ToString());

                return virtualItem;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error creating next episode item", ex);
                return null;
            }
        }
    }
}