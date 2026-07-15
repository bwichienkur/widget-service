using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Services;
using EDDY.IS.WidgetProvider.Core.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.ComponentModels
{
    public class AdStackSoloModel : IRenderable
    {
        private const WidgetComponentType _widgetComponentType = WidgetComponentType.AdServer;
        private readonly IConfiguration _configuration;
        public List<AdPlacement> AdPlacementList { get; set; }
        public string BootstrapCSS => _configuration.GetSection("StyleSources")["Bootstrap"];

        public AdStackSoloModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> RenderAsync(IViewRenderService viewRenderService)
        {
            string renderedJs = "";

            AdServerIncludesModel adServerIncludesModel = new AdServerIncludesModel(_configuration);

            renderedJs = await viewRenderService.RenderToStringAsync(WidgetType.AdStackSolo, "AdStackSoloJavascriptIncludes", adServerIncludesModel);
            renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.AdStackSolo, "AdStackSolo", this);

            return renderedJs;
        }

        public Task Configure(List<VendorWidgetConfig> widgetConfigList)
        {
            this.AdPlacementList = new List<AdPlacement>();

            foreach (var config in widgetConfigList)
            {
                AdPlacement placement = new AdPlacement();

                placement.RenderingDiv = config.WidgetContainerName;
                placement.PlacementToken = Guid.Parse(config.SystemSettings[_widgetComponentType]["placementtoken"]);
                placement.CampaignTrackId = Guid.Parse(config.SystemSettings[_widgetComponentType]["trackid"]);
                placement.WidgetRequestGuid = config.ClientWidgetRequest.WidgetRequestGuid;

                placement.FilterSettings = WidgetSettingsMappingService.Map(_widgetComponentType, config.SiteSettings);

                if (config.SystemSettings[_widgetComponentType].ContainsKey("eddyclickurl"))
                    placement.FilterSettings.Add("eddyclickurl", config.SystemSettings[_widgetComponentType]["eddyclickurl"]);

                if (!String.IsNullOrWhiteSpace(config.CSSText))
                    placement.CSSModel = config.CSSText;

                this.AdPlacementList.Add(placement);
            }
            return Task.CompletedTask;
        }
    }
}
