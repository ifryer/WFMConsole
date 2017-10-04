using log4net;
using log4net.Appender;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WFMDashboard
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            configureLogForNet();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            Response.Clear();
            Server.ClearError();
            log.Error("Unhandled", ex);
            Response.StatusCode = 500;
            //            Response.Redirect("~/Error", false);
        }

        private void configureLogForNet()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(HttpContext.Current.Server.MapPath("~/log4net.config")));

            foreach (IAppender appender in LogManager.GetRepository().GetAppenders())
            {
                if (typeof(SmtpAppender) == appender.GetType())
                {
                    ((SmtpAppender)appender).Subject = string.Format("Unexpected exception occurred in WFM Dash on {0}", HttpContext.Current.Server.MachineName);
                    ((SmtpAppender)appender).ActivateOptions();
                }
            }
            log.InfoFormat("Started log4net on {0} with {1} extenders", HttpContext.Current.Server.MachineName, LogManager.GetCurrentLoggers().Count());
        }
    }
}
