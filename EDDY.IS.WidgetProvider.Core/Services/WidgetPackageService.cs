using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EDDY.IS.WidgetProvider.Core.Services
{
    public class WidgetPackageService : IWidgetPackageService
    {
        private readonly ILogger<WidgetPackageService> logger;
        private readonly IWidgetRepository _widgetRepository;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConfiguration _configuration;
        private readonly IMinificationService _minificationService;
        private readonly ICacheService _cacheService;
        private readonly IModelInstantiationService _modelInstantiationService;

        public WidgetPackageService(ILogger<WidgetPackageService> logger,IWidgetRepository widgetRepository, IViewRenderService viewRenderService, IMinificationService minificationService, IConfiguration configuration, ICacheService cacheService, IModelInstantiationService modelInstantiationService)
        {
            this.logger = logger;
            _widgetRepository = widgetRepository;
            _viewRenderService = viewRenderService;
            _minificationService = minificationService;
            _configuration = configuration;
            _cacheService = cacheService;
            _modelInstantiationService = modelInstantiationService;
        }

        public async Task<string> GetFullWidgetPackage(WidgetRequest widgetRequest)
         {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string fullWidgetString = "";

            try
            {
                Dictionary<WidgetType, List<VendorWidgetConfig>> dicWidgetConfigs = new Dictionary<WidgetType, List<VendorWidgetConfig>>();

                if (widgetRequest != null && widgetRequest.ContainerList != null && widgetRequest.ContainerList.Any())
                {
                    foreach (var container in widgetRequest.ContainerList)
                    {
                        VendorWidgetConfig vendorWidgetConfig = _widgetRepository.GetWidgetConfig(container.ContainerName, widgetRequest.VendorToken);

                        if (vendorWidgetConfig != null)
                        {
                            vendorWidgetConfig.ClientWidgetRequest = widgetRequest;
                            vendorWidgetConfig.SiteSettings = container.Settings.ToDictionary(s => s.Key, s => s.Value.ToString());
                            vendorWidgetConfig.SiteSettings = AugmentSiteSettingsFromUrlConfig(vendorWidgetConfig.SiteSettings, vendorWidgetConfig.WidgetType, widgetRequest.PageUrl);

                            if (!dicWidgetConfigs.ContainsKey(vendorWidgetConfig.WidgetType))
                                dicWidgetConfigs.Add(vendorWidgetConfig.WidgetType, new List<VendorWidgetConfig> { vendorWidgetConfig });
                            else
                                dicWidgetConfigs[vendorWidgetConfig.WidgetType].Add(vendorWidgetConfig);
                        }
                    }

                    if (dicWidgetConfigs.Any())
                    {
                        var tasks = new List<Task<string>>();
                        foreach (var widgetConfig in dicWidgetConfigs.OrderByDescending(f => f.Key))
                            tasks.Add(RenderWidget(widgetConfig, widgetRequest));

                        await Task.WhenAll(tasks);
                        tasks.ForEach(t => fullWidgetString += t.Result);

                        if (Convert.ToBoolean(_configuration["MinifyJavascript"]))
                            fullWidgetString = _minificationService.MinifyJavascript(fullWidgetString);

                        sw.Stop();
                        await Task.Run(() => _widgetRepository.SaveWidgetRequests(dicWidgetConfigs, widgetRequest, sw.ElapsedMilliseconds));
                    }
                }

                if (fullWidgetString.Trim() == string.Empty)
                    fullWidgetString = "WidgetError: No widgets found for VendorToken - " + widgetRequest.VendorToken;

                return fullWidgetString;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return fullWidgetString;
            }
        }

        private async Task<string> RenderWidget(KeyValuePair<WidgetType, List<VendorWidgetConfig>> widgetConfig, WidgetRequest widgetRequest) 
        {
            IRenderable model = _modelInstantiationService.CreateComponent(widgetConfig.Key, widgetRequest);
            string cacheKey = "";
            string widgetString = "";
            bool retrievedFromCache = false;

            if (model is ICacheable)
            {
                cacheKey = ((ICacheable)model).GenerateCacheKey(widgetConfig.Value);
                if (cacheKey != "")
                {
                    string cacheValue = _cacheService.GetCacheItem(cacheKey);
                    if (!string.IsNullOrEmpty(cacheValue))
                    {
                        widgetString += cacheValue;
                        retrievedFromCache = true;
                    }
                }
            }

            if (!retrievedFromCache)
            {
                await model.Configure(widgetConfig.Value);
                string renderedModel = await model.RenderAsync(_viewRenderService);
                widgetString += renderedModel;

                if (model is ICacheable)
                    _cacheService.SetCacheItem(cacheKey, renderedModel);
            }

            return widgetString;
        }

        private Dictionary<string, string> AugmentSiteSettingsFromUrlConfig(Dictionary<string, string> sitePassedInValues, WidgetType widgetType, string pageUrl)
        {
            if (string.IsNullOrWhiteSpace(pageUrl))
                return sitePassedInValues;

            Uri uri = new Uri(pageUrl.ToLower());
            var queryStrings = HttpUtility.ParseQueryString(uri.Query);
            
            if(queryStrings != null)
            {
                // Augment parameters from query string
                if (widgetType == WidgetType.GPListingApi || widgetType == WidgetType.GPQDFLight)
                {
                    foreach (var item in queryStrings.Keys)
                    {
                        if (item == null) continue;
                        var keyName = item.ToString();

                        if (!sitePassedInValues.ContainsKey(keyName))
                            sitePassedInValues[keyName] = queryStrings.Get(keyName);
                    }

                    if (sitePassedInValues.ContainsKey("desired_degree_level") && sitePassedInValues.ContainsKey("degreelevels"))
                        sitePassedInValues.Remove("degreelevels");
                }

                // Augment trackid from query string
                if (queryStrings["trackid"] != null && !sitePassedInValues.ContainsKey("trackid"))
                    sitePassedInValues["trackid"] = queryStrings["trackid"];
            }

            Dictionary<string, Dictionary<string, string>> urlConfigs = (Dictionary<string, Dictionary<string, string>>)_cacheService.GetCacheItem("URLCONFIGS", CacheStorage.Memory);

            if (urlConfigs == null)
            {
                urlConfigs = _widgetRepository.GetVendorUrlConfigurations();
                _cacheService.SetCacheItem("URLCONFIGS", urlConfigs, indefiniteExpiration: true);
            }

            if (urlConfigs != null)
            {
                string url = pageUrl.Split('?')[0];
                if (urlConfigs.ContainsKey(url))
                {
                    Dictionary<string, string> configs = urlConfigs[url];
                    foreach (var config in configs)
                    {
                        if (!sitePassedInValues.ContainsKey(config.Key))
                            sitePassedInValues.Add(config.Key, config.Value);
                    }
                }
            }

            return sitePassedInValues;
        }

        public void SaveWidgetImpression(Guid widgetRequestGuid)
        {
            _widgetRepository.SaveWidgetImpression(widgetRequestGuid);
        }
    }
}
