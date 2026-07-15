using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Service.Controllers
{
    public class HeaderController : Controller
    {
        public IActionResult Index()
        {
            var values = new List<string>();
            try
            {
                foreach (var header in Request.Headers.Keys)
                {
                    values.Add($"{header} => {Request.Headers[header]}");
                }
                if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_VIA")))
                    values.Add($"SV:HTTP_VIA  => {HttpContext.Request.HttpContext.GetServerVariable("HTTP_VIA")}");
                
                if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_TRUE_CLIENT_IP")))
                    values.Add($"SV:HTTP_TRUE_CLIENT_IP  => {HttpContext.Request.HttpContext.GetServerVariable("HTTP_TRUE_CLIENT_IP")}");

                if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")) && HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").ToLower() != "unknown")
                    values.Add($"SV:HTTP_X_FORWARDED_FOR  => {HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")}");

                if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")) && HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").ToLower() != "unknown")
                    values.Add($"SV:HTTP_X_FORWARDED_FOR  => {HttpContext.Request.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")}");

                if (!String.IsNullOrWhiteSpace(HttpContext.Request.HttpContext.GetServerVariable("REMOTE_ADDR")))
                    values.Add($"SV:REMOTE_ADDR  => {HttpContext.Request.HttpContext.GetServerVariable("REMOTE_ADDR")}");

                values.Add($"Derived IP => {GetClientIPAddress()}");
            }
            catch (Exception ex)
            {
                values.Add(ex.Message);
            }

            return View(values);
        }

        private string GetClientIPAddress()
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                //Get the client ip from Cloudflare
                if (!string.IsNullOrWhiteSpace(HttpContext.Request.Headers["True-Client-IP"]))
                {
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
