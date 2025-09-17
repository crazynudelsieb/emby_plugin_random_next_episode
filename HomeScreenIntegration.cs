using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Users;
using NextEpisodePlugin.Services;

namespace NextEpisodePlugin
{
    public class HomeScreenIntegration : IServerEntryPoint
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;
        private readonly NextEpisodeService _nextEpisodeService;

        public HomeScreenIntegration(ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogManager logManager)
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _userDataManager = userDataManager;
            _logger = logManager.GetLogger(GetType().Name);
            _nextEpisodeService = new NextEpisodeService(_libraryManager, _userManager, _userDataManager, _logger);
        }

        public void Run()
        {
            _logger.Info("Next Episode Home Screen Integration started");
            
            // Hook into library events to refresh when needed
            _libraryManager.ItemAdded += OnLibraryChanged;
            _libraryManager.ItemUpdated += OnLibraryChanged;
        }

        private void OnLibraryChanged(object sender, ItemChangeEventArgs e)
        {
            // Refresh next episode recommendations when library changes
            if (e.Item is MediaBrowser.Controller.Entities.TV.Episode)
            {
                _logger.Debug("Episode added/updated, next episode cache may need refresh");
            }
        }

        public void Dispose()
        {
            _libraryManager.ItemAdded -= OnLibraryChanged;
            _libraryManager.ItemUpdated -= OnLibraryChanged;
        }

        public BaseItem CreateNextEpisodeVirtualItem(User user)
        {
            try
            {
                var episode = _nextEpisodeService.GetRandomNextEpisode(user.Id.ToString());
                
                if (episode == null)
                    return null;

                // Create a virtual folder that acts as a "Continue Watching" item
                var virtualItem = new CollectionFolder
                {
                    Name = "🎬 Continue Random Series",
                    Id = new Guid("00000000-0000-0000-0000-000000000001"), // Fixed ID for consistency
                    DateCreated = DateTime.UtcNow,
                    Overview = $"Continue watching {episode.Series?.Name} - {episode.Name}"
                };

                // Set custom properties to identify this as our special item
                virtualItem.SetProviderId("NextEpisodePlugin", episode.Id.ToString());

                return virtualItem;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error creating virtual next episode item", ex);
                return null;
            }
        }
    }
}