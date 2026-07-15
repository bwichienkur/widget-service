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
    public class WizardFormModel : FormsEngineBaseModel, IRenderable
    {
        private readonly IConfiguration _configuration;
        private readonly WidgetType _widgetType;
        public string Theme { get; set; }
        public int? InstitutionId { get; set; }
        public string InstitutionName { get; set; }
        public string IgnoreBootstrap { get; set; }
        public string NoBundleCSS { get; set; }

        public string NoFontAwesome { get; set; }
        public string NoAnimate { get; set; }
        public string NoBeforeModal { get; set; }

        public override string FormsEngineServiceUrl => base.TestMode ? _configuration.GetSection("ScriptSources")["BundledWizardJs"] + "?minifyJavascript=false" : _configuration.GetSection("ScriptSources")["BundledWizardJs"];
        public WizardFormModel(IConfiguration configuration, WidgetType widgetType)
        {
            _configuration = configuration;
            _widgetType = widgetType;
        }
        public Task Configure(List<VendorWidgetConfig> widgetConfigList)
        {
            VendorWidgetConfig config = widgetConfigList.FirstOrDefault();

            if (config != null)
            {
                this.RenderingDiv = config.WidgetContainerName;
                this.CampaignTrackId = Guid.Parse(config.SystemSettings[WidgetComponentType.FormsEngine]["trackid"]);
                this.TemplateId = Convert.ToInt32(config.SystemSettings[WidgetComponentType.FormsEngine]["templateid"]);
                this.WidgetRequestGuid = config.ClientWidgetRequest.WidgetRequestGuid;

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("testmode"))
                    this.TestMode = bool.TryParse(config.SystemSettings[WidgetComponentType.FormsEngine]["testmode"], out bool testMode) ? testMode : false;
                    
                

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("theme"))
                    this.Theme = config.SystemSettings[WidgetComponentType.FormsEngine]["theme"];


                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("nobundlecss"))
                    this.NoBundleCSS = config.SystemSettings[WidgetComponentType.FormsEngine]["nobundlecss"];


                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("nobootstrap"))
                    this.IgnoreBootstrap = config.SystemSettings[WidgetComponentType.FormsEngine]["nobootstrap"];

                if (!String.IsNullOrWhiteSpace(config.CSSText))
                    this.CSSModel = config.CSSText;

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("nofontawesome"))
                    this.NoFontAwesome = config.SystemSettings[WidgetComponentType.FormsEngine]["nofontawesome"];

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("noanimatecss"))
                    this.NoAnimate = config.SystemSettings[WidgetComponentType.FormsEngine]["noanimatecss"];

                this.FilterSettings = WidgetSettingsMappingService.Map(WidgetComponentType.FormsEngine, config.SiteSettings);

                if(this.FilterSettings.Any() && this.FilterSettings.ContainsKey("SubSource2") && this.FilterSettings.ContainsKey("LeadSourceUrl"))
                {
                    this.LeadSourceUrl = this.FilterSettings["LeadSourceUrl"];
                    this.SubSource2 = this.FilterSettings["SubSource2"];

                    this.FilterSettings.Remove("LeadSourceUrl");
                    this.FilterSettings.Remove("SubSource2");
                }

                if (_widgetType == WidgetType.ProgramForm)
                {
                    if (this.FilterSettings.Any() && this.FilterSettings.ContainsKey("InstitutionId") && this.FilterSettings.ContainsKey("InstitutionName"))
                    {
                        this.InstitutionId = Convert.ToInt32(this.FilterSettings["InstitutionId"]);
                        this.InstitutionName = this.FilterSettings["InstitutionName"];

                        this.FilterSettings.Remove("InstitutionId");
                        this.FilterSettings.Remove("InstitutionName");
                    }
                    else
                    {
                        this.InstitutionId = Convert.ToInt32(config.SystemSettings[WidgetComponentType.FormsEngine]["institutionid"]);
                        this.InstitutionName = config.SystemSettings[WidgetComponentType.FormsEngine]["institutionname"];
                    }
                }
            }
            return Task.CompletedTask;
        }

        public async Task<string> RenderAsync(IViewRenderService viewRenderService)
        {
            string renderedJs = "";

            FormsEngineIncludesModel formsEngineIncludesModel = new FormsEngineIncludesModel(_configuration);
            formsEngineIncludesModel.NoFontAwesome = this.NoFontAwesome;
            formsEngineIncludesModel.NoAnimate = this.NoAnimate;
            formsEngineIncludesModel.NoBeforeModal = this.NoBeforeModal;
            formsEngineIncludesModel.IgnoreBootstrap = this.IgnoreBootstrap;

            renderedJs = await viewRenderService.RenderToStringAsync(WidgetType.WizardForm, "WizardFormJavascriptIncludes", formsEngineIncludesModel);

            if (_widgetType == WidgetType.ProgramForm)
                renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.WizardForm, "ProgramForm", this);
            else if (_widgetType == WidgetType.WizardForm)
                renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.WizardForm, "WizardForm", this);
            else
                throw new Exception("Cannot render widget of type: " + _widgetType.ToString());

            return renderedJs;
        }
    }
}
