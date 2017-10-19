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
using System.Configuration;
using WFMConsole.ViewModels;

namespace WFMDashboard.Classes
{
    public static class WFMHelper
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        public static List<ViewAgent> GetStaffList(out string msg)
        {
            var staffList = new List<ViewAgent>();
            using (var db = new inContact_NGEntities())
            {
                var staffListDb = db.Agents.OrderBy(t => t.LastName);
                foreach (var item in staffListDb)
                {
                    staffList.Add(new ViewAgent(item));
                }
                //staffList = staffListDb.Select(t => new WFMConsole.ViewModels.ViewAgent(t)).ToList();

            }
            msg = "Successfully retreived list of staff members";
            return staffList;
        }

        public static List<ViewEvent> GetEventList()
        {
            using (var db = new OnyxEntities())
            {
                var eventList = new List<ViewEvent>();
                //var dbList = db.BUS_WFMDashboard_Event.Where(t => t.StartTime <= DateTime.Today.Date && t.EndTime > DateTime.Today.Date).ToList();
                var events = db.BUS_WFMDashboard_Event.Where(t => t.EndTime >= DateTime.Now || (t.FullDay && t.EndTime >= DateTime.Today.Date)).ToList();
                var pastEvents = db.BUS_WFMDashboard_Event.Where(t => (t.EndTime < DateTime.Now && !t.FullDay) || (t.EndTime < DateTime.Today.Date && t.FullDay)).ToList();


                foreach (var item in events)
                {
                    var newEvent = new ViewEvent(item, false);
                    eventList.Add(newEvent);
                }
                foreach (var item in pastEvents)
                {
                    var newEvent = new ViewEvent(item, true);
                    eventList.Add(newEvent);
                }
                return eventList;
            }
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

        //Color IDs : 4 = Scheduled Event = Pink ----- 7 = Teal = Training ----- 9 = Blue = Unplanned PTO ----- 10 = Green = Planned PTO ----- 11 = Red = Scheduled Event

        public static bool SubmitEventForm(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, int id, string title, string color, string startDateInput, string endDateInput, bool fullDay, string startTime, string endTime, string notes, string eventType, out string msg)
        {
            try
            {
                msg = "Successfully created event";
                var startDate = $"{startDateInput} {startTime}";
                var endDate = $"{endDateInput} {endTime}";

                var startDateTime = DateTime.Parse(startDate);
                var endDateTime = DateTime.Parse(endDate);

                if (endDateTime < startDateTime)
                {
                    msg = "Start time must be before end time";
                    return false;
                }
                if(notes == null)
                {
                    notes = title;
                }
                Agent staffMember;
                using (var db = new inContact_NGEntities())
                {
                    staffMember = db.Agents.Where(t => t.AgentNo == id).FirstOrDefault();
                }
                string lastName = staffMember.LastName;
                string firstName = staffMember.FirstName;


                var googleEventId = SubmitTimeOff_GoogleCalendar(googleAuth, title, lastName, color, startDateTime, endDateTime, fullDay, notes, eventType, staffMember);

                CreateEvent(title, googleEventId, lastName, firstName, color, startDateTime, endDateTime, fullDay, notes, eventType, staffMember);


                SubmitTimeOff_Nice();


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

        public static bool DeleteEvent(int id, out string msg)
        {
            try
            {
                using (var db = new OnyxEntities())
                {
                    var eventItem = db.BUS_WFMDashboard_Event.Where(t => t.Id == id).FirstOrDefault();
                    var googleCalId = eventItem.CalendarEventId;
                    db.BUS_WFMDashboard_Event.Remove(eventItem);
                    db.SaveChanges();


                    //TODO: Remove event from google calendar here too...
                }
                msg = "Successfully deleted event.";
                return true;
            }
            catch(Exception ex)
            {
                log.Error($"Error deleting event {id}", ex);
                msg = ex.ToString();
                return false;
            }
        }

        private static bool SubmitTimeOff_Nice()
        {
            bool success = false;
            return success;
        }

        
        private static bool CreateEvent(string title, string eventId, string lastName, string firstName, string color, DateTime startDateTime, DateTime endDateTime, bool fullDay, string notes, string eventType, Agent agent)
        {
            try
            {
                BUS_WFMDashboard_Event busEvent = new BUS_WFMDashboard_Event() {
                    AgentNo = agent.AgentNo,
                    TeamName = agent.TeamName,
                    TeamId = agent.TeamNo,
                    FullDay = fullDay,
                    EventType = eventType,
                    StartTime = startDateTime, 
                    EndTime = endDateTime,
                    Description = title,
                    Notes = notes,
                    LastName = lastName, 
                    FirstName = firstName,
                    CalendarEventId = eventId,
                    Color = color
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
        private static string SubmitTimeOff_GoogleCalendar(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, string title, string name, string color, DateTime startDateTime, DateTime endDateTime, bool fullDay, string notes, string eventType, Agent agent)
        {
            try
            {
                String calendarId = ConfigurationManager.AppSettings["CalendarId"];
                var team = agent.TeamName;
                team = ProcessTeamName(team);

                var lastName = agent.LastName;

                string summary = title;
                string colorId = color; //Color for unplanned PTO

                //TODO: fix the color based on the event type
                //if(ptoType == "Planned")
                //{
                //    colorId = "10"; //Color for planned PTO
                //}

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
                //bool success = false;
                return createdEvent.Id;
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
            var calendarId = ConfigurationManager.AppSettings["CalendarId"];

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleAuth.Credential,
                ApplicationName = ApplicationName,
                //ApiKey = ConfigurationManager.AppSettings["ApiKey"],
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
            //var ptoDateEnd = DateTime.Now.AddDays(1).Date.ToString("yyyy-MM-dd"); 
            var ptoDateEnd = DateTime.Now.AddDays(1).Date;
            var paidPto = events.Items.Where(t => (t.ColorId == "10" || t.ColorId == "9") && t.End.DateTime == null && DateTime.Parse(t.End.Date) >= ptoDateEnd).ToList();
            var fullDayTraining = events.Items.Where(t => t.ColorId == "7" && t.End.DateTime == null && DateTime.Parse(t.End.Date) >= ptoDateEnd).ToList();
            fullDayTraining = fullDayTraining.Where(t => !t.Summary.ToLower().Contains("cancelled")).ToList();
            var fullDayOther = events.Items.Where(t => t.ColorId == "11" && t.End.DateTime == null && DateTime.Parse(t.End.Date) >= ptoDateEnd).ToList();

            //var partialTraining = events.Items.Where(t => t.ColorId == "7" && t.End.DateTime != null).ToList();

            CreateDatabaseEvents(events);

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

            report = FillDownByReportSectionFullDay(report, fullDayTraining, "Training");
            report = FillDownByReportSectionFullDay(report, fullDayOther, "Other");
            report = FillDownByReportSectionFullDay(report, paidPto, "PTO");

            return report;
        }

        public static bool CreateDatabaseEvents(Events events)
        {
            using (var db = new OnyxEntities())
            using (var dbS = new inContact_NGEntities())
            {
                foreach (var item in events.Items)
                {
                    string eventType = "Other";
                    if(item.ColorId == "10" || item.ColorId == "9" )
                    {
                        eventType = "PTO";
                    }
                    if (item.ColorId == "7")
                    {
                        eventType = "Training";
                    }

                    var sections = item.Summary.Split('-');
                    var teamName = sections[0];
                    var staffName = sections[1];
                    bool fullDay = true;

                    var staffMember = dbS.Agents.Where(t => staffName.Contains(t.LastName.ToLower())).FirstOrDefault();
                    if (staffMember != null)
                    {
                        DateTime startTime = DateTime.Now;
                        DateTime endTime = DateTime.Now;
                        if(item.End.Date != null)
                        {
                            fullDay = true;
                            startTime = DateTime.Parse(item.Start.Date);
                            endTime = DateTime.Parse(item.End.Date);
                        }
                        else
                        {
                            fullDay = false;
                            startTime = item.Start.DateTime.Value;
                            endTime = item.End.DateTime.Value;
                        }
                        var prevEvent = db.BUS_WFMDashboard_Event.Where(t => t.CalendarEventId == item.Id).FirstOrDefault();
                        if (prevEvent == null)
                        {
                            var newEvent = new BUS_WFMDashboard_Event()
                            {
                                FirstName = staffMember.FirstName,
                                Notes = item.Description,
                                CalendarEventId = item.Id,
                                AgentNo = staffMember.AgentNo,
                                StartTime = startTime,
                                EndTime = endTime,
                                EventType = eventType,
                                FullDay = fullDay,
                                TeamId = staffMember.TeamNo,
                                TeamName = staffMember.TeamName,
                                LastName = staffName,
                                Description = item.Summary,
                                Color = item.ColorId
                            };
                            db.BUS_WFMDashboard_Event.Add(newEvent);
                            //db.SaveChanges();
                        }
                        else
                        {
                            //TODO: Update the event here if it needs to be updated
                        }
                    }
                }
                db.SaveChanges();
            }
            return true;
        }

        public static DownByReport FillDownByReportSectionFullDay(DownByReport report, List<Event> events, string eventType)
        {
            List<BUS_WFMDashboard_Event> eventList = new List<BUS_WFMDashboard_Event>();
            using (var db = new OnyxEntities())
            using(var dbS = new inContact_NGEntities())
            {
                foreach (var item in events)
                {
                    var sections = item.Summary.Split('-');
                    var teamName = sections[0];
                    var staffName = sections[1];

                    //var staffMember = dbS.Agents.Where(t => staffName.Contains(t.LastName.ToLower())).FirstOrDefault();
                    //if(staffMember != null)
                    //{
                    //    var startTime = DateTime.Parse(item.Start.Date);
                    //    var endTime = DateTime.Parse(item.End.Date);
                    //    var prevEvent = db.BUS_WFMDashboard_Event.Where(t => t.AgentNo == staffMember.AgentNo && t.StartTime == startTime && t.EndTime == endTime).FirstOrDefault();
                    //    if(prevEvent == null)
                    //    {
                    //        var newEvent = new BUS_WFMDashboard_Event()
                    //        {
                    //            AgentNo = staffMember.AgentNo,
                    //            StartTime = startTime,
                    //            EndTime = endTime,
                    //            EventType = eventType,
                    //            FullDay = true,
                    //            TeamId = staffMember.TeamNo,
                    //            TeamName = staffMember.TeamName,
                    //            LastName = staffName,
                    //            Description = item.Summary
                    //        };
                    //        db.BUS_WFMDashboard_Event.Add(newEvent);
                    //    }
                    //}

                    report.TotalDown++;
                    report.Sections[eventType].TotalDown++;
                    if (teamName.Contains("RA"))
                    {
                        report.Sections[eventType].RA = report.Sections[eventType].RA.TrimStart('0');
                        report.Sections[eventType].RA += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].RA = report.Sections[eventType].RA.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.Contains("AD"))
                    {
                        report.Sections[eventType].AD = report.Sections[eventType].AD.TrimStart('0');
                        report.Sections[eventType].AD += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].AD = report.Sections[eventType].AD.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.Contains("IRC"))
                    {
                        report.Sections[eventType].IRC = report.Sections[eventType].IRC.TrimStart('0');
                        report.Sections[eventType].IRC += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].IRC = report.Sections[eventType].IRC.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.ToLower().Contains("printer"))
                    {
                        report.Sections[eventType].PrinterOps = report.Sections[eventType].PrinterOps.TrimStart('0');
                        report.Sections[eventType].PrinterOps += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].PrinterOps = report.Sections[eventType].PrinterOps.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.Contains("MGR"))
                    {
                        report.Sections[eventType].MGR = report.Sections[eventType].MGR.TrimStart('0');
                        report.Sections[eventType].MGR += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].MGR = report.Sections[eventType].MGR.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.Contains("POA"))
                    {
                        report.Sections[eventType].POA = report.Sections[eventType].POA.TrimStart('0');
                        report.Sections[eventType].POA += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].POA = report.Sections[eventType].POA.TrimStart(',').TrimStart(' ');
                    }
                }
                //db.SaveChanges();
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

        public static TeamInfo GetTeamInfo(int agentNo)
        {
            try
            {
                TeamInfo teamInfo = new TeamInfo();
                int team = 0;
                using (var db = new inContact_NGEntities())
                {
                    var agent = db.Agents.Where(t => t.AgentNo == agentNo).FirstOrDefault();
                    teamInfo.TeamName = agent.TeamName;
                    team = agent.TeamNo;
                    //var teamMembers = db.Agents.Where(t => t.TeamNo == team).ToList();
                }
                using (var db = new OnyxEntities())
                {
                    var totalDown = 0;

                    var events = db.BUS_WFMDashboard_Event.Where(t => t.TeamId == team && t.StartTime <= DateTime.Today.Date && t.EndTime > DateTime.Today.Date).ToList();
                    var ptoCount = events.Where(t => t.EventType == "PTO").ToList();
                    var trainingCount = events.Where(t => t.EventType == "Training").ToList();

                    totalDown += ptoCount.Count;
                    teamInfo.PTO = ptoCount.Count.ToString();

                    totalDown += trainingCount.Count;
                    teamInfo.Training = trainingCount.Count.ToString();

                    teamInfo.TotalDown = totalDown.ToString();
                    return teamInfo;
                }
                
            }
            catch (Exception ex)
            {
                log.Error("Error in GetTeamInfo", ex);
                return null;
            }
        }
    }
}