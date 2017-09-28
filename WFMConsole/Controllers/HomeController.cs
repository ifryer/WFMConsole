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
            var user = HttpContext.KmIdentity();
            var result = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            string downBy, mow;
            WFMHelper.GetReportDates(out downBy, out mow);
            if (result.Credential != null)
            {
                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "ASP.NET MVC Sample"
                });
                ViewBag.Title = "WFM Dashboard A";
                ViewBag.downBy = downBy;
                ViewBag.mow = mow;
                return View();
            }
            else
            {
                log.Info($"User {user} is logging into google calendar");
                ViewBag.Title = "WFM Dashboard B";
                return new RedirectResult(result.RedirectUri);
            }
        }

        public string GetStaffList()
        {
            var msg = "";
            List<int> staffIdList;
            var list =  WFMHelper.GetStaffList(out msg, out staffIdList);
            return JsonConvert.SerializeObject(new { success = msg.ToLower().Contains("success"), msg = msg, nameList = list, idList = staffIdList });
        }

        public string GetTeamInfo()
        {
            string msg = "";
            bool success = false;
            var info = WFMHelper.GetTeamInfo();
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }


        public async Task<string> SubmitTimeOffForm(CancellationToken cancellationToken, int id, string name, string date, bool fullDay, string startTime, string endTime, string notes)
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} called SubmitTimeOffForm - Params: /r/n id: {id} \r\n name: {name} \r\n date: {date} \r\n fullDay: {fullDay} \r\n startTime: {startTime} \r\n endTime: {endTime} \r\n notes: {notes}");
            string msg = "";
            bool success = false;
            var googleAuth = await new AuthorizationCodeMvcAppOverride(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            success = WFMHelper.SubmitTimeOff(googleAuth, name, date, fullDay, startTime, endTime, notes, out msg);
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
        }

        #region Reports

        //Daily at 9:30 and in afternoon (once from 9-10, once at 12:30)
        //It is sent to: WFO, CSM Management. We pull the "Today's Scheduled Tasks and Events" information directly from the Gcal.
        public string CreateDownByReport()
        {
            var user = HttpContext.KmIdentity();
            log.Info($"User {user} created Down By Report");
            string msg = "";
            bool success = false;
            success = WFMHelper.CreateDownByReport();
            return JsonConvert.SerializeObject(new { success = success, msg = msg });
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
