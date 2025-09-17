# Build Instructions

## Requirements
- .NET SDK 8.0+

## Build
```bash
dotnet build NextEpisodePlugin.csproj --configuration Release
```

## Output
- `bin/Release/netstandard2.0/NextEpisodePlugin.dll`

## Install
```bash
cp bin/Release/netstandard2.0/NextEpisodePlugin.dll /path/to/emby/plugins/
```

## Usage
```bash
curl -X POST "http://emby:8096/NextEpisodePlugin/random?UserId=USER_ID&Play=true"
```