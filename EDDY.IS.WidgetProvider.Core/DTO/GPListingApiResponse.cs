using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Core.DTO
{
    public class GPListingApiResponse
    {
        public string Error { get; set; }
        public string Status { get; set; }
        public List<string> Pixels { get; set; }
        public string Message { get; set; }
        public Guid TransactionId { get; set; }
        public bool IsTesting { get; set; }
    }

    public class GPListingObjectResponse : GPListingApiResponse
    {
        public List<AdList> Ads { get; set; } = new();
    }

    public class GPListingCreativeResponse : GPListingApiResponse
    {
        public string Ads { get; set; } = "";
    }

    public class AdList
    {
        public int? Position { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public string DisplayUrl { get; set; }
        public string InstitutionName { get; set; }
        public string LogoLargeImage { get; set; }
        public string LogoSmallImage { get; set; }
        public string LogoMediumImage { get; set; }
        public string ClickThroughUrl { get; set; }
        public string PopularPrograms { get; set; }
        public decimal Commission { get; set; }
        public string CommissionType { get; set; }
    }
}
