using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SourceBrowser.Site
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.RouteExistingFiles = true;
            routes.AppendTrailingSlash = true;

            routes.MapRoute(
                name: "BrowseFile",
                url: "browse/{*id}",
                defaults: new
            {
                controller = "Browse",
                action = "LookupFile",
                id = UrlParameter.Optional
            }
                ,constraints: new { id = @".*\.cs" }
            );
            routes.MapRoute(
                name: "Browse",
                url: "browse/{*id}",
                defaults: new
            {
                controller = "Browse",
                action = "LookupFolder",
                id = UrlParameter.Optional
            }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
