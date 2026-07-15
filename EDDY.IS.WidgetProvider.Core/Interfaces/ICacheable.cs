using EDDY.IS.WidgetProvider.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface ICacheable
    {
        string GenerateCacheKey(List<VendorWidgetConfig> widgetConfigList);
    }
}
