using System;
using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class WidgetSettingType
    {
        public WidgetSettingType()
        {
            WidgetSetting = new HashSet<WidgetSetting>();
        }

        public int WidgetSettingTypeId { get; set; }
        public string WidgetSettingTypeName { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<WidgetSetting> WidgetSetting { get; set; }
    }
}
