using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace EDDY.IS.WidgetProvider.Core.Services
{

    public class FESessionRedisService : IFESessionRedisService
    {
        private readonly ILogger<FESessionRedisService> _logger;
        private static IConfiguration _config;
        private string _cachePrefix = "";
        private static List<string> FE_ALLKEYS = new List<string>()  {"address", "address_2", "age", "alternate_phone", "area_of_interest"
                        , "besttimetocontact", "campus", "campuspreference", "campussoftpreference", "categories_selections", "categories_hidden"
                        , "city", "collegegraduationyear", "completed_1600_hours_of_clinical_experience", "concentration", "contact_info"
                        , "country", "currentschool", "desired_area_of_study", "desired_degree_level", "desired_start_date"
                        , "dynamiccampussoftpreference", "education_info", "email", "employed_radiology_or_graduated_past_5_years", "employmentstatus"
                        , "first_name", "furtheringeducation", "gpa", "havetransfercredits", "highest_level_of_education_completed"
                        , "last_name", "lpn_license", "major", "military_affiliation", "newsletteroptin"
                        , "personal_info", "phone", "postal_code", "preferred_methods_of_contact", "prefix"
                        , "program_of_interest", "program_questions", "registered_and_licensure", "registered_radiology", "rn_license"
                        , "specialties_selections", "specialties_hidden", "state", "studentloan", "subcategories_selections", "subcategories_hidden", "subject"
                        , "teaching_certificate", "uajobs", "uaprospectflow", "undergraduate_degree_education", "undergraduate_degree_grad"
                        , "undergraduate_degree_nursing", "us_citizen", "year_of_highest_education_completed", "yearofbirth", "years_of_teaching_experience"
                        , "years_of_work_experience", "schoolsselected", "LeadSourceUrl", "FormLeadUrl", "leadcreatedproduct"};

        private static List<string> FE_SUPPORTEDKEYS = new List<string>()  {"postal_code", "age", "desired_degree_level", "military_affiliation"
                        , "categories_selections", "categories_hidden","subcategories_selections", "subcategories_hidden"
                        , "year_of_highest_education_completed", "us_citizen", "gpa", "country","state","highest_level_of_education_completed" ,"campuspreference"
                        , "desired_start_date" ,"formleadurl" ,"city","email","teaching_certificate","rn_license","phone","firstname","lastname", "schoolsselected"
                        ,"thank_you_experience","leadcreatedproduct","formstep","workflowstep","paidStatusType","FormLeadUrl","LeadSourceUrl","ccne","lpn_license"
                        ,"christian_faithbased","hybrid_location", "undergraduate_degree_nursing","undergraduate_degree_grad","years_of_teaching_experience"
                        ,"years_of_work_experience","completed_1600_hours_of_clinical_experience","employed_radiology_or_graduated_past_5_years","registered_radiology"
                        ,"registered_and_licensure","gender","household_income","specialties","dynamiccampussoftpreference"};

        public FESessionRedisService(IConfiguration config, ILogger<FESessionRedisService> logger)
        {
            _config = config;
            _cachePrefix = _config.GetSection("RedisSettings")["FormsEngineRedisCachePrefix"];
            _logger = logger;
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = _config.GetConnectionString("RedisConnection");
            return ConnectionMultiplexer.Connect(cacheConnection);
        });


        public Dictionary<string, string> GetFormsEngineSession(string fesessionid)
        {
            Dictionary<string, string> Result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            try
            {
                IDatabase cache = lazyConnection.Value.GetDatabase();
                var cacheItem = cache.StringGet($"{_cachePrefix}.FE.Session[{fesessionid}]");
                if (cacheItem.HasValue)
                {
                    var feSession = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(cacheItem);
                    if (feSession != null)
                    {
                        foreach (var item in feSession)
                        {
                            if (FE_SUPPORTEDKEYS.Contains(item.Key.ToLower()))
                            {
                                Result.TryAdd(item.Key.ToLower(), item.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return Result;
        }
    }
}
