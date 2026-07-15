using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace EDDY.IS.WidgetProvider.Core.Services
{
    public class GPListingApiService : IGPListingApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private static readonly List<string> ADDITIONAL_FIELDS =
        [
          "thank_you_experience",
          "leadcreatedproduct",
          "formstep",
          "workflowstep",
          "paidStatusType",
          "LeadSourceUrl",
          "FormLeadUrl",
          "ccne",
          "lpn_license",
          "christian_faithbased",
          "hybrid_location",
          "undergraduate_degree_nursing",
          "undergraduate_degree_grad",
          "years_of_teaching_experience",
          "years_of_work_experience",
          "completed_1600_hours_of_clinical_experience",
          "employed_radiology_or_graduated_past_5_years",
          "registered_radiology",
          "registered_and_licensure"
        ];

        public GPListingApiService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TResponse> GetApiResponse<TResponse>(Dictionary<string, string> filterSettings, Guid placementToken, Guid campaignTrackId, WidgetRequest widgetRequest, string widgetName)
        {
            bool renderAsCreative = true;
            if (typeof(TResponse) == typeof(GPListingObjectResponse))
                renderAsCreative = false;

            string content = BuildRequestContent(filterSettings, placementToken, campaignTrackId, widgetRequest, widgetName, renderAsCreative);

            var result = await GetDataAsync<TResponse>(content);

            return result;
        }

        private string BuildRequestContent(Dictionary<string, string> filterSettings, Guid placementToken, Guid campaignTrackId, WidgetRequest widgetRequest, string widgetName, bool renderAsCreative)
        {
            Uri uri = new Uri(widgetRequest.PageUrl);
            var isTest = uri.Host.StartsWith("admin.");
            
            if (!isTest)
            {
                string isTestParam = HttpUtility.ParseQueryString(uri.Query).Get("test");
                isTest = !string.IsNullOrWhiteSpace(isTestParam) && string.Equals("true", isTestParam, StringComparison.CurrentCultureIgnoreCase);
            }

            string gclid = HttpUtility.ParseQueryString(uri.Query).Get("gclid");
            string msclickid = HttpUtility.ParseQueryString(uri.Query).Get("msclkid");

            dynamic flexible = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)flexible;
            dictionary.Add("placementtoken", placementToken);
            dictionary.Add("trackid", campaignTrackId);
            dictionary.Add("siteurl", WebUtility.UrlEncode(widgetRequest.PageUrl));
            dictionary.Add("useragent", widgetRequest.UserAgent);
            dictionary.Add("userip", widgetRequest.IPAddress);
            dictionary.Add("widgetRequestGuid", widgetRequest.WidgetRequestGuid.ToString());
            dictionary.Add(widgetName, widgetName);
            dictionary.Add("isTesting", isTest);
            dictionary.Add("urlReferrer", widgetRequest.ReferrerUrl);
            dictionary.Add("FromWidgetProviderService", true);
            dictionary.Add("gclid", gclid);
            dictionary.Add("msclkid", msclickid);

            if (renderAsCreative)
                dictionary.Add("RenderAsCreative", true);

            var additFieldsDictionary = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> filterSetting in filterSettings)
            {
                KeyValuePair<string, string> pair = filterSetting;

                if (pair.Key == "excludedInstitutions")
                    dictionary.TryAdd(pair.Key, pair.Value.Split(','));
                else if (ADDITIONAL_FIELDS.Any(s => s.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)))
                    additFieldsDictionary.Add(pair.Key, pair.Value);
                else
                    dictionary.TryAdd(pair.Key, pair.Value);
            }

            dictionary.TryAdd("AdditionalFields", additFieldsDictionary);

            return JsonSerializer.Serialize(dictionary);
        }

        private async Task<TResponse> GetDataAsync<TResponse>(string requestQueryString)
        {
            var client = _httpClientFactory.CreateClient();

            if (_config["HttpRequestTimeout"] != null &&
                double.TryParse(_config["HttpRequestTimeout"], out double timeoutValue))
                client.Timeout = TimeSpan.FromSeconds(timeoutValue);

            var content = new StringContent(requestQueryString, Encoding.UTF8, "application/json");
            HttpResponseMessage httpResponse = await client.PostAsync(_config["GPListingApiURL"], content);

            if (httpResponse.IsSuccessStatusCode)
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(response, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            return default(TResponse);
        }

    }
}
