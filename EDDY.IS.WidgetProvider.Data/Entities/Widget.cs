using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class Widget
    {
        public Widget()
        {
            VendorWidget = new HashSet<VendorWidget>();
            WidgetSetting = new HashSet<WidgetSetting>();
        }

        public int WidgetId { get; set; }
        public string WidgetName { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<VendorWidget> VendorWidget { get; set; }
        public virtual ICollection<WidgetSetting> WidgetSetting { get; set; }
    }
}
