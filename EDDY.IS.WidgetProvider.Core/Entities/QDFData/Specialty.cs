using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Entities.QDFData
{
	public class Specialty : IMEField
	{
		public int SpecialtyId { get; set; }
		public string SpecialtyName { get; set; }

		public int Id { get { return SpecialtyId; } }
		public string Name { get { return SpecialtyName; } }

		public Specialty() { }
		public Specialty(MatchingEngine.Specialty s)
		{
			SpecialtyId = s.SpecialtyId;
			SpecialtyName = s.SpecialtyName;
		}
	}
}
