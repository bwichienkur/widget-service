using EDDY.IS.WidgetProvider.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core.ComponentModels
{
    public class LeaveBehindModel : IRenderable
    {
        public string TargetUrl { get; set; }
        public string jQuerySelector { get; set; }
        public Task Configure(List<VendorWidgetConfig> widgetConfigList)
        {
            VendorWidgetConfig config = widgetConfigList.FirstOrDefault();

            if (config != null)
            {
                this.TargetUrl = config.SystemSettings[WidgetComponentType.Custom]["targeturl"];
                this.jQuerySelector = config.SystemSettings[WidgetComponentType.Custom].ContainsKey("jqueryselector") ? config.SystemSettings[WidgetComponentType.Custom]["jqueryselector"] : "a";
            }
            return Task.CompletedTask;
        }

        public async Task<string> RenderAsync(IViewRenderService viewRenderService)
        {
            string renderedJs = "";

            renderedJs = await viewRenderService.RenderToStringAsync(WidgetType.LeaveBehind, "LeaveBehind", this);

            return renderedJs;
        }
    }
}
