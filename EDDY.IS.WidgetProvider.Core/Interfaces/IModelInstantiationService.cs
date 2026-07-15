using EDDY.IS.WidgetProvider.Core.DTO;
using EDDY.IS.WidgetProvider.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core
{
	public interface IModelInstantiationService
	{
		IRenderable CreateComponent(WidgetType widgetType, WidgetRequest widgetRequest);
	}
}
