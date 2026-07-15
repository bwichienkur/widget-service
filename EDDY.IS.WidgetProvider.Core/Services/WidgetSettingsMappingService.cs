using EDDY.IS.WidgetProvider.Core.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Services
{
    public static class WidgetSettingsMappingService
    {
        public static Dictionary<string, string> MapAdditionalSettings(Dictionary<string, string> siteSettings) 
        {
            Dictionary<string, string> returnVal = new();
            if (siteSettings == null)
                return returnVal;

            foreach (var setting in siteSettings)
            {
                SettingsMap.ToAdditionalSettingsMap.TryGetValue(setting.Key.ToLower(), out string mappedSetting);

                if (!string.IsNullOrEmpty(mappedSetting) && returnVal.ContainsKey(mappedSetting))
                    returnVal.Add(mappedSetting, setting.Value);
            }

            return returnVal;
        }

        public static Dictionary<string, string> Map(WidgetComponentType widgetComponent, Dictionary<string, string> siteSettings)
        {
            Dictionary<string, string> returnVal = new Dictionary<string, string>();
            if (siteSettings == null)
                return returnVal;

            foreach (var setting in siteSettings)
            {
                string mappedSetting = GetMappedKey(widgetComponent, setting.Key);

                if (!string.IsNullOrEmpty(mappedSetting) && !returnVal.ContainsKey(mappedSetting))
                    returnVal.Add(mappedSetting, setting.Value);
            }

            return returnVal;
        }

        public static Dictionary<string, string> Map(ListingApiType listingType, Dictionary<string, string> siteSettings)
        {
            Dictionary<string, string> returnVal = new();

            if (siteSettings == null)
                return returnVal;

            foreach (var setting in siteSettings)
            {
                string mappedSetting = GetMappedKey(listingType, setting.Key);

                if (!string.IsNullOrEmpty(mappedSetting) && !returnVal.ContainsKey(mappedSetting))
                    returnVal.Add(mappedSetting, setting.Value);
            }

            return returnVal;
        }

        private static string GetMappedKey(ListingApiType listingType, string siteSetting) {

            string mappedValue = "";

            switch (listingType)
            {
                case ListingApiType.AdListingApi:
                    SettingsMap.SiteToAdListingServiceMap.TryGetValue(siteSetting, out mappedValue);
                    break;
                case ListingApiType.GPListingApi:
                    SettingsMap.SiteToPGListingServiceMap.TryGetValue(siteSetting, out mappedValue);
                    break;
                default:
                    break;
            }

            return mappedValue;
        }

        private static string GetMappedKey(WidgetComponentType widgetComponent, string siteSetting)
        {
            string mappedValue = "";

                switch (widgetComponent)
                {
                    case WidgetComponentType.AdServer:
                        SettingsMap.SiteToAdServerMap.TryGetValue(siteSetting, out mappedValue);
                        break;
                    case WidgetComponentType.FormsEngine:
                        SettingsMap.SiteToFormsEngineMap.TryGetValue(siteSetting, out mappedValue);
                        break;
                    default:
                        break;
                }

            return mappedValue;
        }
    }

    public static class SettingsMap
    {
        public const string CATEGORY_NAME = "categories";
        public const string SUBCATEGORY_NAME = "subcategories";
        public const string SPECIALTY_NAME = "specialties";
        public const string DEGREELEVEL_NAME = "degreelevels";
        public const string STATE_NAME = "statecode";

        public static Dictionary<string, string> SiteToAdServerMap =
            new Dictionary<string, string>()
            {
                {CATEGORY_NAME, "categories_selections"}
                ,{SUBCATEGORY_NAME,"subcategories_selections"}
                ,{SPECIALTY_NAME,"specialties_selections" }
                ,{DEGREELEVEL_NAME,"desired_degree_level"}
                ,{"campustype", "dynamiccampussoftpreference"}
                ,{"militaryaffiliation","military_affiliation"}
                ,{"uscitizen","us_citizen"}
                ,{"registerednurse","rn_license" }
                ,{"registeredradiology","registered_radiology" }
                ,{"educationdegree","undergraduate_degree_education"}
                ,{"teachingexperience","years_of_teaching_experience" }
                ,{"subid","sub_1"}
                ,{"prospectpostalcode","postal_code"}
                ,{"age","age" }
                ,{"educationlevel","highest_level_of_education_completed" }
                ,{"subsource2","sub_source"}
                ,{"leadsourceurl","lead_initiating_url"}
                ,{STATE_NAME,"geotarget_state" }
                ,{"postalcode","geotarget_zipcode" }
                ,{"postalcoderadiusinmiles","geotarget_zipradius" }
            };

        public static Dictionary<string, string> SiteToFormsEngineMap =
           new Dictionary<string, string>()
           {
               {"categories", "Categories"}
               ,{"subcategories","SubCategories"}
               ,{"specialties","Specialties"}
               ,{"degreelevels","Desired_Degree_Level"}
               ,{"institutionid","InstitutionId"}
               ,{"institutionname","InstitutionName"}
               ,{"campustype","CampusSoftPreference"}
               ,{"startdate","Desired_Start_Date"}
               ,{"leadsourceurl", "LeadSourceUrl"}
               ,{"subsource2","SubSource2"}
               ,{"subid", "sub_1"}
           };

        public static Dictionary<string, string> SiteToAdListingServiceMap =
            new Dictionary<string, string>()
            {
                {"categories", "categories"}
                ,{"subcategories","subcategories"}
                ,{"specialties","specialties" }
                ,{"degreelevels","desiredegreelevel"}
                ,{"campustype", "campuspreference"}
                ,{"militaryaffiliation","military"}
                ,{"uscitizen","us_citizen"}
                ,{"subid","subid"}
                ,{"prospectpostalcode","postal_code"}
                ,{"age","age" }
                ,{"educationlevel","educationlevel" }
                ,{"startdate", "desiredstartdate"}
                ,{"subsource2","subsource"}
                ,{"leadsourceurl","leadinitiatingurl"}
                ,{"statecode","geotarget_state" }
                ,{"postalcode","geotarget_zipcode" }
                ,{"postalcoderadiusinmiles","geotarget_zipradius" }
                ,{"desired_degree_level", "desireDegreeLevel"}
                ,{"year_of_highest_education_completed", "gradYear" }
            };

        public static Dictionary<string, string> SiteToPGListingServiceMap =
            new Dictionary<string, string>()
            {   
                 {"categories_selections", "Categories"}
                ,{"categories", "Categories"}
                ,{"subcategories_selections","Subcategories"}
                ,{"subcategories","Subcategories"}
                ,{"specialties_selections","Specialties" }
                ,{"specialties","Specialties"}
                ,{"highest_level_of_education_completed","EducationLevel" }
                ,{"educationlevel","EducationLevel" }
                ,{"gpa","gpa"}
                ,{"country","country"}
                ,{"state","state"}               
                ,{"campuspreference", "CampusPreference"}
                ,{"dynamiccampussoftpreference","CampusPreference"}
                ,{"military_affiliation","Military"}
                ,{"postal_code","ZipCode"}
                ,{"zipcode","ZipCode"}
                ,{"age","age" }
                ,{"year_of_highest_education_completed", "HighSchoolGradYear" }
                ,{"desired_start_date","DesiredStartDate"}
                ,{"formleadurl","LeadInitiatingUrl"}
                ,{"us_citizen","IsUSCitizen"}
                ,{"isuscitizen","IsUSCitizen"}
                ,{"city","City"}
                ,{"email","Email" }
                ,{"teaching_certificate","teachingCert" }
                ,{"rn_license","rnLicense" }
                ,{"phone","phone" }
                ,{"firstname","firstName"}
                ,{"lastname","lastName"}
                ,{"desireddegreelevel","DesiredDegreeLevel"}
                ,{"desired_degree_level","DesiredDegreeLevel"}
                ,{"schoolsselected","excludedInstitutions"}
                ,{"thank_you_experience","ThankYouExperience"}
                ,{"leadcreatedproduct","LeadCreatedProduct"}
                ,{"formstep","Step"}
                ,{"workflowstep","WorkflowStep"}
                ,{"paidStatusType","PaidStatusType"}
                ,{"LeadSourceUrl","LeadSourceUrl"}
                ,{"FormLeadUrl","FormLeadUrl"}
                ,{"ccne","CCNE"}
                ,{"lpn_license","LPN_License"}
                ,{"christian_faithbased","Christian_FaithBased"}
                ,{"hybrid_location","Hybrid_Location"}
                ,{"undergraduate_degree_nursing","Undergraduate_Degree_Nursing"}
                ,{"undergraduate_degree_grad","Undergraduate_Degree_Grad"}
                ,{"years_of_teaching_experience","Years_of_Teaching_Experience"}
                ,{"years_of_work_experience","Years_of_Work_Experience"}
                ,{"completed_1600_hours_of_clinical_experience","Completed_1600_Hours_Of_Clinical_Experience"}
                ,{"employed_radiology_or_graduated_past_5_years","Employed_Radiology_Or_Graduated_Past_5_Years"}
                ,{"registered_radiology","Registered_Radiology"}
                ,{"registered_and_licensure","Registered_And_Licensure"}
                ,{"gender","Gender"}
                ,{"household_income","Income"}
                ,{"householdincome","Income"}
            };

        public static Dictionary<string, string> ToAdditionalSettingsMap =
            new Dictionary<string, string>()
            {
                {"subsource2","subsource2"}
                ,{"leadsourceurl","leadsourceurl"}
            };
    }
}
