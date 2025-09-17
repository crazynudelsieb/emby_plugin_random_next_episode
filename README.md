# Next Episode Plugin for Emby

This plugin adds a "Play Random Next Episode" button to the Emby home page that helps you continue watching TV shows you've started but haven't finished.

## Features

- **Random Selection**: Automatically selects a random TV series that you've started watching but haven't completed
- **Next Episode Detection**: Finds the next unwatched episode in the selected series
- **One-Click Play**: Plays the episode immediately with a single button click
- **Home Page Integration**: Adds a prominent button to the Emby home page
- **Configurable**: Enable/disable features through the plugin configuration page

## How It Works

1. The plugin scans your library for TV series where:
   - You have watched at least one episode
   - There are still unwatched episodes remaining
2. It randomly selects one of these "started" series
3. It finds the next unwatched episode in chronological order
4. It starts playing that episode in your current session

## Installation

1. Build the plugin using Visual Studio 2017+ or .NET Core SDK:
   ```bash
   dotnet build NextEpisodePlugin.csproj
   ```

2. Copy the built DLL to your Emby server's plugins directory:
   - Windows: `%ProgramData%\Emby-Server\plugins`
   - Linux: `/var/lib/emby/plugins`

3. Restart your Emby server

4. The plugin will automatically add a "🎬 Play Random Next Episode" button to your home page

## Configuration

You can configure the plugin through the Emby dashboard:

1. Go to **Dashboard** → **Plugins** → **Next Episode Plugin**
2. Configure the following options:
   - **Enable Random Selection**: Enable/disable the random selection feature
   - **Show Button on Homepage**: Show/hide the button on the home page

## API Endpoints

The plugin also provides REST API endpoints:

- `GET /nextepisode/random?UserId={userId}`: Get information about the next random episode
- `POST /nextepisode/random?UserId={userId}&Play=true`: Play the next random episode

## Requirements

- Emby Server 4.8.0.80 or later
- .NET Standard 2.0
- TV series in your Emby library with watch progress

## Building from Source

1. Clone this repository
2. Ensure you have .NET Core SDK installed
3. Run: `dotnet build`
4. The compiled plugin will be in `bin/Debug/netstandard2.0/`

## Troubleshooting

- **Button not appearing**: Check the plugin configuration to ensure "Show Button on Homepage" is enabled
- **No episodes found**: Make sure you have TV series in your library with partial watch progress
- **Plugin not loading**: Check the Emby server logs for any error messages

## License

This plugin is provided as-is for educational and personal use.