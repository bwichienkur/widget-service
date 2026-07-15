using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Core.DTO
{
    public class WidgetRequest
    { 
        public Guid VendorToken { get; set; }
        public Guid TrackId { get; set; }

        public string Location { get; set; }
        public string HostName { get; set; }
        public string PathName { get; set; }
        public string QueryString { get; set; }

        public string PageUrl { get; set; }
        public string ReferrerUrl { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }
        public string JqueryVersionNumber { get; set; }
        public Guid WidgetRequestGuid { get; set; }
        public List<ContainerData> ContainerList { get; set; }
        public Dictionary<string, string> FilterFields { get; set; } = null;
        public bool IsModalLoad { get; set; } = false;
        public bool LoadExternalResources { get; set; } = true;
        public Guid? CookieTrackId { get; set; }
        public bool UpdateWidget { get; set; }
    }

    public class ContainerData
    {
        public string ContainerName { get; set; }
        public Dictionary<string, object> Settings { get; set; }
    }
}
