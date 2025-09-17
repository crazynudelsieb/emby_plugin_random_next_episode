using MediaBrowser.Model.Plugins;

namespace NextEpisodePlugin.Configuration
{
    /// <summary>
    /// Plugin configuration settings
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        public bool EnableRandomSelection { get; set; } = true;
        public bool ShowButtonOnHomepage { get; set; } = true;
    }
}