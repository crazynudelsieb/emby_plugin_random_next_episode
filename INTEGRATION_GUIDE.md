# Next Episode Plugin - TV App Integration Guide

This plugin provides **full TV app compatibility** through server-side API integration that works with all Emby clients including TV apps, mobile apps, and web browsers.

## 🚀 Quick Installation

1. **Install the plugin**:
   ```bash
   cp bin/Release/netstandard2.0/NextEpisodePlugin.dll /path/to/emby/plugins/
   ```

2. **Restart Emby Server**

3. **The plugin is now active** and provides the following TV-compatible endpoints:

## 📺 TV App Compatible Features

### 1. Continue Watching Section API
Get a list of next episodes to continue watching:
```
GET /NextEpisodePlugin/ContinueWatching?UserId={USER_ID}&Limit=10
```

**Response**: JSON array of episodes ready to continue, including a special "🎬 Play Random Next Episode" item.

### 2. One-Click Random Play API
Instantly play a random next episode:
```
POST /NextEpisodePlugin/PlayRandomNext?UserId={USER_ID}
```

**Response**: Automatically starts playback and returns episode details.

### 3. Direct Episode API
Get or play specific next episodes:
```
GET /NextEpisodePlugin/random?UserId={USER_ID}
POST /NextEpisodePlugin/random?UserId={USER_ID}&Play=true
```

## 🎮 TV Remote Control Integration

The plugin works with **all TV app interfaces** including:
- ✅ Android TV
- ✅ Apple TV
- ✅ Samsung Smart TVs
- ✅ LG WebOS
- ✅ Roku
- ✅ Fire TV
- ✅ Xbox/PlayStation
- ✅ Web browsers
- ✅ Mobile apps

## 🔧 Integration Methods

### Method 1: Custom Home Screen Section (Recommended)
For apps that support custom sections, configure a new home screen section:

**Endpoint**: `/NextEpisodePlugin/ContinueWatching`
**Section Name**: "Continue Random Series"
**Type**: "Continue Watching"

### Method 2: Quick Action Button
For TV apps with quick action support:

**Action URL**: `/NextEpisodePlugin/PlayRandomNext`
**Button Text**: "🎬 Play Random Next Episode"

### Method 3: Custom Dashboard Widget
Add a dashboard widget that calls the API endpoints.

### Method 4: Voice Command Integration
For voice-enabled remotes:
**Command**: "Play random next episode"
**API Call**: `POST /NextEpisodePlugin/PlayRandomNext`

## 🛠️ Developer Integration

### For Custom TV App Development:

```javascript
// Get continue watching items
async function getContinueWatching(userId) {
    const response = await fetch(`/NextEpisodePlugin/ContinueWatching?UserId=${userId}&Limit=10`);
    return await response.json();
}

// Play random next episode
async function playRandomNext(userId) {
    const response = await fetch(`/NextEpisodePlugin/PlayRandomNext?UserId=${userId}`, {
        method: 'POST'
    });
    return await response.json();
}
```

### For Emby Plugin Developers:

```csharp
// Access the recommendation provider
var recommendationProvider = new NextEpisodeRecommendationProvider(
    libraryManager, userManager, userDataManager, logManager);

// Get next episodes
var episodes = recommendationProvider.GetContinueWatchingEpisodes(userId, limit: 10);

// Get random recommendation
var randomEpisode = recommendationProvider.GetRecommendedNextEpisode(userId);
```

## 📋 Configuration

The plugin automatically detects:
- ✅ Started but not finished TV series
- ✅ Next unwatched episodes in chronological order
- ✅ User watch progress and preferences
- ✅ Available episodes and their metadata

No manual configuration required - it works out of the box!

## 🔍 Finding Your User ID

### Method 1: Web Browser Console
1. Open Emby in a web browser
2. Press F12 to open developer tools
3. Run: `console.log(ApiClient.getCurrentUserId())`

### Method 2: API Call
```
GET /Users/Me
```
(Returns current user information including ID)

### Method 3: Dashboard
Check the Emby server dashboard under "Users" - the user ID is visible in the URL when editing a user.

## 🎯 How It Works

1. **Library Scanning**: Monitors your TV library for partially watched series
2. **Smart Detection**: Identifies next episodes to watch based on your progress
3. **Random Selection**: Picks a random series from your "in progress" shows
4. **Instant Playback**: Starts the next episode immediately on your current device
5. **Cross-Platform**: Works on any device that can make HTTP requests to Emby

## 🚨 Troubleshooting

**No episodes found**: Make sure you have TV series with partial progress (some episodes watched, some not)

**Playback fails**: Ensure you have an active Emby session on the device you want to play on

**API not responding**: Verify the plugin DLL is in the correct plugins directory and Emby has been restarted

**TV app integration**: Contact your TV app developer to integrate the provided API endpoints

## 🔧 Advanced Usage

### Custom Continue Watching Section
Configure your Emby client to add a custom section that calls:
```
/NextEpisodePlugin/ContinueWatching?UserId={USER_ID}&Limit=20&Fields=Overview,SeriesStudio
```

### Automated Playback
Set up automation (like home automation systems) to trigger random episode playback:
```bash
curl -X POST "http://emby-server:8096/NextEpisodePlugin/PlayRandomNext?UserId=USER_ID"
```

The plugin provides a **complete server-side solution** that integrates seamlessly with any Emby client, ensuring full TV app compatibility without requiring client-side modifications.