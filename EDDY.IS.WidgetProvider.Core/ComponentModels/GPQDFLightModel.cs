using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Extensions;
using EDDY.IS.WidgetProvider.Core.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.ComponentModels
{
	public class GPQDFLightModel : IRenderable, ICacheable
	{
		private readonly IConfiguration _configuration;
		private readonly IWidgetRepository _widgetRepository;
		private readonly IQDFService _qdfService;
		private readonly IFileSerializeService _fileSerializeService;

		public List<QDFTemplatePlacement> TemplatePlacements { get; set; }
		public string DestinationURL { get; set; }
		public string JavaScriptContents { get; set; }




		public GPQDFLightModel(IConfiguration configuration, IWidgetRepository widgetRepository, IQDFService qdfService, IFileSerializeService fileSerializeService)
		{
			_configuration = configuration;
			_widgetRepository = widgetRepository;
			_qdfService = qdfService;
			_fileSerializeService = fileSerializeService;
		}

		public string FontAwesome => _configuration.GetSection("StyleSources")["Fontawesome"];
		public string QDFLightCSS => _configuration.GetSection("StyleSources")["QDFLightCSS"];
		public string Select2CSS => _configuration.GetSection("StyleSources")["Select2CSS"];
		public string Select2Js => _configuration.GetSection("ScriptSources")["Select2Js"];

		public string GenerateCacheKey(List<VendorWidgetConfig> widgetConfigList)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var config in widgetConfigList)
			{
				var trackId = DeriveTrackId(config);

				sb.Append(config.WidgetContainerName);
				sb.Append(";trackid=" + trackId);

				foreach (var x in WidgetSettingsMappingService.Map(WidgetComponentType.FormsEngine, config.SiteSettings))
					sb.Append(";" + x.Key + "=" + x.Value);
			}

			return sb.ToString();
		}

		public Task Configure(List<VendorWidgetConfig> widgetConfigList)
		{
			TemplatePlacements = new List<QDFTemplatePlacement>();

			foreach (var config in widgetConfigList)
            {
                QDFTemplatePlacement placement = new QDFTemplatePlacement();

                placement.RenderingDiv = config.WidgetContainerName;
                placement.FilterSettings = WidgetSettingsMappingService.Map(WidgetComponentType.FormsEngine, config.SiteSettings);
                placement.AdditionalSettings = WidgetSettingsMappingService.MapAdditionalSettings(placement.FilterSettings);

                placement.TrackId = DeriveTrackId(config);

                Guid.TryParse(placement.TrackId, out Guid trackid);

                int templateid = 0;
                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("templateid"))
                    int.TryParse(config.SystemSettings[WidgetComponentType.FormsEngine]["templateid"], out templateid);

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("IgnoreTrackId"))
                    if (bool.TryParse(config.SystemSettings[WidgetComponentType.FormsEngine]["IgnoreTrackId"], out bool ignoreTrackIdParsed))
                    {
                        placement.IgnoreTrackId = ignoreTrackIdParsed;
                    };

                placement.Template = _qdfService.GetQDFTemplate(templateid, trackid, placement.FilterSettings, placement.IgnoreTrackId);

                placement.ButtonText = config.SystemSettings[WidgetComponentType.FormsEngine]["buttontext"];

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("adstackwidget"))
                    placement.AdStackVendorWidget = config.SystemSettings[WidgetComponentType.FormsEngine]["adstackwidget"];

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("headertext"))
                    placement.HeaderText = config.SystemSettings[WidgetComponentType.FormsEngine]["headertext"];

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("nofontawesome"))
                    placement.NoFontAwesome = config.SystemSettings[WidgetComponentType.FormsEngine]["nofontawesome"];

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("noqdflightcss"))
                    placement.NoQDFLightCSS = config.SystemSettings[WidgetComponentType.FormsEngine]["noqdflightcss"];

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("noselect2"))
                    placement.NoSelect2 = config.SystemSettings[WidgetComponentType.FormsEngine]["noselect2"];

                if (!string.IsNullOrWhiteSpace(config.CSSText))
                    placement.CSSModel = config.CSSText;


                this.TemplatePlacements.Add(placement);
            }

            JavaScriptContents = _fileSerializeService.GetFileContents("js", "qdf.js").EscapeNewLines();
			return Task.CompletedTask;
		}

        private static string DeriveTrackId(VendorWidgetConfig config)
        {
            bool cookieOverwrite = false;
            if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("checktrackidinsessioncookies"))
                Boolean.TryParse(config.SystemSettings[WidgetComponentType.FormsEngine]["checktrackidinsessioncookies"], out cookieOverwrite);

            
            // TrackId coming from query string url has higher priority
            if (config.SiteSettings.ContainsKey("trackid"))
                return config.SiteSettings["trackid"];
            else if (cookieOverwrite && config.ClientWidgetRequest.TrackId != Guid.Empty)
                return config.ClientWidgetRequest.TrackId.ToString();

			return config.SystemSettings[WidgetComponentType.FormsEngine]["trackid"];
		}

		public async Task<string> RenderAsync(IViewRenderService viewRenderService)
		{
			string renderedJs = "";

			renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.GPQDFLight, "GPQDFLight", this, true);
			return renderedJs;
		}
		
	}
}
