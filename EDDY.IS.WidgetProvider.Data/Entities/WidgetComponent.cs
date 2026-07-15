using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class WidgetComponent
    {
        public WidgetComponent()
        {
            WidgetSetting = new HashSet<WidgetSetting>();
        }

        public int WidgetComponentId { get; set; }
        public string WidgetComponentName { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<WidgetSetting> WidgetSetting { get; set; }
    }
}
