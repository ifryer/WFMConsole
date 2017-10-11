using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using WFMConsole.Models;
using WFMConsole.Classes;

namespace WFMDashboard.Classes
{
    public static class WFMHelper
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        public static List<string> GetStaffList(out string msg, out List<int> staffIdList)
        {
            var staffList = new List<string>();
            staffIdList = new List<int>();
            using (var db = new inContact_NGEntities())
            {
                var staffListDb = db.Agents.OrderBy(t => t.LastName);
                staffList = staffListDb.Select(t => t.FirstName + " " + t.LastName).ToList();
                staffIdList = staffListDb.Select(t => t.AgentNo).ToList();

            }
            msg = "Successfully retreived list of staff members";
            return staffList;
        }

        public static void GetReportDates(out string downBy, out string mow)
        {
            using (var db = new OnyxEntities())
            {
                var dbDown = db.BUS_WFMDashboard_ReportLog.Where(t => t.ReportType == "down by").FirstOrDefault().LastSent;
                downBy = dbDown.ToShortDateString() + " at " + dbDown.ToShortTimeString();
                var dbMow = db.BUS_WFMDashboard_ReportLog.Where(t => t.ReportType == "mow").FirstOrDefault().LastSent;
                mow = dbMow.ToShortDateString() + " at " + dbMow.ToShortTimeString();

            }

            return;
        }

        public static bool SubmitTimeOff(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, int id, string name, string date, bool fullDay, string startTime, string endTime, string notes, string ptoType, out string msg)
        {
            try
            {
                msg = "Successfully created event";
                var startDate = $"{date} {startTime}";
                var endDate = $"{date} {endTime}";

                var startDateTime = DateTime.Parse(startDate);
                var endDateTime = DateTime.Parse(endDate);

                if (endDateTime < startDateTime)
                {
                    msg = "Start time must be before end time";
                    return false;
                }

                Agent staffMember;
                using (var db = new inContact_NGEntities())
                {
                    staffMember = db.Agents.Where(t => t.AgentNo == id).FirstOrDefault();
                }

                CreateEvent(name, startDateTime, endDateTime, fullDay, notes, ptoType, staffMember);


                SubmitTimeOff_Nice();

                

                SubmitTimeOff_GoogleCalendar(googleAuth, name, startDateTime, endDateTime, fullDay, notes, ptoType, staffMember);
                bool success = true;
                return success;
            }
            catch(Exception ex)
            {
                //Log error
                msg = ex.ToString();
                return false;
            }
            
        }
        private static bool SubmitTimeOff_Nice()
        {
            bool success = false;
            return success;
        }

        
        private static bool CreateEvent(string name, DateTime startDateTime, DateTime endDateTime, bool fullDay, string notes, string ptoType, Agent agent)
        {
            try
            {
                BUS_WFMDashboard_Event busEvent = new BUS_WFMDashboard_Event() {
                    AgentNo = agent.AgentNo,
                    TeamName = agent.TeamName,
                    TeamId = agent.TeamNo,
                    FullDay = fullDay,
                    EventType = ptoType,
                    StartTime = startDateTime, 
                    EndTime = endDateTime
                };
                using (var db = new OnyxEntities())
                {
                    db.BUS_WFMDashboard_Event.Add(busEvent);
                    db.SaveChanges();
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        private static bool SubmitTimeOff_GoogleCalendar(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, string name, DateTime startDateTime, DateTime endDateTime, bool fullDay, string notes, string ptoType, Agent agent)
        {
            try
            {
                //var calendarId = "kmbs.konicaminolta.us_m48bnnmmf4s08pbhm6cmkdiodo@group.calendar.google.com";
                String calendarId = "primary";
                var team = agent.TeamName;
                team = ProcessTeamName(team);

                var lastName = agent.LastName;

                string summary = $"*{team} - {lastName} - PTO ({ptoType}) [WFM DASHBOARD TEST EVENT]";
                string colorId = "9"; //Color for unplanned PTO
                if(ptoType == "Planned")
                {
                    colorId = "10"; //Color for planned PTO
                }

                EventDateTime start = new EventDateTime();
                EventDateTime end = new EventDateTime();

                start = new EventDateTime()
                {
                    TimeZone = "America/New_York"

                };
                end = new EventDateTime()
                {
                    TimeZone = "America/New_York"
                };
                if (fullDay)
                {
                    start.Date = startDateTime.ToString("yyyy-MM-dd");
                    end.Date = endDateTime.ToString("yyyy-MM-dd");
                }
                else
                {
                    start.DateTime = startDateTime;
                    end.DateTime = endDateTime;
                }

                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleAuth.Credential,
                    ApplicationName = ApplicationName,
                });

                Event newEvent = new Event()
                {
                    Summary = summary,
                    Description = notes,
                    Start = start,
                    ColorId = colorId,
                    End = end,
                    Attendees = new EventAttendee[] {
                        new EventAttendee() { Email = "rtyszka@kmbs.konicaminolta.us" },
                    },
                };
                
                EventsResource.InsertRequest request = service.Events.Insert(newEvent, calendarId);
                Event createdEvent = request.Execute();
                bool success = false;
                return success;
            }
            catch (Exception ex)
            {
                log.Error("Error in SubmitTimeOff_GoogleCalendar", ex);
                throw ex;
            }
        }

        public static string ProcessTeamName(string team)
        {
            if (team.ToLower().Contains("management"))
            {
                team = "MGMT";
            }
            if (team.ToLower().Contains("irc"))
            {
                team = "IRC";
            }

            if (team.ToLower().Contains("ad"))
            {
                team = "AD";
            }
            if (team.ToLower().Contains("printer"))
            {
                team = "Printer";
            }
            if (team.ToLower().Contains("ra"))
            {
                team = "RA";
            }
            return team;
        }

        public static DownByReport CreateDownByReport(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth)
        {
            try
            {
                var report = GenerateDownByReport(googleAuth);


                return report;


                //var result = TrapModel.UpdateTrap(user.LdapUserId, updateModel.trapId, updateModel.requestId, updateModel.incidentId, updateModel.model, updateModel.description, updateModel.placement, updateModel.recommendation, updateModel.reason, out msg);

                //string owner = TrapModel.GetTrapOwnerEmail(updateModel.trapId);

                //string userEmail = TrapModel.GetUserEmail(user.LdapUserId);
                //var mailer = new Mailer();
                //var recipients = ConfigurationManager.AppSettings["UpdateDistributionList"];
                //if (!owner.Contains(" ") && owner.Contains("@"))
                //    recipients += "," + owner;
                //if (!userEmail.Contains(" ") && userEmail.Contains("@"))
                //    recipients += "," + userEmail;
                //if (!string.IsNullOrWhiteSpace(recipients))
                //{
                //    var from = ConfigurationManager.AppSettings["UpdateEmailFrom"];
                //    var mail = mailer.UpdateTrapEmail(result, recipients, from, "Trap Update");
                //    mail.Send();
                //}
                //return JsonConvert.SerializeObject(new
                //{
                //    success = (msg.ToLower().Contains("success")),
                //    msg = msg
                //});

            }
            catch (Exception ex)
            {
                log.Error("Error in CreateDownByReport", ex);
                return null;
            }
        }

        public static DownByReport GenerateDownByReport(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth)
        {
            //DownByReport report = new DownByReport();
            var calendarId = "kmbs.konicaminolta.us_m48bnnmmf4s08pbhm6cmkdiodo@group.calendar.google.com";

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleAuth.Credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List(calendarId);

            request.TimeMin = DateTime.Today.Date; //Start of day
            request.TimeMax = DateTime.Today.Date.AddHours(23).AddMinutes(59); //End of day
            request.ShowDeleted = false;
            request.SingleEvents = true;
            //request.MaxResults = 20;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();

            DownByReport report = FillDownByReport(events);

            return report;
        }

        public static DownByReport FillDownByReport(Events events)
        {
            var report = new DownByReport();
            var ptoDateEnd = DateTime.Now.AddDays(1).Date.ToString("yyyy-MM-dd"); //To get proper full-day events that are for today, they apparently end tomorrow. Thanks google!
            var paidPto = events.Items.Where(t => (t.ColorId == "10" || t.ColorId == "9") && t.End.DateTime == null && t.End.Date == ptoDateEnd).ToList();
            var fullDayTraining = events.Items.Where(t => t.ColorId == "7" && t.End.DateTime == null).ToList();

            //var partialTraining = events.Items.Where(t => t.ColorId == "7" && t.End.DateTime != null).ToList();

            var rangeEvents = events.Items.Where(t => t.Start.DateTime != null).OrderBy(t => t.Start.DateTime.Value.TimeOfDay).ToList();
            foreach (var item in rangeEvents)
            {
                var downByEvent = new DownByEvent(item.ColorId);
                var startString = item.Start.DateTime.Value.ToShortTimeString();
                var endString = item.End.DateTime.Value.ToShortTimeString();
                downByEvent.DateRange = $"{startString} - {endString}";
                downByEvent.StartTime = item.Start.DateTime.Value;
                downByEvent.Title = item.Summary;
                report.Events.Add(downByEvent);
            }
            var darkRedEvents = events.Items.Where(t => t.ColorId == "11").ToList();
            var lightRedEvents = events.Items.Where(t => t.ColorId == "4").ToList();

            report = FillDownByReportSection(report, fullDayTraining, "Training");
            report = FillDownByReportSection(report, paidPto, "PTO");

            return report;
        }

        public static DownByReport FillDownByReportSection(DownByReport report, List<Event> events, string keyName)
        {
            foreach (var item in events)
            {

                var sections = item.Summary.Split('-');
                var teamName = sections[0];
                var staffName = sections[1];
                report.TotalDown++;
                report.Sections[keyName].TotalDown++;
                if (teamName.Contains("RA"))
                {
                    report.Sections[keyName].RA = report.Sections[keyName].RA.TrimStart('0');
                    report.Sections[keyName].RA += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                    report.Sections[keyName].RA = report.Sections[keyName].RA.TrimStart(',').TrimStart(' ');
                }
                if (teamName.Contains("AD"))
                {
                    report.Sections[keyName].AD = report.Sections[keyName].AD.TrimStart('0');
                    report.Sections[keyName].AD += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                    report.Sections[keyName].AD = report.Sections[keyName].AD.TrimStart(',').TrimStart(' ');
                }
                if (teamName.Contains("IRC"))
                {
                    report.Sections[keyName].IRC = report.Sections[keyName].IRC.TrimStart('0');
                    report.Sections[keyName].IRC += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                    report.Sections[keyName].IRC = report.Sections[keyName].IRC.TrimStart(',').TrimStart(' ');
                }
                if (teamName.ToLower().Contains("printer"))
                {
                    report.Sections[keyName].PrinterOps = report.Sections[keyName].PrinterOps.TrimStart('0');
                    report.Sections[keyName].PrinterOps += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                    report.Sections[keyName].PrinterOps = report.Sections[keyName].PrinterOps.TrimStart(',').TrimStart(' ');
                }
                if (teamName.Contains("MGR"))
                {
                    report.Sections[keyName].MGR = report.Sections[keyName].MGR.TrimStart('0');
                    report.Sections[keyName].MGR += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                    report.Sections[keyName].MGR = report.Sections[keyName].MGR.TrimStart(',').TrimStart(' ');
                }
                if (teamName.Contains("POA"))
                {
                    report.Sections[keyName].POA = report.Sections[keyName].POA.TrimStart('0');
                    report.Sections[keyName].POA += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                    report.Sections[keyName].POA = report.Sections[keyName].POA.TrimStart(',').TrimStart(' ');
                }
            }
            return report;
        }

        public static bool CreateMOWReport()
        {
            try
            {
                bool success = false;
                return success;
            }
            catch (Exception ex)
            {
                log.Error("Error in CreateMOWReport", ex);
                return false;
            }
        }

        public static bool GetTeamInfo(int agentNo)
        {
            try
            {
                using (var db = new inContact_NGEntities())
                {
                    var agent = db.Agents.Where(t => t.AgentNo == agentNo).FirstOrDefault();
                    var team = agent.TeamNo;
                    var teamMembers = db.Agents.Where(t => t.TeamNo == team).ToList();
                }
                bool success = false;
                return success;
            }
            catch (Exception ex)
            {
                log.Error("Error in GetTeamInfo", ex);
                return false;
            }
            
        }

    }
}