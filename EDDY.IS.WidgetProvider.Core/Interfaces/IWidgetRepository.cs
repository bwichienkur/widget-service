using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface IWidgetRepository
    {
        VendorWidgetConfig GetWidgetConfig(string containerName, Guid vendorWidgetToken);
        void SaveWidgetRequests(Dictionary<WidgetType, List<VendorWidgetConfig>> dicVendorWidgetConfig, WidgetRequest request, long widgetRenderMilliseconds);
        void SaveWidgetImpression(Guid widgetRequestGuid);
        QDFTemplate GetQDFTemplate(int templateId);
        Dictionary<string, Dictionary<string, string>> GetVendorUrlConfigurations();
    }
}
