using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WFMConsole.Classes;
using WFMDashboard.Classes;

namespace WFMDashboard.Controllers
{
    public class HomeController : Controller
    {
        //public ActionResult Index()
        //{
        //    ViewBag.Title = "WFM Console";

        //    return View();
        //}
        static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetTeamInfo()
        {
            string msg = "";
            bool success = false;
            var info = WFMHelper.GetTeamInfo();
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        public async Task<string> SubmitTimeOffForm(CancellationToken cancellationToken, int id, string name, string date, bool fullDay, string startTime, string endTime, string notes)
        {
            string msg = "";
            bool success = false;
            var googleAuth = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            success = WFMHelper.SubmitTimeOff(googleAuth, date, fullDay, startTime, endTime, notes, out msg);
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        public string CreateDownByReport()
        {
            string msg = "";
            bool success = false;
            success = WFMHelper.CreateDownByReport();
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        public string CreateMOWReport()
        {
            string msg = "";
            bool success = false;
            success = WFMHelper.CreateMOWReport();
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }


        //From google API documentation:

        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "ASP.NET MVC Sample"
                });

                

                // YOUR CODE SHOULD BE HERE..
                // SAMPLE CODE:
                //var list = await service.Files.List().ExecuteAsync();
                //ViewBag.Message = "FILE COUNT IS: " + list.Items.Count();
                ViewBag.Title = "WFM Dashboard A";
                return View();
            }
            else
            {
                ViewBag.Title = "WFM Dashboard B";
                //return View();
                return new RedirectResult(result.RedirectUri);
            }
        }
    }
}
