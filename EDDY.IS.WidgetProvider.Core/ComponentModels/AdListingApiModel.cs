using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Extensions;
using EDDY.IS.WidgetProvider.Core.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EDDY.IS.WidgetProvider.Core.ComponentModels
{
    public class AdListingApiModel : IRenderable
    {
        private readonly IConfiguration _configuration;
        private readonly IFileSerializeService _fileSerializeService;

        public List<AdListingApiPlacement> AdPlacementList { get; set; }
        public string AdCSSFile { get; set; }
        public bool LoadExternalResources { get; set; }
        public string JavaScriptContents { get; set; }

        public AdListingApiModel(IConfiguration configuration, IFileSerializeService fileSerializeService, bool loadExternalResources = true)
        {
            _configuration = configuration;
            _fileSerializeService = fileSerializeService;
            LoadExternalResources = loadExternalResources;
            JavaScriptContents = String.Empty;
        }

        public string AdListingApiFontFace => _configuration.GetSection("StyleSources")["AdListingApiFontFace"];

        public Task Configure(List<VendorWidgetConfig> widgetConfigList)
        {
            this.AdPlacementList = new List<AdListingApiPlacement>();

            foreach (var config in widgetConfigList)
            {
                AdListingApiPlacement placement = new AdListingApiPlacement();
                bool cookieOverwrite = false;
                if (config.SystemSettings[WidgetComponentType.AdServer].ContainsKey("checktrackidinsessioncookies"))
                    Boolean.TryParse(config.SystemSettings[WidgetComponentType.AdServer]["checktrackidinsessioncookies"], out cookieOverwrite);
                

                placement.RenderingDiv = config.WidgetContainerName;

                placement.FilterSettings = WidgetSettingsMappingService.Map(ListingApiType.AdListingApi, config.SiteSettings);

                if (config.SystemSettings[WidgetComponentType.AdServer].ContainsKey("nofontface"))
                    placement.NoFontFace = config.SystemSettings[WidgetComponentType.AdServer]["nofontface"];

                if (!String.IsNullOrWhiteSpace(config.CSSText))
                    placement.CSSModel = config.CSSText;

                if (config.SystemSettings[WidgetComponentType.AdServer].ContainsKey("creativeid"))
                    placement.Creative = (AdListingApiCreative)Convert.ToInt32(config.SystemSettings[WidgetComponentType.AdServer]["creativeid"]);
                else
                    placement.Creative = AdListingApiCreative.Default;

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("noadlistingapicss"))
                    placement.NoAdListingAPI = config.SystemSettings[WidgetComponentType.FormsEngine]["noadlistingapicss"];

                _ = Guid.TryParse(config.SystemSettings[WidgetComponentType.AdServer]["trackid"], out Guid trackId);
                if (cookieOverwrite && config.ClientWidgetRequest.TrackId != Guid.Empty)
                    trackId = config.ClientWidgetRequest.TrackId;

                _ = Guid.TryParse(config.SystemSettings[WidgetComponentType.AdServer]["placementtoken"], out Guid placementToken);

                AdListingApiService apiService = new AdListingApiService(_configuration);

                AdListingApiResponse apiResponse = apiService.GetApiResponse(
                    placement.FilterSettings,
                    placementToken,
                    trackId,
                    config.ClientWidgetRequest,
                    config.WidgetContainerName);

                if(apiResponse != null && (String.IsNullOrWhiteSpace(apiResponse.Error) || apiResponse.Error == "0") && apiResponse.Institutions != null && apiResponse.Institutions.Any())
                {
                    placement.AdItemList = MapServiceResponse(apiResponse);
                    this.AdPlacementList.Add(placement);
                } 
            }

            if (this.AdPlacementList.Any(a => a.Creative == AdListingApiCreative.Default))
                this.AdCSSFile = _configuration.GetSection("StyleSources")["AdStackLightDefaultCSS"];
            else if (this.AdPlacementList.Any(a => a.Creative == AdListingApiCreative.Project8Style))
                this.AdCSSFile = _configuration.GetSection("StyleSources")["AdStackLightP8CSS"];
            else if (this.AdPlacementList.Any(a => a.Creative == AdListingApiCreative.QDFAdListingStyle))
                this.AdCSSFile = _configuration.GetSection("StyleSources")["AdStackLightQDFCSS"];

            if (LoadExternalResources)
            {
                JavaScriptContents = _fileSerializeService.GetFileContents("js", "adlisting.js").EscapeNewLines();
            }

            return Task.CompletedTask;
        }

        public async Task<string> RenderAsync(IViewRenderService viewRenderService)
        {
            string renderedJs = "";

			if (this.AdPlacementList != null && this.AdPlacementList.Any())
				renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.AdListingApi, "AdListingApi", this, true);

			return renderedJs;
        }

        public List<AdListingApiAdItem> MapServiceResponse(AdListingApiResponse apiResponse)
        {
            List<AdListingApiAdItem> adItemList = new List<AdListingApiAdItem>();

            foreach(var adListing in apiResponse.Institutions)
            {
                AdListingApiAdItem item = new AdListingApiAdItem();

                item.InstitutionName = adListing.InstitutionName;
                item.LogoUrl = adListing.LogoMediumImage;
                item.LargeLogoUrl = adListing.LogoLargeImage;
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
