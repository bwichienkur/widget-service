using EDDY.IS.WidgetProvider.Core.ComponentModels;
using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Entities;
using EDDY.IS.WidgetProvider.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EDDY.IS.WidgetProvider.Core.Services
{
    public class ModelInstantiationService : IModelInstantiationService
	{
        private readonly IConfiguration _configuration;
        private readonly IWidgetRepository _widgetRepository;
        private readonly IQDFService _qdfService;
        private readonly IFileSerializeService _fileSerializeService;
        private readonly IServiceProvider _services;
        private readonly IFESessionRedisService _feSessionRedisService;

        public ModelInstantiationService(IConfiguration configuration, 
            IWidgetRepository repository,
            IQDFService qdfService,
            IFileSerializeService fileSerializeService,
            IServiceProvider services,
            IFESessionRedisService feSessionRedisService)
        {
            _configuration = configuration;
            _widgetRepository = repository;
            _qdfService = qdfService;
            _fileSerializeService = fileSerializeService;
            _services = services;
            _feSessionRedisService = feSessionRedisService;
        }

        public IRenderable CreateComponent(WidgetType widgetType, WidgetRequest widgetRequest)
		{
            IRenderable component = null;

            switch (widgetType)
            {
                case WidgetType.AdStackSolo:
                    component = new AdStackSoloModel(_configuration);
                    break;
                case WidgetType.SmartListing:
                    component = new SmartListingModel(_configuration);
                    break;
                case WidgetType.QDF:
                    component = new QDFModel(_configuration);
                    break;
                case WidgetType.WizardForm:
                case WidgetType.ProgramForm:
                    component = new WizardFormModel(_configuration, widgetType);
                    break;
                case WidgetType.LeaveBehind:
                    component = new LeaveBehindModel();
                    break;
                case WidgetType.AdListingApi:
                    component = new AdListingApiModel(_configuration, _fileSerializeService, widgetRequest.LoadExternalResources);
                    break;
                case WidgetType.GPListingApi:
                    var service = _services.GetService<IGPListingApiService>();
                    component = new GPListingApiModel(_configuration, service, _fileSerializeService, _feSessionRedisService, widgetRequest.LoadExternalResources);
                    break;
                case WidgetType.QDFLight:
                    component = new QDFLightModel(_configuration, _widgetRepository, _qdfService, _fileSerializeService);
                    break;
                case WidgetType.ExitPop:
                    component = new ExitPopModel(_configuration, _fileSerializeService, widgetRequest);
                    break;
                case WidgetType.GPQDFLight:
                    component = new GPQDFLightModel(_configuration, _widgetRepository, _qdfService, _fileSerializeService);
                    break;
                case WidgetType.GPExitPop:
                    component = new GPExitPopModel(_configuration, _fileSerializeService, widgetRequest);
                    break;
                default:
                    break;
            }

            return component;
        }
	}
}
