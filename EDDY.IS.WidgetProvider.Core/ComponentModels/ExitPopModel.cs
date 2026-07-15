using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Extensions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.ComponentModels
{
    public class ExitPopModel : IRenderable
    {
        public string TargetUrl { get; set; }
        public string Container { get; set; }
        public string JQueryModalJs { get; set; }
        public string JQueryModalCss { get; set; }
        public string JQuerySelector { get; set; }
        public string JQueryModalBefore { get; set; }
        public string AdCSSFile { get; set; }
        private readonly WidgetRequest widgetRequest;
        private readonly IConfiguration configuration;
        public VendorWidgetConfig VendorWidgetConfig { get; set; }
        public IFileSerializeService fileSerializeService { get; set; }

        public ExitPopModel(IConfiguration configuration, IFileSerializeService fileSerializeService,WidgetRequest widgetRequest)
        {
            this.widgetRequest = widgetRequest;
            this.configuration = configuration;
            this.fileSerializeService = fileSerializeService;

            if (!widgetRequest.IsModalLoad)
            {
                JQueryModalJs = configuration.GetSection("ScriptSources")["JqueryModal"];
                JQueryModalCss = configuration.GetSection("StyleSources")["JQueryModalCSS"].Replace("##BASEURL##", configuration.GetSection("WidgetJsReplaceValues")["BASEURL"]);
            }
            JQueryModalBefore = configuration.GetSection("StyleSources")["JQueryModalBeforeCSS"].Replace("##BASEURL##", configuration.GetSection("WidgetJsReplaceValues")["BASEURL"]);
        }

        public string BootstrapCSS => configuration.GetSection("StyleSources")["Bootstrap"];

        public async Task<string> RenderAsync(IViewRenderService viewRenderService)
        {          
            string renderedJs = string.Empty;
            renderedJs += fileSerializeService.GetFileContents("scripts/js", "AdAggregator.js").Replace("&#xD", string.Empty).Replace("&#xA", string.Empty).Replace("//# sourceMappingURL=AdAggregator.js.map",string.Empty).Replace("/** @class */",string.Empty).Replace("//@ts-ignore",string.Empty).Replace("// @ts-ignore", string.Empty).Replace("##SERVICEURL##", configuration.GetSection("WidgetJsReplaceValues")["SERVICEURL"]);
            renderedJs += fileSerializeService.GetFileContents("scripts/js", "MouseTracker.js").Replace("&#xD", string.Empty).Replace("&#xA", string.Empty).Replace("//# sourceMappingURL=MouseTracker.js.map", string.Empty).Replace("/** @class */", string.Empty).Replace("//@ts-ignore", string.Empty).Replace("// @ts-ignore", string.Empty);
            renderedJs += (await viewRenderService.RenderToStringAsync(WidgetType.ExitPop, "ExitPopDependencies", this, true)).Replace("&#xD", string.Empty).Replace("&#xA", string.Empty);
            renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.ExitPop, "ExitPop", this, true);

            renderedJs = renderedJs.EscapeNewLines();

            return renderedJs;
        }

        public Task Configure(List<VendorWidgetConfig> widgetConfigList)
        {
            var config = widgetConfigList.FirstOrDefault();

            if (config != null)
            {
                AdCSSFile = config.CSSText;
                VendorWidgetConfig = config;
                Container = config.WidgetContainerName;
                TargetUrl = config.SystemSettings[WidgetComponentType.Custom]["targeturl"];
                JQuerySelector = config.SystemSettings[WidgetComponentType.Custom].ContainsKey("jqueryselector") ? config.SystemSettings[WidgetComponentType.Custom]["jqueryselector"] : "a"; 
            }
            return Task.CompletedTask;
        }

        public List<AdListingApiAdItem> MapServiceResponse(AdListingApiResponse apiResponse)
        {
            List<AdListingApiAdItem> adItemList = new List<AdListingApiAdItem>();

            foreach (var adListing in apiResponse.Institutions)
            {
                AdListingApiAdItem item = new AdListingApiAdItem();

                item.InstitutionName = adListing.InstitutionName;
                item.LogoUrl = adListing.LogoMediumImage;
                item.ClickUrl = adListing.ClickThroughUrl;
                item.SmallLogoUrl = adListing.LogoSmallImage;
                item.Description = adListing.Description;
                item.IsLeadUrl = adListing.IsLeadUrl;
                if (adListing.Programs != null && adListing.Programs.Any())
                    item.AdProgramNameList = adListing.Programs.Select(p => p.Name).ToList();

                adItemList.Add(item);
            }

            return adItemList;
        }
    }
}