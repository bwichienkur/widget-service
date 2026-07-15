using EDDY.IS.WidgetProvider.Core.Entities;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(WidgetType widgetType, string viewName, object model);
        Task<string> RenderToStringAsync(WidgetType widgetType, string viewName, object model, bool shouldEscape);
    }
}
