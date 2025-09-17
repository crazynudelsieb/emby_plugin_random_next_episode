using MediaBrowser.Model.Plugins;

namespace NextEpisodePlugin.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public bool EnableRandomSelection { get; set; } = true;
        public bool ShowButtonOnHomepage { get; set; } = true;
    }
}