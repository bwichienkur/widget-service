using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Core.Entities
{
    public class GPListingApiPlacement
    {
        public string RenderingDiv { get; set; }
        public string CSSModel { get; set; }
        public Dictionary<string, string> FilterSettings { get; set; }
        public List<AdListingGPApiAdItem> AdItemList { get; set; } = new();
        public string RenderedAds { get; set; } = "";
        public List<string> ImpressionPixels { get; set; } = new();
        public GPListingApiCreative Creative { get; set; }
        public string NoAdListingAPI { get; set; }
        public string NoFontFace { get; set; }
        public bool IsTesting { get; set; }
    }

    public class AdListingGPApiAdItem
    {
        public string Header { get; set; }
        public string Description { get; set; }
        public string InstitutionName { get; set; }
        public string LogoLargeImage { get; set; }
        public string LogoSmallImage { get; set; }
        public string LogoMediumImage { get; set; }
        public string ClickThroughUrl { get; set; }
        public string DisplayUrl { get; set; }
        public string PopularPrograms { get; set; }
    }

    public enum GPListingApiCreative
    {
        Default = 1,
        Exclusive = 2,
        CustomCreative = 3
    }
}
