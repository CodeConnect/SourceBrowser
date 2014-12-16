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
                name: "BrowseUser",
                url: "browse/{username}",
                defaults: new
            {
                controller = "Browse",
                action = "LookupUser",
            }
            );

           routes.MapRoute(
                name: "BrowseRepo",
                url: "browse/{username}/{repository}",
                defaults: new
            {
                controller = "Browse",
                action = "LookupRepo",
            }
            );


            routes.MapRoute(
                name: "BrowseFileAjax",
                url: "browse/{username}/{repository}/{*path}",
                defaults: new
                {
                    controller = "Browse",
                    action = "LookupFileAjax",
                }
                , constraints: new { path = @".*\.cs", test = new Attributes.AjaxOnlyConstraint() }
            );

            routes.MapRoute(
                name: "BrowseFile",
                url: "browse/{username}/{repository}/{*path}",
                defaults: new
            {
                controller = "Browse",
                action = "LookupFile",

            }
                , constraints: new { path = @".*(\.cs|\.vb)" }
            );


            routes.MapRoute(
                name: "BrowseFolder",
                url: "browse/{username}/{repository}/{*path}",
                defaults: new
            {
                controller = "Browse",
                action = "LookupFolder",
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
