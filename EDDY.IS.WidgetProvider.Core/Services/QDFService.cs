using System;
using System.Collections.Generic;
using System.Text;
using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Entities.QDFData;
using EDDY.IS.WidgetProvider.Core;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace EDDY.IS.WidgetProvider.Core.Services
{
	public class QDFService : IQDFService
	{
		private readonly IWidgetRepository _widgetRepository;
		private readonly IConfiguration _config;
		private readonly ICacheService _cacheService;
		private static List<string> macroValues = new List<string> { "{SUBCATEGORY}", "{CATEGORY}", "{PROGRAMLEVEL}", "{SPECIALTY}" };

		public QDFService(IWidgetRepository widgetRepository, IConfiguration config, ICacheService cacheService)
		{
			_widgetRepository = widgetRepository;
			_config = config;
			_cacheService = cacheService;
		}

		private Dictionary<MatchingEngineInput, string> GetFilters(Dictionary<string, string> inputFilters, List<MatchingInputField> predecessors)
		{
			Dictionary<MatchingEngineInput, string> filters = new Dictionary<MatchingEngineInput, string>();

			if (predecessors != null)
			{
				foreach (var p in predecessors)
				{
					if (inputFilters.ContainsKey(p.Code))
						filters.Add(p.MEInput, inputFilters[p.Code]);
				}
			}

			return filters;
		}

		//private QDFTemplate GetTemplate(int templateId)
		//{
		//	string cacheKey = "qdfService_TemplateId_" + templateId;
		//	string cacheValue = _cacheService.GetCacheItem(cacheKey);
		//	QDFTemplate template = null;

		//	if (!String.IsNullOrEmpty(cacheValue))
		//	{
		//		template = JsonConvert.DeserializeObject<QDFTemplate>(cacheValue);
		//	}
		//	else
		//	{
		//		template = _widgetRepository.GetQDFTemplate(templateId);

		//		_cacheService.SetCacheItem(cacheKey, JsonConvert.SerializeObject(template));
		//	}

		//	return template;
		//}

		private string ReplaceMacroWithValue(string searchString, Dictionary<string, string> filters, Guid trackid)
		{
			string tempString = searchString.ToUpper();

			foreach (var macro in macroValues)
			{
				if (tempString.Contains(macro))
				{
					switch (macro)
					{
						case "{SUBCATEGORY}":
							if (filters.ContainsKey("SubCategories"))
							{
								Dictionary<MatchingEngineInput, string> values = new Dictionary<MatchingEngineInput, string>();
								values.Add(MatchingEngineInput.Subject, filters["SubCategories"]);

								List<Subject> subjects = GetSubjects(trackid, values);

								if (subjects != null && subjects.Count > 0)
									return Regex.Replace(searchString, macro, subjects[0].Name, RegexOptions.IgnoreCase);
							}
							break;
						case "{CATEGORY}":
							if (filters.ContainsKey("Categories"))
							{
								Dictionary<MatchingEngineInput, string> values = new Dictionary<MatchingEngineInput, string>();
								values.Add(MatchingEngineInput.Category, filters["Categories"]);

								List<Category> categories = GetCategories(trackid, values);

								if (categories != null && categories.Count > 0)
									return Regex.Replace(searchString, macro, categories[0].Name, RegexOptions.IgnoreCase);
							}
							break;
						case "{SPECIALTY}":
							if (filters.ContainsKey("Specialties"))
							{
								Dictionary<MatchingEngineInput, string> values = new Dictionary<MatchingEngineInput, string>();
								values.Add(MatchingEngineInput.Specialty, filters["Specialties"]);

								List<Specialty> specialties = GetSpecialties(trackid, values);

								if (specialties != null && specialties.Count > 0)
									return Regex.Replace(searchString, macro, specialties[0].Name, RegexOptions.IgnoreCase);
							}
							break;
						case "{PROGRAMLEVEL}":
							if (filters.ContainsKey("Desired_Degree_Level"))
							{
								Dictionary<MatchingEngineInput, string> values = new Dictionary<MatchingEngineInput, string>();
								values.Add(MatchingEngineInput.ProgramLevel, filters["Desired_Degree_Level"]);

								List<ProgramLevel> programLevels = GetProgramLevels(trackid, values);

								if (programLevels != null && programLevels.Count > 0)
									return Regex.Replace(searchString, macro, programLevels[0].Name, RegexOptions.IgnoreCase);
							}
							break;
					}
				}
			}
			return searchString;
		}

		public QDFTemplate GetQDFTemplate(int templateId, Guid trackid, Dictionary<string, string> filters, bool ignoreTrackId = false)
		{

			//QDFTemplate template = GetTemplate(templateId);
			QDFTemplate template = _widgetRepository.GetQDFTemplate(templateId);

			if (template != null)
			{
				template.Header = ReplaceMacroWithValue(template.Header, filters, trackid);
				template.SubHeading = ReplaceMacroWithValue(template.SubHeading, filters, trackid);

				foreach (var field in template.Fields)
				{
					switch(field.ControlType)
					{
						case QDFFieldType.Dropdown:
						case QDFFieldType.DropdownMultiple:
							if (field.Code == "Categories")
							{
								List<Category> categories = GetCategories(trackid, GetFilters(filters, field.Predecessors), ignoreTrackId);
								PopulateOptionValues(field, categories.ToArray(), filters);
							}
							else if (field.Code == "SubCategories")
							{
								List<Subject> subjects = GetSubjects(trackid, GetFilters(filters, field.Predecessors), ignoreTrackId);
								PopulateOptionValues(field, subjects.ToArray(), filters);
							}
							else if (field.Code == "Desired_Degree_Level")
							{
								List<ProgramLevel> programLevels = GetProgramLevels(trackid, GetFilters(filters, field.Predecessors), ignoreTrackId);
								PopulateOptionValues(field, programLevels.ToArray(), filters);
							}
							else if (field.Code == "Specialties")
							{
								List<Specialty> specialties = GetSpecialties(trackid, GetFilters(filters, field.Predecessors), ignoreTrackId);
								PopulateOptionValues(field, specialties.ToArray(), filters);
							}
							break;
						case QDFFieldType.Hidden:
						case QDFFieldType.Textbox:
							if (filters.ContainsKey(field.Code) && !String.IsNullOrEmpty(filters[field.Code]))
								field.Value = filters[field.Code];
							if (field.Code == "Year_of_Highest_Education_Completed")
								field.DataType = QDFFieldDataType.Year;
							break;
					}
					
				}
			}

			return template;
		}

		private void PopulateOptionValues(QDFField field, IMEField[] meFields, Dictionary<string, string> filters)
		{
			field.Options = new List<QDFFieldOptions>();

			foreach (IMEField f in meFields)
			{
				int filterValue;
				
				if (filters.ContainsKey(field.Code))
				{
					foreach(string s in filters[field.Code].Split(','))
                    {
                        Int32.TryParse(s, out filterValue);

                        if (filterValue == f.Id)
                            field.Options.Add(new QDFFieldOptions() { Id = f.Id, Name = f.Name, Selected = true });
                    }

                    if (!field.Options.Exists(x => x.Id == f.Id))
                        field.Options.Add(new QDFFieldOptions() { Id = f.Id, Name = f.Name, Selected = false });
                }
                else
                    field.Options.Add(new QDFFieldOptions() { Id = f.Id, Name = f.Name, Selected = false });
            }
		}

		private int[] CreateIntArray(string input)
		{
			List<int> returnValue = new List<int>();

			foreach (var s in input.Split(','))
			{
				int item = 0;

				if(int.TryParse(s, out item))
					returnValue.Add(item);
			}

			return returnValue.ToArray();
		}

		private MatchingEngine.DirectoryMatchRequest CreateRequest(Guid trackid, Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false)
		{
			MatchingEngine.DirectoryMatchRequest request = new MatchingEngine.DirectoryMatchRequest();
			List<KeyValuePair<string, int>> kvcodes;

			request.TrackGuid = trackid;
			request.RemoveInvalidEntities = true;
			request.SortMethod = MatchingEngine.EntitySortMethod.Alphabetical;

            request.IgnoreTrackingTag = ignoreTrackId;
            if (inputValues != null)
			{
				foreach (var item in inputValues)
				{
					switch (item.Key)
					{
						case MatchingEngineInput.Age:
							if (request.ProspectInput == null) 
								request.ProspectInput = new MatchingEngine.ProspectInput();

							request.ProspectInput.Age = Convert.ToInt32(item.Value);

							break;
						case MatchingEngineInput.CampusType:
							if (item.Value == "Online")
								request.CampusType = MatchingEngine.CampusType.Online;
							else if (item.Value == "Campus")
								request.CampusType = MatchingEngine.CampusType.Ground;

							break;
						case MatchingEngineInput.Category:
							request.CategoryList = CreateIntArray(item.Value);

							break;
						case MatchingEngineInput.Desired_Start_Date:
							if (request.ProspectInput == null)
								request.ProspectInput = new MatchingEngine.ProspectInput();

							kvcodes = new List<KeyValuePair<string, int>>();

							if (request.ProspectInput.KVCodeData != null)
								kvcodes.AddRange(request.ProspectInput.KVCodeData);

							kvcodes.Add(new KeyValuePair<string, int>("desired_start_date", Convert.ToInt32(item.Value)));
							request.ProspectInput.KVCodeData = kvcodes.ToArray();

							break;
						case MatchingEngineInput.EducationLevel:
							if (request.ProspectInput == null)
								request.ProspectInput = new MatchingEngine.ProspectInput();

							request.ProspectInput.EducationLevelId = Convert.ToInt32(item.Value);
							break;
						case MatchingEngineInput.HSGradYear:
							if (request.ProspectInput == null)
								request.ProspectInput = new MatchingEngine.ProspectInput();

							request.ProspectInput.HSGraduationYear = Convert.ToInt32(item.Value);
							break;
						case MatchingEngineInput.Military_Affiliation:
							if (request.ProspectInput == null)
								request.ProspectInput = new MatchingEngine.ProspectInput();

							request.ProspectInput.MilitaryStatusId = Convert.ToInt32(item.Value);

							if (Convert.ToInt32(item.Value) != 126)
								request.ProspectInput.IsMilitary = true;
							else
								request.ProspectInput.IsMilitary = false;

							break;
						case MatchingEngineInput.ProgramLevel:
							request.ProgramLevelList = CreateIntArray(item.Value);

							break;
						case MatchingEngineInput.RNLicense:
							if (request.ProspectInput == null)
								request.ProspectInput = new MatchingEngine.ProspectInput();

							kvcodes = new List<KeyValuePair<string, int>>();

							if (request.ProspectInput.KVCodeData != null)
								kvcodes.AddRange(request.ProspectInput.KVCodeData);

							kvcodes.Add(new KeyValuePair<string, int>("rn_license", Convert.ToInt32(item.Value)));
							request.ProspectInput.KVCodeData = kvcodes.ToArray();

							break;
						case MatchingEngineInput.Specialty:
							request.SpecialtyList = CreateIntArray(item.Value);

							break;
						case MatchingEngineInput.Subject:
							request.SubjectList = CreateIntArray(item.Value);

							break;
						case MatchingEngineInput.USCitizen:
							if (request.ProspectInput == null)
								request.ProspectInput = new MatchingEngine.ProspectInput();

							request.ProspectInput.IsCitizen = Convert.ToBoolean(item.Value);

							break;
						case MatchingEngineInput.ZipCode:
							if (request.ProspectInput == null)
								request.ProspectInput = new MatchingEngine.ProspectInput();

							request.ProspectInput.PostalCode = item.Value;

							break;
					}
				}
			}

			return request;
		}

		public List<Category> GetCategories(Guid trackid,Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false)
		{
			MatchingEngine.MatchingServiceClient msc = new MatchingEngine.MatchingServiceClient(MatchingEngine.MatchingServiceClient.EndpointConfiguration.CustomBinding_IMatchingService, _config["MatchingServiceURL"]);

			MatchingEngine.DirectoryMatchRequest request = CreateRequest(trackid,inputValues, ignoreTrackId);
			MatchingEngine.CategoryResponse cr = msc.GetCategoriesAsync(request).Result;

			if (cr != null && cr.CategoryList != null)
			{
				List<Category> categoryList = new List<Category>();

				foreach (MatchingEngine.Category c in cr.CategoryList)
					categoryList.Add(new Category(c));

				return categoryList;
			}

			return null;
		}

		public List<Subject> GetSubjects(Guid trackid, Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false)
		{
			MatchingEngine.MatchingServiceClient msc = new MatchingEngine.MatchingServiceClient(MatchingEngine.MatchingServiceClient.EndpointConfiguration.CustomBinding_IMatchingService, _config["MatchingServiceURL"]);

			MatchingEngine.DirectoryMatchRequest request = CreateRequest(trackid, inputValues, ignoreTrackId);
			MatchingEngine.SubjectResponse sr = msc.GetSubjectsAsync(request).Result;

			if (sr != null && sr.SubjectList != null)
			{
				List<Subject> subjectList = new List<Subject>();

				foreach (MatchingEngine.Subject s in sr.SubjectList)
					subjectList.Add(new Subject(s));

				return subjectList;
			}

			return null;
		}

		public List<Specialty> GetSpecialties(Guid trackid, Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false)
		{
			MatchingEngine.MatchingServiceClient msc = new MatchingEngine.MatchingServiceClient(MatchingEngine.MatchingServiceClient.EndpointConfiguration.CustomBinding_IMatchingService, _config["MatchingServiceURL"]);

			MatchingEngine.DirectoryMatchRequest request = CreateRequest(trackid, inputValues, ignoreTrackId);
			MatchingEngine.SpecialtyResponse sr = msc.GetSpecialtiesAsync(request).Result;

			if (sr != null && sr.SpecialtyList != null)
			{
				List<Specialty> SpecialtyList = new List<Specialty>();

				foreach (MatchingEngine.Specialty s in sr.SpecialtyList)
					SpecialtyList.Add(new Specialty(s));

				return SpecialtyList;
			}

			return null;
		}

		public List<ProgramLevel> GetProgramLevels(Guid trackid, Dictionary<MatchingEngineInput, string> inputValues, bool ignoreTrackId = false)
		{
			MatchingEngine.MatchingServiceClient msc = new MatchingEngine.MatchingServiceClient(MatchingEngine.MatchingServiceClient.EndpointConfiguration.CustomBinding_IMatchingService, _config["MatchingServiceURL"]);

			MatchingEngine.DirectoryMatchRequest request = CreateRequest(trackid, inputValues, ignoreTrackId);
			MatchingEngine.ProgramLevelResponse pr = msc.GetProgramLevelsAsync(request).Result;

			if (pr != null && pr.ProgramLevelList != null)
			{
				List<ProgramLevel> ProgramLevelList = new List<ProgramLevel>();

				foreach (MatchingEngine.ProgramLevel pl in pr.ProgramLevelList)
					ProgramLevelList.Add(new ProgramLevel(pl));

				return ProgramLevelList;
			}

			return null;
		}

		public MatchingEngineInput? MapFromCode(string code)
		{
			switch (code)
			{
				case "Desired_Degree_Level":
					return MatchingEngineInput.ProgramLevel;
				case "Categories":
					return MatchingEngineInput.Category;
				case "SubCategories":
					return MatchingEngineInput.Subject;
				case "Specialties":
					return MatchingEngineInput.Specialty;
			}

			return null;
		}

	}
}
