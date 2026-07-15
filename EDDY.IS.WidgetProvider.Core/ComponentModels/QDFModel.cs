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
    public class QDFInstance : FormsEngineBaseModel
    {
        private readonly IConfiguration _configuration;
        public override string FormsEngineServiceUrl => base.TestMode ? _configuration.GetSection("ScriptSources")["BundledQdfPluginJs"] + "?minifyJavascript=false" : _configuration.GetSection("ScriptSources")["BundledQdfPluginJs"];
        
        public string TargetUrl { get; set; }
        public string SubId { get; set; }
        
        public QDFInstance(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }

    public class QDFModel : IRenderable
    {
        private readonly IConfiguration _configuration;

        public List<QDFInstance> QDFList { get; set; }

        public QDFModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task Configure(List<VendorWidgetConfig> widgetConfigList)
        {
            this.QDFList = new List<QDFInstance>();

            foreach (var config in widgetConfigList)
            {
                QDFInstance qdf = new QDFInstance(_configuration);

                qdf.RenderingDiv = config.WidgetContainerName;
                qdf.CampaignTrackId = Guid.Parse(config.SystemSettings[WidgetComponentType.FormsEngine]["trackid"]);
                qdf.TemplateId = Convert.ToInt32(config.SystemSettings[WidgetComponentType.FormsEngine]["templateid"]);
                qdf.TargetUrl = config.SystemSettings[WidgetComponentType.FormsEngine]["targeturl"];
                qdf.WidgetRequestGuid = config.ClientWidgetRequest.WidgetRequestGuid;

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("testmode"))
                    qdf.TestMode = bool.TryParse(config.SystemSettings[WidgetComponentType.FormsEngine]["testmode"], out bool testMode) 
                        ?  testMode 
                        : false;

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("buttontext"))
                {
                    string buttonText = config.SystemSettings[WidgetComponentType.FormsEngine]["buttontext"];
                    if (!String.IsNullOrWhiteSpace(buttonText))
                        qdf.ButtonText = buttonText;
                }

                qdf.FilterSettings = WidgetSettingsMappingService.Map(WidgetComponentType.FormsEngine, config.SiteSettings);

                if (!String.IsNullOrWhiteSpace(config.CSSText))
                    qdf.CSSModel = config.CSSText;
                

                if (qdf.FilterSettings.Any() && qdf.FilterSettings.ContainsKey("SubSource2") && qdf.FilterSettings.ContainsKey("LeadSourceUrl"))
                {
                    qdf.LeadSourceUrl = qdf.FilterSettings["LeadSourceUrl"];
                    qdf.SubSource2 = qdf.FilterSettings["SubSource2"];

                    qdf.FilterSettings.Remove("LeadSourceUrl");
                    qdf.FilterSettings.Remove("SubSource2");
                }
                if (qdf.FilterSettings.Any() && qdf.FilterSettings.ContainsKey("sub_1"))
                {
                    qdf.SubId = qdf.FilterSettings["sub_1"];
                    qdf.FilterSettings.Remove("sub_1");
                }

                this.QDFList.Add(qdf);
            }
            return Task.CompletedTask;
        }

        public async Task<string> RenderAsync(IViewRenderService viewRenderService)
        {
            string renderedJs = "";

            FormsEngineIncludesModel formsEngineIncludesModel = new FormsEngineIncludesModel(_configuration);

            renderedJs = await viewRenderService.RenderToStringAsync(WidgetType.QDF, "QDFIncludes", formsEngineIncludesModel);

            renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.QDF, "QDF", this);

            return renderedJs;
        }
    }
}
