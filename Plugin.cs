using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using NextEpisodePlugin.Configuration;

namespace NextEpisodePlugin
{
    /// <summary>
    /// Main plugin class for Next Episode functionality
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public override string Name => "Next Episode Plugin";

        public override string Description => "Adds a button to play the next episode of a random started TV show";

        public override Guid Id => Guid.Parse("8B8C4A12-3F4E-4D5A-9B6C-7E8F9A0B1C2D");

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) 
            : base(applicationPaths, xmlSerializer)
        {
        }

        public override string ConfigurationFileName => "NextEpisodePlugin.xml";
    }
}