using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Services;
using EDDY.IS.WidgetProvider.Core.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.ComponentModels
{
    public class SmartListingModel : FormsEngineBaseModel, IRenderable  
    {
        private readonly IConfiguration _configuration;
        public Guid PlacementToken { get; set; }
        public string Theme { get; set; }
        public string EddyClickUrl { get; set; }

        public override string FormsEngineServiceUrl => base.TestMode ? _configuration.GetSection("ScriptSources")["BundledQdfJs"] + "?minifyJavascript=false" : _configuration.GetSection("ScriptSources")["BundledQdfJs"];

        public SmartListingModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task Configure(List<VendorWidgetConfig> widgetConfigList)
        {
            VendorWidgetConfig config = widgetConfigList.FirstOrDefault();

            if (config != null)
            {
                this.RenderingDiv = config.WidgetContainerName;
                this.PlacementToken = Guid.Parse(config.SystemSettings[WidgetComponentType.FormsEngine]["placementtoken"]);
                this.CampaignTrackId = Guid.Parse(config.SystemSettings[WidgetComponentType.FormsEngine]["trackid"]);
                this.TemplateId = Convert.ToInt32(config.SystemSettings[WidgetComponentType.FormsEngine]["templateid"]);
                this.ApplicationId = Convert.ToInt32(config.SystemSettings[WidgetComponentType.FormsEngine]["applicationid"]);
                this.Theme = config.SystemSettings[WidgetComponentType.FormsEngine]["theme"];
                this.WidgetRequestGuid = config.ClientWidgetRequest.WidgetRequestGuid;

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("eddyclickurl"))
                    this.EddyClickUrl = config.SystemSettings[WidgetComponentType.FormsEngine]["eddyclickurl"];

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("testmode"))
                    this.TestMode = bool.TryParse(config.SystemSettings[WidgetComponentType.FormsEngine]["testmode"], out bool testMode) ? testMode :false;

                if (!String.IsNullOrWhiteSpace(config.CSSText))
                    this.CSSModel = config.CSSText;

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("buttontext"))
                {
                    string buttonText = config.SystemSettings[WidgetComponentType.FormsEngine]["buttontext"];
                    if (!String.IsNullOrWhiteSpace(buttonText))
                        this.ButtonText = buttonText;
                }

                this.FilterSettings = WidgetSettingsMappingService.Map(WidgetComponentType.FormsEngine, config.SiteSettings);

                if (this.FilterSettings.Any() && this.FilterSettings.ContainsKey("SubSource2") && this.FilterSettings.ContainsKey("LeadSourceUrl"))
                {
                    this.LeadSourceUrl = this.FilterSettings["LeadSourceUrl"];
                    this.SubSource2 = this.FilterSettings["SubSource2"];

                    this.FilterSettings.Remove("LeadSourceUrl");
                    this.FilterSettings.Remove("SubSource2");
                }
            }
            return Task.CompletedTask;
        }

        public async Task<string> RenderAsync(IViewRenderService viewRenderService)
        {
            string renderedJs = "";

            AdServerIncludesModel adServerIncludesModel = new AdServerIncludesModel(_configuration);
            FormsEngineIncludesModel formsEngineIncludesModel = new FormsEngineIncludesModel(_configuration);

            renderedJs = await viewRenderService.RenderToStringAsync(WidgetType.SmartListing, "SmartListingJavascriptIncludes", adServerIncludesModel);
            renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.SmartListing, "SmartListingIncludes", formsEngineIncludesModel);            

            renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.SmartListing, "SmartListing", this);

            return renderedJs;
        }
    }
}
