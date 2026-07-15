using EDDY.IS.WidgetProvider.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface IRenderable
    {
        Task Configure(List<VendorWidgetConfig> widgetConfigList);
        Task<string> RenderAsync(IViewRenderService viewRenderService);
    }
}
