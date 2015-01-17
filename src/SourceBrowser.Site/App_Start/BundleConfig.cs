using System.Web;
using System.Web.Optimization;

namespace SourceBrowser.Site
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-2.1.1.js",
                        "~/Scripts/jquery-ui-1.11.1.js",
                        "~/Scripts/jquery.widget.js"
                        ));

            /* For now, this bundle is empty and not used.
            bundles.Add(new ScriptBundle("~/bundles/sourcebrowser").Include(
                        "~/Scripts/TODO.js"));
                        */

            bundles.Add(new ScriptBundle("~/bundles/treeViewScripts").Include(
                                    "~/Scripts/treeViewHelpers.js"));

            bundles.Add(new ScriptBundle("~/bundles/search").Include(
                                    "~/Scripts/history.js/history.adapter.jquery.js",
                                    "~/Scripts/history.js/history.js",
                                    "~/Scripts/history.js/history.html4.js",
                                    "~/Scripts/search.js"));

            bundles.Add(new ScriptBundle("~/bundles/upload").Include(
                                    "~/Scripts/upload.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/js/metro.min.js"));

            //Metro UI files
            //bundles.Add(new ScriptBundle("~/bundles/metro").Include(
            //    "~/js/metro.min.js"));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/css/metro-bootstrap.css",
                      "~/css/metro-bootstrap-responsive.css",
                      "~/Content/sourcebrowser-browse.css",
                      "~/Content/sourcebrowser-codebrowser.css",
                      "~/Content/sourcebrowser-upload.css",
                      "~/Content/site.css"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}

