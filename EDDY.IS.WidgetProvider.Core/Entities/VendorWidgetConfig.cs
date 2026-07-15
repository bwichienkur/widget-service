using EDDY.IS.WidgetProvider.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Entities
{
    public enum WidgetComponentType
    {
        AdServer = 1,
        FormsEngine = 2,
        Custom = 3
    }

    public enum ListingApiType 
    { 
        AdListingApi = 1,
        GPListingApi = 2
    }

    public enum WidgetType
    {
        AdStackSolo = 1,
        SmartListing = 2,
        QDF = 3,
        ProgramForm = 4,
        LeaveBehind = 5,
        WizardForm = 6,
        AdListingApi = 7,
        QDFLight = 8,
        ExitPop = 9,
        GPQDFLight = 10,
        GPListingApi = 11,
        GPExitPop = 12
    }

    public class VendorWidgetConfig
    {
        public int VendorWidgetId { get; set; }
        public string WidgetContainerName { get; set; }
        public WidgetType WidgetType { get; set; }
        public string CSSText { get; set; }
        public Dictionary<WidgetComponentType, Dictionary<string, string>> SystemSettings {get; set;}
        public Dictionary<string, string> SiteSettings { get; set; }
        public WidgetRequest ClientWidgetRequest { get; set; }

    }
}
