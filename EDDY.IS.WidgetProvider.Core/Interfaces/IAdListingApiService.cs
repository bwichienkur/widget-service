using EDDY.IS.WidgetProvider.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Interfaces
{
    interface IAdListingApiService
    {
        public AdListingApiResponse GetApiResponse(Dictionary<string, string> filterSettings, Guid placementToken, Guid campaignTrackId, WidgetRequest widgetRequest, string widgetName);
    }
}
