using EDDY.IS.WidgetProvider.Core.ComponentModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Entities
{
	public class QDFTemplatePlacement
	{
		public string RenderingDiv { get; set; }
		public string CSSModel { get; set; }
		public Dictionary<string, string> FilterSettings { get; set; }
		public Dictionary<string, string> AdditionalSettings { get; set; }
		public string ButtonText { get; set; }
		public string HeaderText { get; set; }
		public string TrackId { get; set; }
        public string AdStackVendorWidget { get; set; }
        public string NoFontAwesome { get; set; }
		public string NoQDFLightCSS { get; set; }
        public string NoSelect2 { get; set; }
        public QDFTemplate Template { get; set; }
		public string OpenInNewWindow { get; set; }
        public bool IgnoreTrackId { get; set; } = false;

    }

	public class QDFTemplate
	{
		private string _uniqueValue { get; set; }
		public int TemplateId { get; set; }
		public string TemplateName { get; set; }
		public string Header { get; set; }
		public string SubHeading { get; set; }

		public string UniqueValue { get { return _uniqueValue; } }
		public List<QDFField> Fields { get; set; }

		public QDFTemplate()
		{
			_uniqueValue = (System.Guid.NewGuid()).ToString().Substring(24);
		}
	}

	public enum QDFFieldType
	{
		Dropdown = 1,
		Textbox = 2,
		RadioButtons = 3,
		Hidden = 4,
		DropdownMultiple = 5
	}

	public enum QDFFieldDataType
	{
		String = 0,
		Number = 1,
		Year = 2
	}

	public enum MatchingEngineInput
	{
		Category = 1,
		Subject = 2,
		Specialty = 3,
		ProgramLevel = 4,
		USCitizen = 5,
		ZipCode = 6,
		Age = 7,
		Military_Affiliation = 8,
		Desired_Start_Date = 9,
		HSGradYear = 10,
		EducationLevel = 11,
		RNLicense = 12,
		CampusType = 13
	}

	public class QDFField
	{
		public string Code { get; set; }
		public bool IsRequired { get; set; }
		public string FieldName { get; set; }
		public string Label { get; set; }
		public string Watermark { get; set; }
		public QDFFieldType ControlType { get; set; }
		public QDFFieldDataType DataType { get; set; }
		public List<QDFFieldOptions> Options { get; set; }
		public List<string> UpdateFields { get; set; }		
		public List<MatchingInputField> Predecessors { get; set; }
		public MatchingEngineInput MEInput { get; set; }
		public string Value { get; set; }
	}

	public class MatchingInputField
	{
		public MatchingEngineInput MEInput { get; set; }
		public string Code { get; set; }
	}

	public class QDFFieldOptions
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool Selected { get; set; }
	}
}
