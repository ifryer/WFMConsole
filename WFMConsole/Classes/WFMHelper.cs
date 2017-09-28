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

        public static bool SubmitTimeOff(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, string name, string date, bool fullDay, string startTime, string endTime, string notes, out string msg)
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

                SubmitTimeOff_Nice();

                

                SubmitTimeOff_GoogleCalendar(googleAuth, name, startDateTime, endDateTime, fullDay, notes);
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

        

        private static bool SubmitTimeOff_GoogleCalendar(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, string name, DateTime startDateTime, DateTime endDateTime, bool fullDay, string notes)
        {
            try
            {
                EventDateTime start = new EventDateTime();
                EventDateTime end = new EventDateTime();
                
                if (fullDay)
                {
                    start = new EventDateTime()
                    {
                        Date = startDateTime.ToString("yyyy-MM-dd"),
                        TimeZone = "America/New_York",

                    };
                    end = new EventDateTime()
                    {
                        Date = endDateTime.ToString("yyyy-MM-dd"),
                        TimeZone = "America/New_York",
                    };
                }
                else
                {
                    
                    start = new EventDateTime()
                    {
                        DateTime = startDateTime,
                        TimeZone = "America/New_York",

                    };
                    end = new EventDateTime()
                    {
                        DateTime = endDateTime,
                        TimeZone = "America/New_York",
                    };
                 }


                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleAuth.Credential,
                    ApplicationName = ApplicationName,
                });

               
                Event newEvent = new Event()
                {
                    Summary = $"{name} Out",
                    Description = notes,
                    Start = start,
                    End = end,
                    Attendees = new EventAttendee[] {
                        new EventAttendee() { Email = "rtyszka@kmbs.konicaminolta.us" },
                    },
                };

                String calendarId = "primary";
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

        public static bool CreateDownByReport()
        {
            try
            {
                bool success = false;
                return success;
            }
            catch (Exception ex)
            {
                log.Error("Error in CreateDownByReport", ex);
                return false;
            }
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

        public static bool GetTeamInfo()
        {
            try
            {
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