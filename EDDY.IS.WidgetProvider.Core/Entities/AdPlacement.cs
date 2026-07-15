using EDDY.IS.WidgetProvider.Core.ComponentModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Entities
{
    public class AdPlacement
    {
        public Guid PlacementToken { get; set; }
        public Guid CampaignTrackId { get; set; }
        public string RenderingDiv { get; set; }
        public Guid WidgetRequestGuid { get; set; }
        public bool TestMode { get; set; }
        public string CSSModel { get; set; }
        public Dictionary<string, string> FilterSettings { get; set; }
    }
}
