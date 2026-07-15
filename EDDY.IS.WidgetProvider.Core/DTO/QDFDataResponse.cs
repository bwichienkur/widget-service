using EDDY.IS.WidgetProvider.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.DTO
{
	public class QDFDataResponse
	{
		public Dictionary<string, List<QDFFieldOptions>> ReturnData { get; set; }

		public QDFDataResponse()
		{
			ReturnData = new Dictionary<string, List<QDFFieldOptions>>();
		}
	}
}
