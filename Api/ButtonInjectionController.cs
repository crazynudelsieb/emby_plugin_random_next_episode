using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Services;

namespace NextEpisodePlugin.Api
{
    [Route("/NextEpisodePlugin/inject", "GET", Summary = "Injects the next episode button into the current page")]
    public class InjectButton
    {
        [ApiMember(Name = "UserId", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "query")]
        public string UserId { get; set; }
    }

    public class ButtonInjectionController : IService
    {
        public object Get(InjectButton request)
        {
            var script = @"
<script>
(function() {
    'use strict';
    
    var buttonAdded = false;
    
    function addNextEpisodeButton() {
        if (buttonAdded) return;
        
        // Find a container on the home page
        var containers = [
            document.querySelector('.sections'),
            document.querySelector('.homePageContent'),
            document.querySelector('.page'),
            document.querySelector('main'),
            document.querySelector('body')
        ];
        
        var homeContainer = null;
        for (var i = 0; i < containers.length; i++) {
            if (containers[i]) {
                homeContainer = containers[i];
                break;
            }
        }
        
        if (!homeContainer) return;
        
        // Check if button already exists
        if (document.querySelector('.nextEpisodeButton')) return;
        
        // Create the button
        var button = document.createElement('button');
        button.className = 'nextEpisodeButton';
        button.innerHTML = '🎬 Play Random Next Episode';
        button.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            z-index: 1000;
            padding: 12px 20px;
            font-size: 14px;
            background-color: #00a4dc;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(0,0,0,0.3);
        `;
        
        button.addEventListener('click', function() {
            playRandomNextEpisode('" + request.UserId + @"');
        });
        
        document.body.appendChild(button);
        buttonAdded = true;
    }
    
    function playRandomNextEpisode(userId) {
        var button = document.querySelector('.nextEpisodeButton');
        if (!button) return;
        
        button.disabled = true;
        button.innerHTML = '🔄 Finding episode...';
        
        fetch('/NextEpisodePlugin/random?UserId=' + userId + '&Play=true', {
            method: 'POST',
            headers: {
                'Authorization': ApiClient.getAuthenticationInfo().AccessToken
            }
        })
        .then(function(response) { return response.json(); })
        .then(function(data) {
            if (data.Success) {
                button.innerHTML = '✅ Playing: ' + data.EpisodeName;
                setTimeout(function() {
                    button.disabled = false;
                    button.innerHTML = '🎬 Play Random Next Episode';
                }, 5000);
            } else {
                button.innerHTML = '❌ ' + (data.Message || 'No episodes found');
                setTimeout(function() {
                    button.disabled = false;
                    button.innerHTML = '🎬 Play Random Next Episode';
                }, 3000);
            }
        })
        .catch(function(error) {
            console.error('Error:', error);
            button.innerHTML = '❌ Error occurred';
            setTimeout(function() {
                button.disabled = false;
                button.innerHTML = '🎬 Play Random Next Episode';
            }, 3000);
        });
    }
    
    // Add the button when the page loads
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', addNextEpisodeButton);
    } else {
        addNextEpisodeButton();
    }
})();
</script>";

            return script;
        }
    }
}