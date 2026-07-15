using EDDY.IS.WidgetProvider.Core;
using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Interfaces;
using EDDY.IS.WidgetProvider.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Service.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExitPopController : Controller
    {
        private readonly ILogger<ExitPopController> logger;
        private readonly IConfiguration configuration;
        private readonly IWidgetRepository widgetRepository;
        private readonly IViewRenderService _viewRenderService;
        private readonly ICampaignRepository campaignRepository;
        private readonly IGPListingApiService _listingService;

        public ExitPopController(
            ILogger<ExitPopController> logger, 
            IConfiguration configuration, 
            IWidgetRepository widgetRepository, 
            IViewRenderService viewRenderService, 
            ICampaignRepository campaignRepository,
            IGPListingApiService listingService)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.widgetRepository = widgetRepository;
            this._viewRenderService = viewRenderService;
            this.campaignRepository = campaignRepository;
            _listingService = listingService;
        }

        [HttpPost]
        public async Task<bool> CanRender([FromBody]string trackId)
        {
            try
            {
                var guidTrackId = new Guid(trackId);
                var canRender = await campaignRepository.HasExitPop(guidTrackId);
                return canRender;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return false;
            }
        }

        [HttpPost]
        public async Task<string> RenderAd([FromBody]WidgetRequest widgetRequest)
        {
            try
            {
                string adList = "";

                if (widgetRequest != null && widgetRequest.ContainerList != null && widgetRequest.ContainerList.Any())
                {
                    widgetRequest.IPAddress = GetClientIPAddress();
                    widgetRequest.WidgetRequestGuid = Guid.NewGuid();

                    VendorWidgetConfig widgetConfig = GetWidgetInformation(widgetRequest.ContainerList, widgetRequest.VendorToken);
                    if (widgetConfig.WidgetType == WidgetType.GPExitPop)
                    {
                        adList = await RenderGPAdsAsync(widgetRequest, widgetConfig);
                    }
                    else if (widgetConfig.WidgetType == WidgetType.ExitPop)
                    {
                        adList = await RenderAdsAsync(widgetRequest, widgetConfig);
                    }
                }

                return adList;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                //TODO: This is dangerous
                return ex.Message;
            }
        }

        [HttpPost]
        public string GetWidgetTrackId(WidgetRequest widgetRequest)
        {
            try
            {
                var widgetTrackId = WidgetTrackId(widgetRequest);
                return widgetTrackId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return ex.Message;
            }
        }

        private async Task<string> RenderAdsAsync(WidgetRequest widgetRequest, VendorWidgetConfig widgetConfig)
        {
            try
            {
                var placement = new AdListingApiPlacement();

                var apiService = new AdListingApiService(configuration);

                placement.RenderingDiv = widgetConfig.WidgetContainerName;
                placement.FilterSettings = new Dictionary<string, string>();

                var cleanedFilters = CleanFilters(widgetRequest.FilterFields,widgetRequest);

                foreach (var filter in cleanedFilters)
                {
                    if (!placement.FilterSettings.ContainsKey(filter.Key))
                        placement.FilterSettings[filter.Key] = filter.Value;
                }

                placement.FilterSettings.Add("transactionId", Guid.NewGuid().ToString());
                var apiResponse = apiService.GetApiResponse(placement.FilterSettings, Guid.Parse(widgetConfig.SystemSettings[WidgetComponentType.Custom]["placementtoken"]), widgetRequest.TrackId, widgetRequest, widgetConfig.WidgetContainerName);
                if (apiResponse != null && (string.IsNullOrWhiteSpace(apiResponse.Error) || apiResponse.Error == "0") && apiResponse.Institutions != null && apiResponse.Institutions.Any())
                {
                    placement.AdItemList = MapServiceResponse(apiResponse);
                }

                if (placement.AdItemList != null && placement.AdItemList.Any()) 
                {
                    var htlm = await _viewRenderService.RenderToStringAsync(WidgetType.ExitPop, "default", placement);
                    return htlm;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return ex.Message;
            }
        }

        private async Task<string> RenderGPAdsAsync(WidgetRequest widgetRequest, VendorWidgetConfig widgetConfig)
        {
            try
            {
                widgetConfig.ClientWidgetRequest = widgetRequest;

                var placement = new GPListingApiPlacement();
                placement.RenderingDiv = widgetConfig.WidgetContainerName;
                placement.FilterSettings = WidgetSettingsMappingService.Map(ListingApiType.GPListingApi, widgetRequest.FilterFields);
                Guid trackId = Guid.Parse(widgetConfig.SystemSettings[WidgetComponentType.AdServer]["trackid"]);
                Guid placementToken = Guid.Parse(widgetConfig.SystemSettings[WidgetComponentType.AdServer]["placementtoken"]);

                var apiResponse = await _listingService.GetApiResponse<GPListingObjectResponse>(placement.FilterSettings,
                    placementToken,
                    trackId,
                    widgetConfig.ClientWidgetRequest,
                    widgetConfig.WidgetContainerName);

                if (apiResponse != null && (string.IsNullOrWhiteSpace(apiResponse.Error) || apiResponse.Error == "0")
                    && apiResponse.Ads != null && apiResponse.Ads.Any())
                {
                    placement.AdItemList = MapServiceGPResponse(apiResponse);
                    placement.ImpressionPixels = apiResponse.Pixels;
                    placement.IsTesting = apiResponse.IsTesting;

                    if (placement.AdItemList != null && placement.AdItemList.Any())
                    {
                        string htlm = await _viewRenderService.RenderToStringAsync(WidgetType.GPExitPop, "default", placement);
                        return htlm;
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return ex.Message;
            }
        }

        private string WidgetTrackId(WidgetRequest widgetRequest)
        {
            try
            {
                var dicWidgetConfigs = new Dictionary<WidgetType, List<VendorWidgetConfig>>();

                if (widgetRequest != null && widgetRequest.ContainerList != null && widgetRequest.ContainerList.Where(cl => cl.ContainerName.Contains("Exit")).Any())
                {
                    var config = widgetRepository.GetWidgetConfig(widgetRequest.ContainerList.Where(cl => cl.ContainerName.Contains("Exit")).First().ContainerName, widgetRequest.VendorToken);
                    return Guid.Parse(config.SystemSettings[WidgetComponentType.Custom]["trackid"]).ToString();
                }

                return Guid.Empty.ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return ex.Message;
            }
        }

        private List<AdListingApiAdItem> MapServiceResponse(AdListingApiResponse apiResponse)
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

        private List<AdListingGPApiAdItem> MapServiceGPResponse(GPListingObjectResponse apiResponse)
        {
            var adItemList = new List<AdListingGPApiAdItem>();

            foreach (var ad in apiResponse.Ads)
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

        private VendorWidgetConfig GetWidgetInformation(List<ContainerData> containers, Guid vendorToken)
        {
            try
            {
                var vendorWidgetConfigList = new List<VendorWidgetConfig>();

                foreach (var container in containers)
                    vendorWidgetConfigList.Add(widgetRepository.GetWidgetConfig(container.ContainerName, vendorToken));

                var exitPopWidgetCount = vendorWidgetConfigList.Count(vw => vw != null && (vw.WidgetType == WidgetType.ExitPop || vw.WidgetType == WidgetType.GPExitPop));
                if (exitPopWidgetCount == 0 || exitPopWidgetCount > 1)
                    throw new Exception("Check configuration either no ExitPop widget is configured or more than one is present in the page");

                return vendorWidgetConfigList.Where(vw => vw != null && (vw.WidgetType == WidgetType.ExitPop || vw.WidgetType == WidgetType.GPExitPop)).First();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private string GetClientIPAddress()
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_VIA")))
                {
                    if (!string.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_TRUE_CLIENT_IP")))
                        ipAddress = HttpContext.Request.HttpContext.GetServerVariable("HTTP_TRUE_CLIENT_IP");
                    else if (!string.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")) && HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").ToLower() != "unknown")
                        ipAddress = !HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Contains(",") ? HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR") : HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Split(',').Last();
                }
                else if (!string.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")) && HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").ToLower() != "unknown")
                    ipAddress = !HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Contains(",") ? HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR") : HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Split(',').Last();
                else if (!string.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("REMOTE_ADDR")))
                    ipAddress = HttpContext.Request.HttpContext.GetServerVariable("REMOTE_ADDR");
            }
            catch { }

            if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(':'))
                ipAddress = ipAddress.Substring(0, ipAddress.LastIndexOf(':'));

            return ipAddress;
        }

        private Dictionary<string,string> CleanFilters(Dictionary<string, string> filters,WidgetRequest widgetRequest)
        {
            var cleanedFilters = new Dictionary<string, string>();


            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    switch (filter.Key)
                    {
                        case "age":
                            cleanedFilters.Add("age", filter.Value);
                            break;
                        case "formStep":
                            cleanedFilters.Add("formStep", filter.Value);
                            break;
                        case "workflowStep":
                            cleanedFilters.Add("workflowStep", filter.Value);
                            break;                   
                        case "categories_selections":
                            cleanedFilters.Add("categories", filter.Value);
                            break;
                        case "subcategories_selections":
                            cleanedFilters.Add("subcategories", filter.Value);
                            break;
                        case "specialties_selections":
                            cleanedFilters.Add("specialties", filter.Value);
                            break;
                        case "military_affiliation":
                            cleanedFilters.Add("military", filter.Value);
                            break;
                        case "gpa":
                            cleanedFilters.Add("gpa", filter.Value);
                            break;
                        case "us_citizen":
                            cleanedFilters.Add("isUSCitizen", filter.Value.ToLower() == "yes" ? "true" : "false");
                            break;
                        case "postal_code":
                            cleanedFilters.Add("zipCode", filter.Value);
                            break;
                        case "country":
                            cleanedFilters.Add("country", filter.Value);
                            break;
                        case "state":
                            cleanedFilters.Add("state", filter.Value);
                            break;
                        case "highest_level_of_education_completed":
                            cleanedFilters.Add("educationLevel", filter.Value);
                            break;
                        case "desired_degree_level":
                            cleanedFilters.Add("desireDegreeLevel", filter.Value);
                            break;
                        case "schoolsselected":
                            cleanedFilters.Add("institution", filter.Value);
                            break;
                        case "desired_start_date":
                            cleanedFilters.Add("desiredStartDate", filter.Value);
                            break;
                        case "dynamiccampussoftpreference":
                            cleanedFilters.Add("campusPreference", filter.Value);
                            break;
                        case "year_of_highest_education_completed":
                            cleanedFilters.Add("gradYear", filter.Value);
                            break;
                        case "renderingStrategy":
                            cleanedFilters.Add("renderingstrategy", filter.Value);
                            break;
                        case "devicetype":
                            cleanedFilters.Add("devicetype", filter.Value);
                            break;
                    }
                }

                cleanedFilters.Add("queryString", widgetRequest.QueryString);
                cleanedFilters.Add("location", widgetRequest.Location);
                cleanedFilters.Add("hostName", widgetRequest.HostName);
                cleanedFilters.Add("pathName", widgetRequest.PathName);
                cleanedFilters.Add("urlReferer", widgetRequest.ReferrerUrl);
            }

            return cleanedFilters;
        }
    }
}