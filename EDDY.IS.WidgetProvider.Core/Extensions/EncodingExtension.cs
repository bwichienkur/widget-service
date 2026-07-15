using System.Web;

namespace EDDY.IS.WidgetProvider.Core.Extensions
{
    public static class EncodingExtension
    {
        public static string CleanContent(this string content) 
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            return HttpUtility.JavaScriptStringEncode(content.EscapeNewLines());
        }

        public static string EscapeNewLines(this string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            return content.Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Replace("  ", "").Replace("\t", "");
        }

        public static string EscapeContent(this string content) 
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            return content.Replace("\"","\\\"");
        }

        public static string RemoveSingleQuote(this string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            return content.Replace("'", "");
        }

    }
}
