using System.Collections.Generic;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface IFESessionRedisService
    {
        Dictionary<string, string> GetFormsEngineSession(string fesessionid);
    }
}
