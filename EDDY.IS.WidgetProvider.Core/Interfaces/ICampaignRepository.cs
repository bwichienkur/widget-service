using System;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface ICampaignRepository
    {
       public Task<bool> HasExitPop(Guid traclId);
    }
}
