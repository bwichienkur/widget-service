using Newtonsoft.Json;
using NUglify;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Services
{
    public class MinificationService : IMinificationService
    {
        public string MinifyJavascript(string javaScript)
        {
            string minifiedScript = string.Empty;

            if (!string.IsNullOrWhiteSpace(javaScript))
            {
                var settings = new NUglify.JavaScript.CodeSettings();

                UglifyResult minifiedJsResult = Uglify.Js(javaScript, settings);

                minifiedScript = minifiedJsResult.Code ?? string.Empty;

                if (minifiedJsResult.HasErrors)
                {
                    string errorJson = JsonConvert.SerializeObject(minifiedJsResult.Errors, Formatting.Indented);
                    minifiedScript += $"<!-- Rendering Errors: {errorJson} -->";
                }
            }

            return minifiedScript;
        }
    }
}
