using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Entities.QDFData
{
	public class ProgramLevel : IMEField
	{
		public int ProgramLevelId { get; set; }
		public string ProgramLevelName { get; set; }

		public int Id { get { return ProgramLevelId; } }
		public string Name { get { return ProgramLevelName; } }

		public ProgramLevel() { }

		public ProgramLevel(MatchingEngine.ProgramLevel pl)
		{
			ProgramLevelId = pl.ProgramLevelId;
			ProgramLevelName = pl.ProgramLevelName;
		}
	}
}
