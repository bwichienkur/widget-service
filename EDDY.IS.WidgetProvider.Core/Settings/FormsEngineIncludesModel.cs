using Microsoft.Extensions.Configuration;

namespace EDDY.IS.WidgetProvider.Core.Settings
{
    public class FormsEngineIncludesModel
    {
        private readonly IConfiguration _configuration;
        public FormsEngineIncludesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string JQueryValidate => _configuration.GetSection("ScriptSources")["JqueryValidate"];
        public string BootstrapCSS => _configuration.GetSection("StyleSources")["Bootstrap"];
        public string FontAwesome => _configuration.GetSection("StyleSources")["Fontawesome"];
        public string AnimateCSS => _configuration.GetSection("StyleSources")["AnimateCSS"];
        public string BoostrapGridCSS => _configuration.GetSection("StyleSources")["BootstrapGrid"];
        public string AdListingAPIFont => _configuration.GetSection("StyleSources")["AdListingApiFontFace"];

        public string NoFontAwesome { get; set; }
        public string NoAnimate { get; set; }
        public string NoBeforeModal { get; set; }
        public string IgnoreBootstrap { get; set; }
    }
}
