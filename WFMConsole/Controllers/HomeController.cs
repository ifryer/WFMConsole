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
using WFMConsole.ViewModels;
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
            string downByDate, mowDate;
            WFMHelper.GetReportDates(out downByDate, out mowDate);
            var agentList =  WFMHelper.GetStaffList(out msg);
            var eventList = WFMHelper.GetEventList();
            var managerList = WFMHelper.GetManagerList(agentList); //TODO: Remove this?
            var mowList = WFMHelper.GetMowList(agentList);

            var lateShift = WFMHelper.GetLateShift();

            var icmSchedule = WFMHelper.GetIcmSchedule();
            var mowSchedule = WFMHelper.GetMowSchedule(DateTimeExtensions.StartOfWeek(DateTime.Now, DayOfWeek.Monday));

            var latestIcmInfo = WFMHelper.GetLatestIcmInfo(icmSchedule.Last().Value.Last());

            //var latestMowInfo = WFMHelper.GetLatestMowInfo();

            return JsonConvert.SerializeObject(new { success = msg.ToLower().Contains("success"), msg = msg, downByDate = downByDate, mowDate = mowDate, eventList = eventList, agentList = agentList, mowList = mowList, managerList = managerList, lateShift = lateShift, icmSchedule = icmSchedule, mowSchedule = mowSchedule, latestIcmInfo = latestIcmInfo });
        }

        public string GetTeamInfo(int agentNo)
        {
            string msg = "";
            //bool success = false;
            var teamInfo = WFMHelper.GetTeamInfo(agentNo);
            return JsonConvert.SerializeObject(new { success = teamInfo != null, msg = msg, teamInfo = teamInfo });
        }


        public async Task<string> SubmitEventForm(CancellationToken cancellationToken, int eventId, int agentId, string title, string color, string startDate, string endDate, bool fullDay, string startTime, string endTime, string notes, string eventType)
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} called SubmitEventForm - Params: /r/n agentId: {agentId} \r\n title: {title} \r\n start date: {startDate} \r\n end date: {endDate} \r\n fullDay: {fullDay} \r\n startTime: {startTime} \r\n endTime: {endTime} \r\n notes: {notes}");
            string msg = "";
            //bool success = false;
            var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            ViewEvent eventObject = WFMHelper.SubmitEventForm(googleAuth, eventId, agentId, title, color, startDate, endDate, fullDay, startTime, endTime, notes, eventType, out msg);
            return JsonConvert.SerializeObject(new { success = eventObject != null, msg = msg, eventObject = eventObject });
        }

        public async Task<string> DeleteEvent(CancellationToken cancellationToken, int id)
        {
            var user = HttpContext.KmIdentity();
            string msg = "";
            log.Info($"User {user} called DeleteEvent /r/n id: {id}");
            var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            var success = WFMHelper.DeleteEvent(googleAuth, id, out msg);
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

        public async Task<ActionResult> CreateMOWReport()
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} created MOW Report");
            string msg = "";
            bool success = false;
            var report = WFMHelper.CreateMOWReport();

            if (report != null)
            {
                ViewBag.report = report;

                return View("~/Views/Mailer/MowReport.cshtml");
            }
            else
            {
                return View("Index");
            }
            //success = WFMHelper.CreateMOWReport();
            //return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        #endregion


        #region MOW Tab

        public string GetMowScheduleWeek(string mondayString)
        {
            var monday = DateTime.Parse(mondayString);
            var mowSchedule = WFMHelper.GetMowSchedule(monday);
            return JsonConvert.SerializeObject(new { mowSchedule = mowSchedule, success = mowSchedule != null});
        }

        public string SubmitIcmForm(int month, int year, int agentNo)
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} called SubmitIcmForm - Params: /r/n month: {month} \r\n year: {year} \r\n agentNo: {agentNo}");
            string msg;
            bool success;
            var icmSchedule = WFMHelper.SubmitIcmForm(month, year, agentNo, out msg, out success);
            //var latestIcmInfo = WFMHelper.GetLatestIcmInfo(icmSchedule.Last().Value.Last());
            //return JsonConvert.SerializeObject(new { success = success, msg = msg, icmSchedule = icmSchedule, latestIcmInfo = latestIcmInfo });
            return JsonConvert.SerializeObject(new { success = success, msg = msg, icmSchedule = icmSchedule });
        }

        public string DeleteMowRow(int rowId)
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} called DeleteMowRow - Params: /r/n id: {rowId}");
            string msg;
            var success = WFMHelper.DeleteMowRow(rowId, out msg);
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        public string SubmitMowForm(MowFormInput Item, string MondayString)
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} called SubmitMowForm");
            string msg;
            bool success;
            var mowSchedule = WFMHelper.SubmitMowForm(Item, MondayString, out msg, out success);
            //var icmSchedule = WFMHelper.SubmitIcmForm(month, year, agentNo, out msg, out success);
            //var latestIcmInfo = WFMHelper.GetLatestIcmInfo(icmSchedule.Last().Value.Last());
            //return JsonConvert.SerializeObject(new { success = success, msg = msg, icmSchedule = icmSchedule, latestIcmInfo = latestIcmInfo });
            return JsonConvert.SerializeObject(new { mowSchedule = mowSchedule, success = success, msg = msg });
        }

        public string SubmitLateShiftForm(string date, int agentNo)
        {
            string msg;
            var success = WFMHelper.SubmitLateShiftForm(date, agentNo, out msg);
            var lateShift = WFMHelper.GetLateShift();
            return JsonConvert.SerializeObject(new { lateShift = lateShift, success = success, msg = msg });
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
