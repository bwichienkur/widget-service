using EDDY.IS.WidgetProvider.Core;
using EDDY.IS.WidgetProvider.Core.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WidgetProviderController : ControllerBase
    {
        private readonly IWidgetPackageService _widgetPackageService;
        private readonly IFileSerializeService _fileSerializeService;
        private readonly ILogger<WidgetProviderController> _logger;

        public WidgetProviderController(IWidgetPackageService widgetPackageService, IFileSerializeService fileSerializeService, ILogger<WidgetProviderController> logger)
        {
            _widgetPackageService = widgetPackageService;
            _fileSerializeService = fileSerializeService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult GetWidgetJs(string vendorToken, bool checkJquery = true)
        {
            string content = "";
            try
            {
                //TODO: validate token and if fails return error response
                Guid vendorGuid;

                if (!Guid.TryParse(vendorToken, out vendorGuid))
                {
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return Content("WidgetError: Invalid Vendor Token.");
                }

                content = $"var checkJquery = {checkJquery.ToString().ToLower()};";

                content += $"var globalVendorGuid = '{vendorGuid.ToString()}';"; 

                content += _fileSerializeService.GetFileContents("js", "eddywidget.min.js");

                content += $"widget_setCookie('EddyVendorToken', '{vendorGuid.ToString()}');";                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                //Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Content($"WidgetError: { ex.Message}");
            }

            Response.Headers.Add("Access-Control-Max-Age", "600");
            return Content(content, "text/javascript", System.Text.Encoding.UTF8);
        }

        [HttpPost]
        public async Task<ActionResult> GetWidgetPackage(WidgetRequest widgetConfig)
        {
            var content = string.Empty;
            
            try
            {
                widgetConfig.IPAddress = GetClientIPAddress();
                widgetConfig.WidgetRequestGuid = Guid.NewGuid();

                var cookieTrackId = widgetConfig.CookieTrackId;

                if (cookieTrackId.HasValue)
                {
                    widgetConfig.TrackId = cookieTrackId.Value;
                }

                content = await _widgetPackageService.GetFullWidgetPackage(widgetConfig);

                content += $"widget_setCookie('EddyWidgetSession', '{widgetConfig.WidgetRequestGuid.ToString()}');";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                //Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
                return Content($"WidgetError: { ex.Message}");
            }

            Response.Headers.Add("Access-Control-Max-Age", "600");
            return Content(content, "text/html", System.Text.Encoding.UTF8);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateWidgetPackage([FromBody] WidgetRequest widgetConfig)
        {
            try
            {
                widgetConfig.IPAddress = GetClientIPAddress();
                widgetConfig.WidgetRequestGuid = Guid.NewGuid();
                widgetConfig.LoadExternalResources = false;
                widgetConfig.UpdateWidget = true;
                Guid? cookieTrackId = widgetConfig.CookieTrackId;
                if (cookieTrackId.HasValue)
                    widgetConfig.TrackId = cookieTrackId.Value;

                string content = await _widgetPackageService.GetFullWidgetPackage(widgetConfig);

                return Content(content, "text/html", System.Text.Encoding.UTF8);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Content("WidgetError");
            }
        }

        [HttpGet]
        public void SaveWidgetImpression(Guid widgetSessionGuid)
        {
            try
            {
                _widgetPackageService.SaveWidgetImpression(widgetSessionGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private string GetClientIPAddress()
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                //Get the client ip from Cloudflare
                if (!string.IsNullOrWhiteSpace(HttpContext.Request.Headers["True-Client-IP"])) {
                    ipAddress = HttpContext.Request.Headers["True-Client-IP"];
                    return ipAddress;
                }

                if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_VIA")))
                {
                    if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_TRUE_CLIENT_IP")))
                        ipAddress = HttpContext.Request.HttpContext.GetServerVariable("HTTP_TRUE_CLIENT_IP");
                    else if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")) && HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").ToLower() != "unknown")
                        ipAddress = !HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Contains(",") ? HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR") : HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Split(',').Last();
                }
                else if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")) && HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").ToLower() != "unknown")
                    ipAddress = !HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Contains(",") ? HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR") : HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Split(',').Last();
                else if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("REMOTE_ADDR")))
                    ipAddress = HttpContext.Request.HttpContext.GetServerVariable("REMOTE_ADDR");
            }
            catch { }

			if (!String.IsNullOrEmpty(ipAddress) && ipAddress.Contains(':'))
				ipAddress = ipAddress.Substring(0, ipAddress.LastIndexOf(':'));

			return ipAddress;
        }
    }
}
