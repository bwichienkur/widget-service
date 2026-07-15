using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
	public class VwQDFTemplateConfiguration
	{
		public int TemplateId { get; set; }
		public string TemplateName { get; set; }
		public string Header { get; set; }
		public string SubHeading { get; set; }
		public int TemplateStepId { get; set; }
		public int RowSequence { get; set; }
		public string Code { get; set; }
		public string FieldName { get; set; }
		public bool? IsRequired { get; set; }
		public string InstanceLabel { get; set; }
		public string InstanceWatermark { get; set; }
		public string StandardControlTypeName { get; set; }
	}
}
