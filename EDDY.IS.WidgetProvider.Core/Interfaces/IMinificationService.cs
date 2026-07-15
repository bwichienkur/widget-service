using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface IMinificationService
    {
        string MinifyJavascript(string javaScript);
    }
}
