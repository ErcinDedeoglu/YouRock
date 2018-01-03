using Microsoft.AspNetCore.Mvc.Rendering;

namespace YouRock
{
    public class URLHelper
    {
        public static string ActionName(ViewContext context)
        {
            return context.RouteData.Values["Action"].ToString();
        }

        public static string ControllerName(ViewContext context)
        {
            return context.RouteData.Values["Controller"].ToString();
        }

        public static string JavascriptHead(ViewContext context)
        {
            return ControllerName(context) + "_" + ActionName(context);
        }
    }
}