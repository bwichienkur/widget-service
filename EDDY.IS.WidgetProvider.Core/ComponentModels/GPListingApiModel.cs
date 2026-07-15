using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Extensions;
using EDDY.IS.WidgetProvider.Core.Interfaces;
using EDDY.IS.WidgetProvider.Core.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.ComponentModels
{
    public class GPListingApiModel : IRenderable
    {
        private readonly IConfiguration _configuration;
        private readonly IGPListingApiService _listingService;
        private readonly IFileSerializeService _fileSerializeService;
        private readonly IFESessionRedisService _feSessionRedisService;

        public List<GPListingApiPlacement> AdPlacementList { get; set; }

        public string AdCSSFile { get; set; }

        public bool LoadExternalResources { get; set; }

        public string JavaScriptContents { get; set; }

        public bool PassiveLoad { get; set; }

        public bool UpdateWidget { get; set; }

        public string AdListingApiFontFace => _configuration.GetSection("StyleSources")["AdListingApiFontFace"];

        public GPListingApiModel(
            IConfiguration configuration, 
            IGPListingApiService listingService, 
            IFileSerializeService fileSerializeService, 
            IFESessionRedisService feSessionRedisService, 
            bool loadExternalResources = true)
        {
            _configuration = configuration;
            _listingService = listingService;
            LoadExternalResources = loadExternalResources;
            _fileSerializeService = fileSerializeService;
            _feSessionRedisService = feSessionRedisService;
            JavaScriptContents = string.Empty;
        }

        public async Task Configure(List<VendorWidgetConfig> widgetConfigList)
        {
            this.AdPlacementList = new List<GPListingApiPlacement>();

            foreach (var config in widgetConfigList)
            {
                var placement = new GPListingApiPlacement();
                bool cookieOverwrite = false;
                string formsEngineSessionId = string.Empty;

                if (config.SiteSettings.ContainsKey("fesessionid"))
                    formsEngineSessionId = config.SiteSettings["fesessionid"];

                if (config.SystemSettings[WidgetComponentType.AdServer].ContainsKey("checktrackidinsessioncookies"))
                    bool.TryParse(config.SystemSettings[WidgetComponentType.AdServer]["checktrackidinsessioncookies"], out cookieOverwrite);
                
                placement.RenderingDiv = config.WidgetContainerName;

                // ADD FormsEngine Form fields from FE session if available
                var guidFESession = Guid.Empty;
                if (!string.IsNullOrEmpty(formsEngineSessionId) && Guid.TryParse(formsEngineSessionId,out guidFESession))
                {
                    var feDictionary = _feSessionRedisService.GetFormsEngineSession(formsEngineSessionId);
                    if(feDictionary != null)
                    {
                        config.SiteSettings = MergeDictionaries(feDictionary, config.SiteSettings);
                    }
                }

                placement.FilterSettings = WidgetSettingsMappingService.Map(ListingApiType.GPListingApi, config.SiteSettings);

               
                if (config.SystemSettings[WidgetComponentType.AdServer].ContainsKey("nofontface"))
                    placement.NoFontFace = config.SystemSettings[WidgetComponentType.AdServer]["nofontface"];

                if (!string.IsNullOrWhiteSpace(config.CSSText))
                    placement.CSSModel = config.CSSText;

                if (config.SystemSettings[WidgetComponentType.AdServer].ContainsKey("creativeid"))
                    placement.Creative = (GPListingApiCreative)Convert.ToInt32(config.SystemSettings[WidgetComponentType.AdServer]["creativeid"]);
                else
                    placement.Creative = GPListingApiCreative.Default;

                if (config.SystemSettings[WidgetComponentType.FormsEngine].ContainsKey("noadlistingapicss"))
                    placement.NoAdListingAPI = config.SystemSettings[WidgetComponentType.FormsEngine]["noadlistingapicss"];

                _ = Guid.TryParse(config.SystemSettings[WidgetComponentType.AdServer]["trackid"], out Guid trackId);
                if (cookieOverwrite && config.ClientWidgetRequest.TrackId != Guid.Empty)
                    trackId = config.ClientWidgetRequest.TrackId;

                if (config.SiteSettings != null && config.SiteSettings.ContainsKey("passiveload"))
                    this.PassiveLoad = config.SiteSettings["passiveload"].ToLower() == "true";

                this.UpdateWidget = config.ClientWidgetRequest.UpdateWidget;

                if(placement.Creative == GPListingApiCreative.CustomCreative)
                {
                    await LoadCreativeAds(placement, trackId, config);
                }
                else
                {
                    await LoadObjectAds(placement, trackId, config);
                }

                this.AdPlacementList.Add(placement);
            }

            if (this.AdPlacementList.Any(a => a.Creative == GPListingApiCreative.Default))
                this.AdCSSFile = _configuration.GetSection("StyleSources")["AdStackLightQDFCSS"];

            if (!UpdateWidget)
            {
                JavaScriptContents = _fileSerializeService.GetFileContents("js", "adlisting.js").EscapeNewLines();
            }
        }

        private async Task LoadObjectAds(GPListingApiPlacement placement, Guid trackId, VendorWidgetConfig config)
        {
            GPListingObjectResponse apiResponse = null;

            if (!PassiveLoad)
            {
               apiResponse = await _listingService.GetApiResponse<GPListingObjectResponse>(placement.FilterSettings,
                    Guid.Parse(config.SystemSettings[WidgetComponentType.AdServer]["placementtoken"]),
                    trackId,
                    config.ClientWidgetRequest,
                    config.WidgetContainerName);
            }

            if (apiResponse != null && (string.IsNullOrWhiteSpace(apiResponse.Error) || apiResponse.Error == "0")
                    && apiResponse.Ads != null && apiResponse.Ads.Any())
            {
                placement.AdItemList = MapServiceResponse(apiResponse);
                placement.ImpressionPixels = apiResponse.Pixels;
                placement.IsTesting = apiResponse.IsTesting;
            }
        }

        private async Task LoadCreativeAds(GPListingApiPlacement placement, Guid trackId, VendorWidgetConfig config)
        {
            GPListingCreativeResponse apiResponse = null;

            if (!PassiveLoad)
            {
                apiResponse = await _listingService.GetApiResponse<GPListingCreativeResponse>(placement.FilterSettings,
                     Guid.Parse(config.SystemSettings[WidgetComponentType.AdServer]["placementtoken"]),
                     trackId,
                     config.ClientWidgetRequest,
                     config.WidgetContainerName);
            }

            if (apiResponse != null && (string.IsNullOrWhiteSpace(apiResponse.Error) || apiResponse.Error == "0")
                    && !string.IsNullOrWhiteSpace(apiResponse.Ads))
            {
                placement.RenderedAds = apiResponse.Ads.Replace("\"", "\\\"");
                placement.ImpressionPixels = apiResponse.Pixels;
                placement.IsTesting = apiResponse.IsTesting;
            }
        }

        public async Task<string> RenderAsync(IViewRenderService viewRenderService)
        {
            string renderedJs = "";

            if (this.AdPlacementList != null && this.AdPlacementList.Any())
                renderedJs += await viewRenderService.RenderToStringAsync(WidgetType.GPListingApi, "GPListingApi", this, true);

            return renderedJs;
        }

        private Dictionary<string, string> MergeDictionaries(Dictionary<string, string> feDictionary, Dictionary<string, string> siteSettings)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if(feDictionary != null)
                foreach(string key in feDictionary.Keys)
                {
                    result.TryAdd(key, feDictionary[key]);
                }

            if(siteSettings != null)
                foreach(string key in siteSettings.Keys)
                {
                    if(!result.ContainsKey(key))
                        result.TryAdd(key, siteSettings[key]);
                }

            return result;
        }

        private List<AdListingGPApiAdItem> MapServiceResponse(GPListingObjectResponse apiResponse)
        {
            var adItemList = new List<AdListingGPApiAdItem>();

            foreach(var ad in apiResponse.Ads)
            {
                adItemList.Add(new AdListingGPApiAdItem 
                { 
                     ClickThroughUrl = ad.ClickThroughUrl,
                     DisplayUrl = ad.DisplayUrl,
                     Description = ad.Description,
                     Header = ad.Header,
                     InstitutionName = ad.InstitutionName,
                     LogoLargeImage = ad.LogoLargeImage,
                     LogoMediumImage = ad.LogoMediumImage,
                     LogoSmallImage = ad.LogoSmallImage,
                     PopularPrograms = ad.PopularPrograms
                });
            }

            return adItemList;
        }

        
    }
}
