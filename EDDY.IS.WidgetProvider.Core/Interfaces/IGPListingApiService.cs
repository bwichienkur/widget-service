using EDDY.IS.WidgetProvider.Core.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.Interfaces
{
    public interface IGPListingApiService
    {
        public Task<TResponse> GetApiResponse<TResponse>(Dictionary<string, string> filterSettings, Guid placementToken, Guid campaignTrackId, WidgetRequest widgetRequest, string widgetName);
    }
}
