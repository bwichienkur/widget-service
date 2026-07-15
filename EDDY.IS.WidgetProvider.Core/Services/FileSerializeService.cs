using EDDY.IS.WidgetProvider.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Services
{
    public class FileSerializeService : IFileSerializeService
    {
        private readonly IConfiguration _configuration;
        private readonly IMinificationService _minificationService;
        public FileSerializeService(IMinificationService minificationService, IConfiguration configuration)
        {
            _configuration = configuration;
            _minificationService = minificationService;
        }

        public string GetFileContents(string directory, string fileName)
        {
            string fileText = "";
            
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), directory, fileName);

            fileText = File.ReadAllText(filePath);

            fileText = ReplaceTokens(fileText);

            if (Convert.ToBoolean(_configuration["MinifyJavascript"]))
                fileText = _minificationService.MinifyJavascript(fileText);

            return fileText;
        }

        private string ReplaceTokens(string fileText)
        {
            string replacedFileText = fileText;

            replacedFileText = replacedFileText.Replace("##SERVICEURL##", _configuration.GetSection("WidgetJsReplaceValues")["SERVICEURL"]);
            replacedFileText = replacedFileText.Replace("##JQUERY_VERSION##", _configuration.GetSection("WidgetJsReplaceValues")["JQUERY_VERSION"]);

            return replacedFileText;
        }
    }
}
