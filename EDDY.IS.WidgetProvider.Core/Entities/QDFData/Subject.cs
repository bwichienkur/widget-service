using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Entities.QDFData
{
	public class Subject : IMEField
	{
		public int SubjectId { get; set; }
		public string SubjectName { get; set; }

		public int Id { get { return SubjectId; } }
		public string Name { get { return SubjectName; } }

		public Subject() { }

		public Subject(MatchingEngine.Subject s)
		{
			SubjectId = s.SubjectId;
			SubjectName = s.SubjectName;
		}
	}
}
