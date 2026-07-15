using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EDDY.IS.WidgetProvider.Core;
using EDDY.IS.WidgetProvider.Core.Services;
using EDDY.IS.WidgetProvider.Core.Interfaces;
using EDDY.IS.WidgetProvider.Core.DTO;
using Newtonsoft.Json.Serialization;
using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Entities.QDFData;

namespace EDDY.IS.WidgetProvider.Service.Controllers
{
    
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QDFController : Controller
    {
        private readonly IQDFService _qdfService;
        private readonly IWidgetRepository _widgetRepository;

        public QDFController(
            IQDFService qDFService,
            IWidgetRepository widgetRepository
            )
        {
            _widgetRepository = widgetRepository;
            _qdfService = qDFService;
        }

        [HttpPost]
        public JsonResult RetrieveQDFData([FromBody]QDFDataRequest request)
        {
            QDFDataResponse response = new QDFDataResponse();
            
            var ignoreTrackId = request.IgnoreTrackId;
            if (request != null)
            {
                Guid trackid = Guid.Empty;
                if (!string.IsNullOrWhiteSpace(request.TrackId))
                {
                    trackid = Guid.Parse(request.TrackId);
                }

                foreach (var x in request.FieldsToUpdate)
                {
                    if (x.Key == "Categories")
                        response.ReturnData.Add(x.Key, CreateQDFFieldOptionsFromCategories(_qdfService.GetCategories(trackid, GetInput(request.CurrentData, x.Value.Split(',')), ignoreTrackId)));
                    else if (x.Key == "SubCategories")
                        response.ReturnData.Add(x.Key, CreateQDFFieldOptionsFromSubjects(_qdfService.GetSubjects(trackid, GetInput(request.CurrentData, x.Value.Split(',')), ignoreTrackId)));
                    else if (x.Key == "Specialties")
                        response.ReturnData.Add(x.Key, CreateQDFFieldOptionsFromSpecialties(_qdfService.GetSpecialties(trackid, GetInput(request.CurrentData, x.Value.Split(',')), ignoreTrackId)));
                    else if (x.Key == "Desired_Degree_Level")
                        response.ReturnData.Add(x.Key, CreateQDFFieldOptionsFromProgramLevels(_qdfService.GetProgramLevels(trackid, GetInput(request.CurrentData, x.Value.Split(',')), ignoreTrackId)));
                }
            }
            return Json(response);
        }

        private List<QDFFieldOptions> CreateQDFFieldOptionsFromCategories(List<Category> categories)
        {
            List<QDFFieldOptions> options = new List<QDFFieldOptions>();

            foreach (var x in categories)
                options.Add(new QDFFieldOptions() { Id = x.CategoryId, Name = x.CategoryName });

            return options;
        }

        private List<QDFFieldOptions> CreateQDFFieldOptionsFromSubjects(List<Subject> subjects)
        {
            List<QDFFieldOptions> options = new List<QDFFieldOptions>();

            foreach (var x in subjects)
                options.Add(new QDFFieldOptions() { Id = x.SubjectId, Name = x.SubjectName });

            return options;
        }

        private List<QDFFieldOptions> CreateQDFFieldOptionsFromSpecialties(List<Specialty> specialties)
        {
            List<QDFFieldOptions> options = new List<QDFFieldOptions>();

            foreach (var x in specialties)
                options.Add(new QDFFieldOptions() { Id = x.SpecialtyId, Name = x.SpecialtyName });

            return options;
        }

        private List<QDFFieldOptions> CreateQDFFieldOptionsFromProgramLevels(List<ProgramLevel> programLevels)
        {
            List<QDFFieldOptions> options = new List<QDFFieldOptions>();

            foreach (var x in programLevels)
                options.Add(new QDFFieldOptions() { Id = x.ProgramLevelId, Name = x.ProgramLevelName });

            return options;
        }

        private Dictionary<MatchingEngineInput, string> GetInput(Dictionary<string, string> currentData, string[] dataPointsNeeded)
        {
            Dictionary<MatchingEngineInput, string> returnValue = new Dictionary<MatchingEngineInput, string>();

            foreach (var s in dataPointsNeeded)
            {
                MatchingEngineInput? input = _qdfService.MapFromCode(s);

                if (input.HasValue && currentData.ContainsKey(s))
                    returnValue.Add(input.Value, currentData[s]);
            }
            return returnValue;
        }
    }
}