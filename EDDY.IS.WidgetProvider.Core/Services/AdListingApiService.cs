using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.Services
{
    public class AdListingApiService : IAdListingApiService
    {
        private static IConfiguration _config;
        public AdListingApiService(IConfiguration config)
        {
            _config = config;
        }

        public AdListingApiResponse GetApiResponse(Dictionary<string,string> filterSettings, Guid placementToken, Guid campaignTrackId, WidgetRequest widgetRequest, string widgetName)
        {
            AdListingApiResponse result = new AdListingApiResponse();

            string requestQueryString = null;

            requestQueryString = BuildRequestQueryString(filterSettings, placementToken, campaignTrackId, widgetRequest.PageUrl, widgetRequest.UserAgent, widgetRequest.IPAddress, widgetRequest.WidgetRequestGuid, widgetName);

            result = GetDataAsync(requestQueryString).GetAwaiter().GetResult();
            
            return result;
        }

        private string BuildRequestQueryString(Dictionary<string, string> filterSettings, Guid placementToken, Guid campaignTrackId, string pageUrl, string userAgent, string iPAddress, Guid widgetRequestGuid, string widgetName) 
        {
            string queryString = "";

            queryString = $"?placementtoken={placementToken}&trackid={campaignTrackId}&siteurl={WebUtility.UrlEncode(pageUrl)}&useragent={userAgent}&userip={iPAddress}&widgetRequestGuid={widgetRequestGuid.ToString()}&widgetName={widgetName}";

            foreach (var pair in filterSettings)
            {
                queryString += $"&{pair.Key}={pair.Value}";
            }

            return queryString;
        }
        private async Task<AdListingApiResponse> GetDataAsync(string requestQueryString)
        {
            AdListingApiResponse resultObj = null;

            using (HttpClient client = new HttpClient())
            {
                double timeoutValue;

                if (_config["HttpRequestTimeout"] != null && Double.TryParse(_config["HttpRequestTimeout"], out timeoutValue))
                    client.Timeout = TimeSpan.FromSeconds(timeoutValue);

                client.BaseAddress = new Uri(_config["AdListingApiURL"]);

                HttpResponseMessage httpResponse = await client.GetAsync(requestQueryString);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var streamResponse = await httpResponse.Content.ReadAsStreamAsync();

                    resultObj = await JsonSerializer.DeserializeAsync<AdListingApiResponse>(streamResponse);
                }
            }

            return resultObj;
        }
    }
}
