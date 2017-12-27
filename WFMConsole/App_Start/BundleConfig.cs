using System.Web;
using System.Web.Optimization;

namespace WFMDashboard
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/libraries/jquery-{version}.js",
                        "~/Scripts/libraries/DataTables-1.10.13/js/jquery.dataTables.min.js",
                        "~/Scripts/libraries/chosen_v1.8.2/chosen.jquery.js",
                        "~/Scripts/libraries/fullcalendar-3.6.1/lib/moment.min.js",
                        "~/Scripts/libraries/fullcalendar-3.6.1/fullcalendar.min.js",
                        "~/Scripts/libraries/jquery.timepicker.min.js",
                        "~/Scripts/libraries/jquery-ui-1.12.1.custom/jquery-ui.js",
                        "~/Scripts/libraries/jquery.datepair.min.js"
                        ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/libraries/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/libraries/bootstrap.js",
                      "~/Scripts/libraries/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/jquery.timepicker.min.css",
                      "~/Scripts/libraries/DataTables-1.10.13/css/jquery.dataTables.min.css",
                      "~/Scripts/libraries/fullcalendar-3.6.1/fullcalendar.min.css",
                      "~/Scripts/libraries/jquery-ui-1.12.1.custom/jquery-ui.css",
                      "~/Content/site.css"));
        }
    }
}
