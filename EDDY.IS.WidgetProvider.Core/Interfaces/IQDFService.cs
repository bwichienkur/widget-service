using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Entities.QDFData;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core
{
	public interface IQDFService
	{
		QDFTemplate GetQDFTemplate(int templateId, Guid trackid, Dictionary<string, string> filters, bool ignoreTrackId = false);
		List<Category> GetCategories(Guid trackid, Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false);
		List<Subject> GetSubjects(Guid trackid, Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false);
		List<Specialty> GetSpecialties(Guid trackid, Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false);
		List<ProgramLevel> GetProgramLevels(Guid trackid, Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false);
		MatchingEngineInput? MapFromCode(string code);
	}
}
