using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
	public partial class VendorWidgetUrlParameterConfig
	{
		public int VendorWidgetUrlParameterConfigId { get; set; }
		public int VendorId { get; set; }
		public string Url { get; set; }
		public string Categories { get; set; }
		public string SubCategories { get; set; }
		public string Specialties { get; set; }
		public string DegreeLevels { get; set; }
		public string States { get; set; }

		public bool IsEnabled { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}
