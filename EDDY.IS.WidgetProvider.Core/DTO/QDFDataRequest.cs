using EDDY.IS.WidgetProvider.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EDDY.IS.WidgetProvider.Core.DTO
{
	public class QDFDataRequest
	{
        [JsonConverter(typeof(BooleanConverter))]
        public bool IgnoreTrackId { get; set; }
        public string TrackId { get; set; }
		public Dictionary<string, string> CurrentData { get; set; }
		public Dictionary<string, string> FieldsToUpdate { get; set; }
	}
}
