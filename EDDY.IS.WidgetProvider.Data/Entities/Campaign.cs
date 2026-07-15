using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class Campaign
    {
        public Campaign()
        {

        }
        public Int64 CampaignId { get; set; }
        public Guid TrackId { get; set; }
        public bool HasExitPop { get; set; }
    }
}
