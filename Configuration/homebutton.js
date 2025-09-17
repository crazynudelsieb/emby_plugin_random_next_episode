(function() {
    'use strict';

    var pluginId = "8B8C4A12-3F4E-4D5A-9B6C-7E8F9A0B1C2D";
    var buttonAdded = false;

    function addNextEpisodeButton() {
        if (buttonAdded) return;

        // Check if we're on the home page
        if (window.location.hash !== '#/home.html' && window.location.pathname !== '/web/index.html' && !window.location.pathname.endsWith('/')) {
            return;
        }

        // Wait for the page to be fully loaded
        setTimeout(function() {
            var homeContainer = document.querySelector('.homePage') || 
                               document.querySelector('.homePageContent') || 
                               document.querySelector('.page');
            
            if (!homeContainer) return;

            // Check if button already exists
            if (document.querySelector('.nextEpisodeButton')) return;

            // Create the button container
            var buttonContainer = document.createElement('div');
            buttonContainer.className = 'nextEpisodeButtonContainer';
            buttonContainer.style.cssText = 'margin: 20px; text-align: center;';

            // Create the button
            var button = document.createElement('button');
            button.className = 'nextEpisodeButton raised button-submit';
            button.innerHTML = '<span>🎬 Play Random Next Episode</span>';
            button.style.cssText = 'padding: 15px 30px; font-size: 16px; background-color: #00a4dc; color: white; border: none; border-radius: 5px; cursor: pointer;';

            // Add hover effect
            button.addEventListener('mouseenter', function() {
                this.style.backgroundColor = '#0082b3';
            });
            button.addEventListener('mouseleave', function() {
                this.style.backgroundColor = '#00a4dc';
            });

            // Add click handler
            button.addEventListener('click', function() {
                playRandomNextEpisode();
            });

            buttonContainer.appendChild(button);
            
            // Insert at the top of the home page
            var firstChild = homeContainer.firstChild;
            if (firstChild) {
                homeContainer.insertBefore(buttonContainer, firstChild);
            } else {
                homeContainer.appendChild(buttonContainer);
            }

            buttonAdded = true;
        }, 1000);
    }

    function playRandomNextEpisode() {
        var button = document.querySelector('.nextEpisodeButton');
        if (!button) return;

        // Disable button and show loading
        button.disabled = true;
        button.innerHTML = '<span>🔄 Finding episode...</span>';

        // Get current user
        var userId = Dashboard.getCurrentUserId() || ApiClient.getCurrentUserId();
        
        if (!userId) {
            button.innerHTML = '<span>❌ Error: No user found</span>';
            setTimeout(function() {
                button.disabled = false;
                button.innerHTML = '<span>🎬 Play Random Next Episode</span>';
            }, 3000);
            return;
        }

        // Make API call to play random next episode
        var requestData = {
            UserId: userId,
            Play: true
        };
        
        ApiClient.ajax({
            type: 'POST',
            url: ApiClient.getUrl('nextepisode/random'),
            data: requestData
        })
        .then(function(data) {
            if (data.Success) {
                button.innerHTML = '<span>✅ Playing: ' + data.EpisodeName + '</span>';
                setTimeout(function() {
                    button.disabled = false;
                    button.innerHTML = '<span>🎬 Play Random Next Episode</span>';
                }, 5000);
            } else {
                button.innerHTML = '<span>❌ ' + (data.Message || 'No episodes found') + '</span>';
                setTimeout(function() {
                    button.disabled = false;
                    button.innerHTML = '<span>🎬 Play Random Next Episode</span>';
                }, 3000);
            }
        })
        .catch(function(error) {
            console.error('Error playing random next episode:', error);
            button.innerHTML = '<span>❌ Error occurred</span>';
            setTimeout(function() {
                button.disabled = false;
                button.innerHTML = '<span>🎬 Play Random Next Episode</span>';
            }, 3000);
        });
    }

    function checkConfiguration() {
        ApiClient.getPluginConfiguration(pluginId).then(function(config) {
            if (config.ShowButtonOnHomepage !== false) { // Default to true
                addNextEpisodeButton();
            }
        }).catch(function() {
            // If config fails, still show button by default
            addNextEpisodeButton();
        });
    }

    // Watch for page changes
    function onPageChange() {
        buttonAdded = false;
        setTimeout(checkConfiguration, 500);
    }

    // Listen for navigation events
    if (window.addEventListener) {
        window.addEventListener('hashchange', onPageChange);
        window.addEventListener('popstate', onPageChange);
    }

    // Initial load
    document.addEventListener('DOMContentLoaded', function() {
        setTimeout(checkConfiguration, 1000);
    });

    // For Emby's navigation system
    if (window.Emby && window.Emby.Page) {
        window.Emby.Page.addEventListener('viewshow', onPageChange);
    }

    // Fallback: Check periodically
    setInterval(function() {
        if (!buttonAdded) {
            checkConfiguration();
        }
    }, 5000);

})();