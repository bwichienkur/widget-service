using EDDY.IS.WidgetProvider.Core;
using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Services;
using EDDY.IS.WidgetProvider.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace EDDY.IS.WidgetProvider.Data
{
    public class WidgetRepository : IWidgetRepository
    {
        private readonly NexusContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WidgetRepository> _logger;

        public WidgetRepository(NexusContext context, IConfiguration configuration, ILogger<WidgetRepository> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }
        public VendorWidgetConfig GetWidgetConfig(string containerName, Guid vendorWidgetToken)
        {
            VendorWidgetConfig widgetConfig = null;

            List<VwVendorWidgetConfiguration> configList = _context.VwVendorWidgetConfiguration.Where(wc => wc.WidgetServiceToken == vendorWidgetToken && wc.VendorWidgetName == containerName).ToList();

            if(configList.Any())
            {
                widgetConfig = new VendorWidgetConfig();

                widgetConfig.VendorWidgetId = configList.First().VendorWidgetId;
                widgetConfig.WidgetContainerName = configList.First().VendorWidgetName;
                widgetConfig.WidgetType = (WidgetType)configList.First().WidgetId;
                widgetConfig.CSSText = configList.First().CustomCSS;

                widgetConfig.SystemSettings = new Dictionary<WidgetComponentType,Dictionary<string, string>>();

                var componentGrouping = configList.GroupBy(c => c.WidgetComponentId);

                foreach (var component in componentGrouping)
                {
                    var settingDict = new Dictionary<string, string>();

                    foreach (var config in configList)
                    {
                        settingDict.Add(config.WidgetSettingKey, config.SettingValue);
                    }

                    widgetConfig.SystemSettings.Add((WidgetComponentType)component.First().WidgetComponentId, settingDict);
                }
            }

            return widgetConfig;
        }

    
        public void SaveWidgetImpression(Guid widgetRequestGuid)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<EddyTrackingISContext>();
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("EddyTrackingISConnection"));

                using (EddyTrackingISContext trackingContext = new EddyTrackingISContext(optionsBuilder.Options))
                {
                    trackingContext.Add(new WidgetImpression() { WidgetRequestGuid = widgetRequestGuid, CreatedDate = DateTime.Now });
                    trackingContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public void SaveWidgetRequests(Dictionary<WidgetType, List<VendorWidgetConfig>> dicVendorWidgetConfig, Core.DTO.WidgetRequest request, long widgetRenderMilliseconds)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<EddyTrackingISContext>();
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("EddyTrackingISConnection"));

                int impressionSaveCount = 0;                

                using (EddyTrackingISContext trackingContext = new EddyTrackingISContext(optionsBuilder.Options))
                {
                    foreach (var widgetType in dicVendorWidgetConfig)
                    {
                        foreach (var config in widgetType.Value)
                        {
                            if (!request.PageUrl.Contains("admin.educationdynamics.com"))
                            {
                                Entities.WidgetRequest requestEntity = new Entities.WidgetRequest();

                                Tuple<string, string> pageUrlSplit = SplitURL(request.PageUrl);
                                Tuple<string, string> referrerUrlSplit = SplitURL(request.ReferrerUrl);
                                
                                requestEntity.CreatedDate = DateTime.Now;

                                requestEntity.JqueryVersionNumber = request.JqueryVersionNumber;
                                requestEntity.WidgetRequestGuid = request.WidgetRequestGuid;
                                requestEntity.Ipaddress = request.IPAddress;
                                requestEntity.PageUrl = pageUrlSplit.Item1;
                                requestEntity.PageQueryString = pageUrlSplit.Item2;
                                requestEntity.UserAgent = request.UserAgent;
                                requestEntity.VendorWidgetId = config.VendorWidgetId;
                                requestEntity.RenderTimeMilliseconds = widgetRenderMilliseconds;

                                if (referrerUrlSplit != null)
                                {
                                    requestEntity.ReferringUrl = referrerUrlSplit.Item1;
                                    requestEntity.ReferringQueryString = referrerUrlSplit.Item2;
                                }

                                if (config.SiteSettings != null && config.SiteSettings.Any())
                                {
                                    requestEntity.WidgetSettingsJson = JsonSerializer.Serialize(config.SiteSettings);
                                }

                                impressionSaveCount++;
                                trackingContext.Add(requestEntity);
                            }
                        }
                    }

                    if(impressionSaveCount > 0)
                        trackingContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public Dictionary<string, Dictionary<string, string>> GetVendorUrlConfigurations()
        {
            Dictionary<string, Dictionary<string, string>> returnValue = new Dictionary<string, Dictionary<string, string>>();
            List<VendorWidgetUrlParameterConfig> configs = _context.VendorWidgetUrlParameterConfig.Where(url => url.IsEnabled == true).ToList();

            foreach (var c in configs)
            {
                if (!returnValue.ContainsKey(c.Url))
                {
                    Dictionary<string, string> configStrings = new Dictionary<string, string>();

                    if (!String.IsNullOrEmpty(c.Categories)) configStrings.Add(SettingsMap.CATEGORY_NAME, c.Categories);
                    if (!String.IsNullOrEmpty(c.SubCategories)) configStrings.Add(SettingsMap.SUBCATEGORY_NAME, c.SubCategories);
                    if (!String.IsNullOrEmpty(c.Specialties)) configStrings.Add(SettingsMap.SPECIALTY_NAME, c.Specialties);
                    if (!String.IsNullOrEmpty(c.States)) configStrings.Add(SettingsMap.STATE_NAME, c.States);
                    if (!String.IsNullOrEmpty(c.DegreeLevels)) configStrings.Add(SettingsMap.DEGREELEVEL_NAME, c.DegreeLevels);

                    returnValue.Add(c.Url, configStrings);
                }
            }

            return returnValue;
        }

        public QDFTemplate GetQDFTemplate(int templateId)
        {
            QDFTemplate template = null;

            List<VwQDFTemplateConfiguration> templateList = _context.VwQDFTemplateConfiguration.Where(t => t.TemplateId == templateId).OrderBy(o => o.TemplateStepId).ThenBy(o => o.RowSequence).ToList();
            List<MatchingInputField> matchingEngineFields = new List<MatchingInputField>();

            if (templateList.Any())
            {
                template = new QDFTemplate();

                template.Header = templateList.First().Header;
                template.SubHeading = templateList.First().SubHeading;
                template.TemplateId = templateList.First().TemplateId;
                template.TemplateName = templateList.First().TemplateName;

                template.Fields = new List<QDFField>();

                for(int i = 0; i < templateList.Count; i++)
                {
                    var question = templateList[i];

                    QDFField field = new QDFField();

                    field.Code = question.Code;
                    field.FieldName = question.FieldName;
                    field.IsRequired = question.IsRequired.HasValue ? question.IsRequired.Value : false;
                    field.Label = question.InstanceLabel;
                    field.Watermark = String.IsNullOrEmpty(question.InstanceWatermark) ? "Select one of the following..." : question.InstanceWatermark;

                    if (question.StandardControlTypeName == "Text Box")
                        field.ControlType = QDFFieldType.Textbox;
                    else if (question.StandardControlTypeName == "Radio Buttons")
                        field.ControlType = QDFFieldType.RadioButtons;
                    else if (question.StandardControlTypeName == "Drop-Down")
                        field.ControlType = QDFFieldType.Dropdown;
                    else if (question.StandardControlTypeName == "Multi Select Drop-Down")
                        field.ControlType = QDFFieldType.DropdownMultiple;
                    else
                        field.ControlType = QDFFieldType.Hidden;

                    switch (question.Code)
                    {
                        case "Desired_Degree_Level":
                            field.MEInput = MatchingEngineInput.ProgramLevel;
                            break;
                        case "Categories":
                            field.MEInput = MatchingEngineInput.Category;
                            break;
                        case "SubCategories":
                            field.MEInput = MatchingEngineInput.Subject;
                            break;
                        case "Specialties":
                            field.MEInput = MatchingEngineInput.Specialty;
                            break;
                    }

                    field.UpdateFields = FieldsToUpdate(templateList, i + 1);
                    field.Predecessors = new List<MatchingInputField>(matchingEngineFields);

                    matchingEngineFields.Add(new MatchingInputField() { Code = field.Code, MEInput = field.MEInput });
                    
                    template.Fields.Add(field);
                }
            }

            return template;
        }

        private List<string> FieldsToUpdate(List<VwQDFTemplateConfiguration> templateList, int startIndex)
        {
            List<string> fieldsToUpdate = new List<string>();

            for (int i = startIndex; i < templateList.Count; i++)
            {
                switch (templateList[i].Code)
                {
                    case "Desired_Degree_Level":
                    case "Categories":
                    case "SubCategories":
                    case "Specialties":
                        fieldsToUpdate.Add(templateList[i].Code);
                        break;
                }
            }

            return fieldsToUpdate;
        }

        private Tuple<string, string> SplitURL(string fullUrl)
        {
            if (String.IsNullOrWhiteSpace(fullUrl))
                return null;

            string[] splitUrl = fullUrl.Split('?');

            string pageURL = splitUrl[0];
            string queryString;

            if (splitUrl.Length == 2)
                queryString = splitUrl[1];
            else
                queryString = null;

            return new Tuple<string, string>(pageURL, queryString);
        }
    }
}
