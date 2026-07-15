using EDDY.IS.WidgetProvider.Core.DTO;
using System;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface IWidgetPackageService
    {
        Task<string> GetFullWidgetPackage(WidgetRequest widgetConfig);
        void SaveWidgetImpression(Guid widgetRequestGuid);
    }
}
