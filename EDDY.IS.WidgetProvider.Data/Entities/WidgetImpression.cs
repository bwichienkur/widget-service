using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class WidgetImpression
    {
        public int WidgetImpressionId { get; set; }
        public Guid WidgetRequestGuid { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
