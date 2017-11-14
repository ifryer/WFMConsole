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
using WFMConsole.Configuration;
using WFMConsole.ViewModels;
using WFMDashboard.Classes;

namespace WFMDashboard.Controllers
{
    public class HomeController : Controller
    {
        static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return RedirectToAction("Error", new { msg = "You are not authorized to use WFM Dashboard" });
            ViewBag.Title = "WFM Dashboard";
            //GCal login stuff no longer needed (probably)
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
                log.Info($"User {user.LdapUserId} is logging into google calendar");
                ViewBag.Title = "WFM Dashboard";
                return new RedirectResult(result.RedirectUri);
            }
        }

        public string GetPageInfo()
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return JsonConvert.SerializeObject(new { success = false, msg = "You are not authorized to access WFM Dashboard" });
            var msg = "";
            string downByDate, mowDate;
            WFMHelper.GetReportDates(out downByDate, out mowDate);
            var agentList = WFMHelper.GetStaffList(out msg);
            //var eventList = WFMHelper.GetEventList(DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"));
            var managerList = WFMHelper.GetManagerList(agentList); //TODO: Remove this?
            var mowList = WFMHelper.GetMowList(agentList);
            var lateShift = WFMHelper.GetLateShift();
            var icmSchedule = WFMHelper.GetIcmSchedule();
            var mowSchedule = WFMHelper.GetMowSchedule(DateTimeExtensions.StartOfWeek(DateTime.Now, DayOfWeek.Monday));
            var latestIcmInfo = WFMHelper.GetLatestIcmInfo(icmSchedule.Last().Value.Last());
            //var latestMowInfo = WFMHelper.GetLatestMowInfo();
            return JsonConvert.SerializeObject(new { success = msg.ToLower().Contains("success"), msg = msg, downByDate = downByDate, mowDate = mowDate, agentList = agentList, mowList = mowList, managerList = managerList, lateShift = lateShift, icmSchedule = icmSchedule, mowSchedule = mowSchedule, latestIcmInfo = latestIcmInfo });
            //return JsonConvert.SerializeObject(new { success = msg.ToLower().Contains("success"), msg = msg, downByDate = downByDate, mowDate = mowDate, eventList = eventList, agentList = agentList, mowList = mowList, managerList = managerList, lateShift = lateShift, icmSchedule = icmSchedule, mowSchedule = mowSchedule, latestIcmInfo = latestIcmInfo });
        }

        public string GetEvents(string start, string end)
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return JsonConvert.SerializeObject(new { success = false, msg = "You are not authorized to access WFM Dashboard" });
            var eventList = WFMHelper.GetEventList(start, end);
            return JsonConvert.SerializeObject(eventList);
        }

        public string GetTeamInfo(int agentNo)
        {
            string msg = "";
            var teamInfo = WFMHelper.GetTeamInfo(agentNo);
            return JsonConvert.SerializeObject(new { success = teamInfo != null, msg = msg, teamInfo = teamInfo });
        }

        public string GetRepeatingEventInfo(int eventId)
        {
            var results = WFMHelper.GetRepeatingEventInfo(eventId);
            return JsonConvert.SerializeObject(new { success = results != null, results = results });
        }

        public async Task<string> SubmitEventForm(CancellationToken cancellationToken, EventForm inputForm)
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return JsonConvert.SerializeObject(new { success = false, msg = "You are not authorized to access WFM Dashboard" });
            log.Info($"User {user.LdapUserId} called SubmitEventForm \r\n Params: InputForm: {inputForm.ToString()}");
            string msg = "";
            var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            List<ViewEvent> eventObject = WFMHelper.SubmitEventForm(googleAuth, inputForm, user.LdapUserId, out msg);
            return JsonConvert.SerializeObject(new { success = eventObject != null, msg = msg, eventObject = eventObject });
        }

        public async Task<string> DeleteEvent(CancellationToken cancellationToken, int id)
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return JsonConvert.SerializeObject(new { success = false, msg = "You are not authorized to access WFM Dashboard" });
            string msg = "";
            log.Info($"User {user.LdapUserId} called DeleteEvent /r/n id: {id}");
            var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            var success = WFMHelper.DeleteEvent(googleAuth, id, user.LdapUserId, out msg);
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        #region Reports

        public async Task<ActionResult> CreateDownByReport(CancellationToken cancellationToken)
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return RedirectToAction("Error", new { msg = "You are not authorized to use WFM Dashboard" });
            log.Info($"User {user.LdapUserId} created Down By Report");
            string msg = "";
            //bool success = false;
            var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            var report = WFMHelper.CreateDownByReport(googleAuth, out msg);
            if (report != null)
            {
                ViewBag.report = report;
                return View("~/Views/Mailer/DownByReport.cshtml");
            }
            else
            {
                log.Warn("Something went wrong creating Down By report");
                return RedirectToAction("Error", new { msg = msg});
            }
        }

        public ActionResult CreateMOWReport()
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return RedirectToAction("Error", new { msg = "You are not authorized to use WFM Dashboard" });
            log.Info($"User {user.LdapUserId} created MOW Report");
            string msg = "";
            var report = WFMHelper.CreateMOWReport(out msg);
            if (report != null)
            {
                ViewBag.report = report;
                return View("~/Views/Mailer/MowReport.cshtml");
            }
            else
            {
                log.Warn("Something went wrong creating MOW report");
                return RedirectToAction("Error", new { msg = msg });
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
            return JsonConvert.SerializeObject(new { mowSchedule = mowSchedule, success = mowSchedule != null });
        }

        public string SubmitIcmForm(int month, int year, int agentNo)
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return JsonConvert.SerializeObject(new { success = false, msg = "You are not authorized to access WFM Dashboard" });
            log.Info($"User {user.LdapUserId} called SubmitIcmForm - Params: /r/n month: {month} \r\n year: {year} \r\n agentNo: {agentNo}");
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
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return JsonConvert.SerializeObject(new { success = false, msg = "You are not authorized to access WFM Dashboard" });
            log.Info($"User {user.LdapUserId} called DeleteMowRow - Params: /r/n id: {rowId}");
            string msg;
            var success = WFMHelper.DeleteMowRow(rowId, out msg);
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        public string SubmitMowForm(MowFormInput Item, string MondayString)
        {
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return JsonConvert.SerializeObject(new { success = false, msg = "You are not authorized to access WFM Dashboard" });
            log.Info($"User {user.LdapUserId} called SubmitMowForm /r/n/ MondayString: {MondayString} \r\n InputItem: {Item.ToString()}");
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
            var user = HttpContext.KmIdentity();
            var WFMUser = getWFMUser(user.LdapUserId);
            if (WFMUser == null) return JsonConvert.SerializeObject(new { success = false, msg = "You are not authorized to access WFM Dashboard" });
            log.Info($"User {user.LdapUserId} called SubmitLateShiftForm /r/n/ date: {date} \r\n agentNo: {agentNo}");
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
            log.Info($"User {user.LdapUserId} logged out of google calendar");
            await EFDataStore.ClearAsyncStatic();
            return Redirect(ConfigurationManager.AppSettings["LogOutRedirect"]);
        }

        //Misc

        public ActionResult Error(string msg)
        {
            if (!string.IsNullOrWhiteSpace(msg))
                ViewBag.Msg = msg;
            return View();
        }

        private WFMConfig.WFMUser getWFMUser(string ldapUserId)
        {
            var wfmUser = WFM.Config.GetUser(ldapUserId);
            if (wfmUser != null) return wfmUser;
            log.Warn($"User {ldapUserId} attempted to use WFM Dashboard");
            return null;
        }
    }
}
