using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using KonicaMinolta.Authentication.Identity;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            log.Info("Accessing WFM Dashboard");
            var user = HttpContext.KmIdentity();
            var result = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            if (result.Credential != null)
            {
                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "ASP.NET MVC Sample"
                });
                ViewBag.Title = "WFM Dashboard";
                return View();
            }
            else
            {
                log.Info($"User {user} is logging into google calendar");
                ViewBag.Title = "WFM Dashboard";
                return new RedirectResult(result.RedirectUri);
            }
        }

        public string GetPageInfo()
        {
            var msg = "";
            string downBy, mow;
            WFMHelper.GetReportDates(out downBy, out mow);
            var list =  WFMHelper.GetStaffList(out msg);
            var eventList = WFMHelper.GetEventList();
            return JsonConvert.SerializeObject(new { success = msg.ToLower().Contains("success"), msg = msg, agentList = list, downBy = downBy, mow = mow, eventList = eventList });
        }

        public string GetTeamInfo(int agentNo)
        {
            string msg = "";
            //bool success = false;
            var teamInfo = WFMHelper.GetTeamInfo(agentNo);
            return JsonConvert.SerializeObject(new { success = teamInfo != null, msg = msg, teamInfo = teamInfo });
        }


        public async Task<string> SubmitEventForm(CancellationToken cancellationToken, int id, string title, string color, string startDate, string endDate, bool fullDay, string startTime, string endTime, string notes, string eventType)
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} called SubmitEventForm - Params: /r/n id: {id} \r\n title: {title} \r\n start date: {startDate} \r\n end date: {endDate} \r\n fullDay: {fullDay} \r\n startTime: {startTime} \r\n endTime: {endTime} \r\n notes: {notes}");
            string msg = "";
            //bool success = false;
            var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            var newEvent = WFMHelper.SubmitEventForm(googleAuth, id, title, color, startDate, endDate, fullDay, startTime, endTime, notes, eventType, out msg);
            return JsonConvert.SerializeObject(new { success = newEvent != null, msg = msg, newEvent = newEvent });
        }

        public string DeleteEvent(int id)
        {
            string msg = "";
            var success = WFMHelper.DeleteEvent(id, out msg);
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        #region Reports

        //Daily at 9:30 and in afternoon (once from 9-10, once at 12:30)
        //It is sent to: WFO, CSM Management. We pull the "Today's Scheduled Tasks and Events" information directly from the Gcal.
        //public async Task<string> CreateDownByReport(CancellationToken cancellationToken)
        //{
        //    var user = HttpContext.KmIdentity();
        //    log.Info($"User {user} created Down By Report");
        //    string msg = "";
        //    bool success = false;
        //    var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
        //    success = WFMHelper.CreateDownByReport(googleAuth);
        //    return JsonConvert.SerializeObject(new { success = success, msg = msg });
        //}

        public async Task<ActionResult> CreateDownByReport(CancellationToken cancellationToken)
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} created Down By Report");
            string msg = "";
            bool success = false;
            var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            var report = WFMHelper.CreateDownByReport(googleAuth);

            if (report != null)
            {
                ViewBag.report = report;

                return View("~/Views/Mailer/DownByReport.cshtml");
            }
            else
            {
                return View("Index");
            }
        }

        public string CreateMOWReport()
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} created MOW Report");
            string msg = "";
            bool success = false;
            success = WFMHelper.CreateMOWReport();
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        #endregion

        //Google calendar
        public async Task<ActionResult> LogOutGoogle(CancellationToken cancellationToken)
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} logged out of google calendar");
            await EFDataStore.ClearAsyncStatic();

            return Redirect(ConfigurationManager.AppSettings["LogOutRedirect"]);
        }

    }
}
