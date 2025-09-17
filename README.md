# Next Episode Plugin for Emby

API-based plugin for Emby that finds and plays next episodes from partially watched TV series.

## Features

- **Smart Detection**: Automatically finds TV series you've started but not finished
- **Random Selection**: Picks a random series from your in-progress shows  
- **Next Episode Logic**: Finds the next unwatched episode in chronological order
- **Instant Playback**: Starts playing immediately on your current device
- **REST API**: Clean API endpoints for integration with any Emby client

## API Endpoints

### Get Random Next Episode
```
GET /NextEpisodePlugin/random?UserId={userId}
```
Returns episode information without playing.

### Play Random Next Episode  
```
POST /NextEpisodePlugin/random?UserId={userId}&Play=true
```
Finds and immediately plays a random next episode.

### Continue Watching List
```
GET /NextEpisodePlugin/ContinueWatching?UserId={userId}&Limit=10
```
Returns list of next episodes for home screen integration.

## Installation

1. Copy `NextEpisodePlugin.dll` to your Emby plugins directory
2. Restart Emby server
3. API endpoints are immediately available

## Usage Examples

### Direct API Call
```bash
curl -X POST "http://emby-server:8096/NextEpisodePlugin/random?UserId=USER_ID&Play=true"
```

### JavaScript Integration
```javascript
// Play random next episode
fetch(`/NextEpisodePlugin/random?UserId=${userId}&Play=true`, {method: 'POST'})
  .then(r => r.json())
  .then(data => console.log(`Playing: ${data.EpisodeName}`));
```

### TV App Integration
```javascript
// Get continue watching items for home screen
const response = await fetch(`/NextEpisodePlugin/ContinueWatching?UserId=${userId}`);
const {Items} = await response.json();
```

## Requirements

- Emby Server 4.8.0+
- TV series in library with partial watch progress
- Active user session for playback

## How It Works

1. Scans user's TV library for partially watched series
2. Identifies next unwatched episodes in chronological order
3. Randomly selects from available options
4. Uses Emby's session API to start playback

## Finding Your User ID

**Browser Console Method:**
1. Open Emby web interface
2. Press F12 → Console tab
3. Run: `console.log(ApiClient.getCurrentUserId())`

**URL Method:**
1. Edit your user profile in Emby
2. Check browser address bar for `userId=` parameter

## Development

Built with .NET Standard 2.0 targeting MediaBrowser.Server.Core 4.8.11.

### Project Structure
```
├── Plugin.cs                    # Main plugin entry point
├── Configuration/
│   └── PluginConfiguration.cs   # Plugin settings
├── Services/
│   └── NextEpisodeService.cs    # Core logic
└── Api/
    ├── NextEpisodeController.cs  # Main API endpoints
    └── HomeScreenController.cs   # Continue watching API
```

### Building
```bash
dotnet build NextEpisodePlugin.csproj --configuration Release
```

## License

MIT License - Educational and personal use.