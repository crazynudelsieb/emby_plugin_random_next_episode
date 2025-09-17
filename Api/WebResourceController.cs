using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Model.Services;

namespace NextEpisodePlugin.Api
{
    [Route("/NextEpisodePlugin/homebutton.js", "GET")]
    public class GetHomeButtonScript
    {
    }

    [Route("/NextEpisodePlugin/configPage.html", "GET")]
    public class GetConfigPage
    {
    }

    public class WebResourceController : IService
    {
        public object Get(GetHomeButtonScript request)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "NextEpisodePlugin.Configuration.homebutton.js";
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return "Resource not found";
                
                using (var reader = new StreamReader(stream))
                {
                    var content = reader.ReadToEnd();
                    return content;
                }
            }
        }

        public object Get(GetConfigPage request)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "NextEpisodePlugin.Configuration.configPage.html";
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return "Resource not found";
                
                using (var reader = new StreamReader(stream))
                {
                    var content = reader.ReadToEnd();
                    return content;
                }
            }
        }
    }
}