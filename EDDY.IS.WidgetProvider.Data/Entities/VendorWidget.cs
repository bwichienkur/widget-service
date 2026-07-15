using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class VendorWidget
    {
        public VendorWidget()
        {
            VendorWidgetSettingValue = new HashSet<VendorWidgetSettingValue>();
        }

        public int VendorWidgetId { get; set; }
        public Guid VendorWidgetToken { get; set; }
        public int WidgetId { get; set; }
        public int VendorId { get; set; }
        public string VendorWidgetName { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual Widget Widget { get; set; }
        public virtual ICollection<VendorWidgetSettingValue> VendorWidgetSettingValue { get; set; }
    }
}
