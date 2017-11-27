﻿using System.Web;
using System.Web.Optimization;

namespace WFMDashboard
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/DataTables-1.10.13/js/jquery.dataTables.min.js",
                        "~/Scripts/chosen_v1.8.2/chosen.jquery.js",
                        "~/Scripts/fullcalendar-3.6.1/lib/moment.min.js",
                        "~/Scripts/fullcalendar-3.6.1/fullcalendar.min.js",
                        "~/Scripts/jquery.timepicker.min.js",
                        "~/Scripts/jquery-ui-1.12.1.custom/jquery-ui.js",
                        "~/Scripts/jquery.datepair.min.js"
                        ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/libraries").Include(
                        //"~/Scripts/tinymce/tinymce.js",
                        //"~/Scripts/tinymce/plugins/textcolor/plugin.js",
                        //"~/Scripts/tinymce/plugins/paste/plugin.js",
                        //"~/Scripts/tinymce/plugins/colorpicker/plugin.js",
                        //"~/Scripts/tinymce/plugins/link/plugin.js",
                        //"~/Scripts/tinymce/plugins/lists/plugin.js",
                        //"~/Scripts/tinymce/plugins/preview/plugin.js",
                        //"~/Scripts/tinymce/plugins/searchreplace/plugin.js",
                        //"~/Scripts/tinymce/plugins/autolink/plugin.js",
                        //"~/Scripts/tinymce/plugins/image/plugin.js",
                        //"~/Scripts/tinymce/plugins/code/plugin.js",
                        //"~/Scripts/tinymce/plugins/table/plugin.js",
                        //"~/Scripts/tinymce/themes/modern/theme.js"

                        ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/jquery.timepicker.min.css",
                      "~/Scripts/DataTables-1.10.13/css/jquery.dataTables.min.css",
                      "~/Scripts/fullcalendar-3.6.1/fullcalendar.min.css",
                      "~/Scripts/jquery-ui-1.12.1.custom/jquery-ui.css",
                      "~/Content/site.css"));
        }
    }
}
