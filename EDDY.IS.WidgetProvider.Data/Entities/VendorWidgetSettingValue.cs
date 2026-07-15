using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class VendorWidgetSettingValue
    {
        public int VendorWidgetSettingValueId { get; set; }
        public int VendorWidgetId { get; set; }
        public int WidgetSettingId { get; set; }
        public string Value { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual VendorWidget VendorWidget { get; set; }
        public virtual WidgetSetting WidgetSetting { get; set; }
    }
}
