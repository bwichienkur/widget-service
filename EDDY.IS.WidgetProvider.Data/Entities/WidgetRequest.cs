using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class WidgetRequest
    {
        public int WidgetRequestId { get; set; }
        public Guid WidgetRequestGuid { get; set; }
        public int VendorWidgetId { get; set; }
        public string WidgetSettingsJson { get; set; }
        public string PageUrl { get; set; }
        public string PageQueryString { get; set; }
        public string ReferringUrl { get; set; }
        public string ReferringQueryString { get; set; }
        public string UserAgent { get; set; }
        public string Ipaddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public long RenderTimeMilliseconds { get; set; }
        public string JqueryVersionNumber { get; set; }
    }
}
