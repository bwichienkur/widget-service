using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class VwVendorWidgetConfiguration
    {
        public int VendorWidgetId { get; set; }
        public string VendorWidgetName { get; set; }
        public string SettingValue { get; set; }
        public string WidgetSettingKey { get; set; }
        public int WidgetComponentId { get; set; }
        public Guid? WidgetServiceToken { get; set; }
        public int WidgetId { get; set; }
        public string CustomCSS { get; set; }
    }
}
