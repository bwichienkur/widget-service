using EDDY.IS.WidgetProvider.Core.ComponentModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Entities
{
    public class AdListingApiPlacement
    {
        public string RenderingDiv { get; set; }
        public string CSSModel { get; set; }
        public Dictionary<string, string> FilterSettings { get; set; }
        public List<AdListingApiAdItem> AdItemList { get; set; }
        public AdListingApiCreative Creative { get; set; }

        public string NoAdListingAPI { get; set; }
        public string NoFontFace { get; set; }
    }
    public class AdListingApiAdItem
    {
        public string InstitutionName { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string SmallLogoUrl { get; set; }
        public string LargeLogoUrl { get; set; }
        public string ClickUrl { get; set; }
        public bool IsLeadUrl { get; set; }
        public List<string> AdProgramNameList { get; set; }
        public string PopularPrograms { get; set; }
    }

    public enum AdListingApiCreative
    {
        Default = 1,
        Project8Style = 2,
        QDFAdListingStyle = 3
    }
}
