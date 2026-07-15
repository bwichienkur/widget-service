using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.ComponentModels
{
    public abstract class FormsEngineBaseModel
    {
        public abstract string FormsEngineServiceUrl { get; }
        public Guid CampaignTrackId { get; set; }
        public string RenderingDiv { get; set; }
        public int TemplateId { get; set; }
        public int ApplicationId { get; set; }
        public bool TestMode { get; set; }
        public string LeadSourceUrl { get; set; }
        public string SubSource2 { get; set; }
        public string ButtonText { get; set; }
        public Guid WidgetRequestGuid { get; set; }
        public string CSSModel { get; set; }
        public Dictionary<string, string> FilterSettings { get; set; }

    }
}
