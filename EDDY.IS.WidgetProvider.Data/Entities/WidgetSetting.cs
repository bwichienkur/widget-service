using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class WidgetSetting
    {
        public WidgetSetting()
        {
            VendorWidgetSettingValue = new HashSet<VendorWidgetSettingValue>();
        }

        public int WidgetSettingId { get; set; }
        public int WidgetId { get; set; }
        public int WidgetComponentId { get; set; }
        public string WidgetSettingKey { get; set; }
        public int WidgetSettingTypeId { get; set; }
        public bool IsRequired { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual Widget Widget { get; set; }
        public virtual WidgetComponent WidgetComponent { get; set; }
        public virtual WidgetSettingType WidgetSettingType { get; set; }
        public virtual ICollection<VendorWidgetSettingValue> VendorWidgetSettingValue { get; set; }
    }
}
