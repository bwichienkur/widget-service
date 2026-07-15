using Microsoft.Extensions.Configuration;

namespace EDDY.IS.WidgetProvider.Core.Settings
{
    public class AdServerIncludesModel
    {
        private readonly IConfiguration _configuration;
        public AdServerIncludesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string AdAggregatorBundle => _configuration.GetSection("ScriptSources")["AdAggregator"];
    }
}
