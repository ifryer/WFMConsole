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
using System.Globalization;
using FluentDateTime;

namespace WFMDashboard.Classes
{
    public static class WFMHelper
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";

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
                    var newEvent = new ViewEvent(item, false, false);
                    eventList.Add(newEvent);
                }
                foreach (var item in pastEvents)
                {
                    var newEvent = new ViewEvent(item, true, false);
                    eventList.Add(newEvent);
                }

                var repeatingEvents = db.BUS_WFMDashboard_Repeating_Event.Where(t => t.CalculatedEndDate >= DateTime.Today.Date || t.CalculatedEndDate == null).ToList();
                foreach (var item in repeatingEvents)
                {
                    var addRepeat = eventList.Where(t => t.Id == item.EventId).FirstOrDefault();
                    if (addRepeat != null)
                        addRepeat.repeating = true;
                    var eventItem = db.BUS_WFMDashboard_Event.Where(t => t.Id == item.EventId).FirstOrDefault();
                    var dateList = GetRepeatingEventDateList(item, DateTime.Now.Date, DateTime.Now.Date.AddMonths(1));
                    foreach (var dateItem in dateList)
                    {
                        var newEvent = new ViewEvent(eventItem, dateItem);
                        eventList.Add(newEvent);
                    }
                }

                
                return eventList;
            }
        }

        public static List<ViewEvent> GetEventList(string start, string end)
        {
            using (var db = new OnyxEntities())
            {
                var startDateInput = DateTime.Parse(start);
                var endDateInput = DateTime.Parse(end);
                var eventList = new List<ViewEvent>();
                //var dbList = db.BUS_WFMDashboard_Event.Where(t => t.StartTime <= DateTime.Today.Date && t.EndTime > DateTime.Today.Date).ToList();
                var events = db.BUS_WFMDashboard_Event.Where(t => (t.StartTime >= startDateInput && t.StartTime <= endDateInput) || (t.FullDay && t.EndTime >= DateTime.Today.Date)).ToList();
                var pastEvents = events.Where(t => (t.EndTime < DateTime.Now && !t.FullDay) || (t.EndTime < DateTime.Today.Date && t.FullDay)).ToList();
                events = events.Except(pastEvents).ToList();
                //var pastEvents = db.BUS_WFMDashboard_Event.Where(t => ((t.StartTime >= startDateInput && t.StartTime <= endDateInput) && (t.EndTime < DateTime.Now && !t.FullDay)) || (t.EndTime < DateTime.Today.Date && t.FullDay && (t.StartTime >= startDateInput && t.StartTime <= endDateInput))).ToList();
                foreach (var item in events)
                {
                    var newEvent = new ViewEvent(item, false, false);
                    eventList.Add(newEvent);
                }
                foreach (var item in pastEvents)
                {
                    var newEvent = new ViewEvent(item, true, false);
                    eventList.Add(newEvent);
                }

                var repeatingEvents = db.BUS_WFMDashboard_Repeating_Event.Where(t => t.CalculatedEndDate >= startDateInput || t.CalculatedEndDate == null).ToList();
                foreach (var item in repeatingEvents)
                {
                    var addRepeat = eventList.Where(t => t.Id == item.EventId).FirstOrDefault();
                    if (addRepeat != null)
                        addRepeat.repeating = true;
                    var eventItem = db.BUS_WFMDashboard_Event.Where(t => t.Id == item.EventId).FirstOrDefault();
                    var dateList = GetRepeatingEventDateList(item, startDateInput, endDateInput);
                    foreach (var dateItem in dateList)
                    {
                        var newEvent = new ViewEvent(eventItem, dateItem);
                        eventList.Add(newEvent);
                    }
                }


                return eventList;
            }
        }

        public static List<ViewAgent> GetStaffList(out string msg)
        {
            var staffList = new List<ViewAgent>();
            using (var db = new inContact_NGEntities())
            {
                var staffListDb = db.Agents.Where(t => t.Status == "Active").OrderBy(t => t.LastName);
                foreach (var item in staffListDb)
                {
                    staffList.Add(new ViewAgent(item));
                }
                //staffList = staffListDb.Select(t => new WFMConsole.ViewModels.ViewAgent(t)).ToList();

            }
            msg = "Successfully retreived list of staff members";
            return staffList;
        }

        

        public static List<ViewAgent> GetMowList(List<ViewAgent> agentList)
        {
            return agentList.Where(t => t.TeamName.Contains("CCA")).ToList();
        }

        public static List<ViewAgent> GetManagerList(List<ViewAgent> agentList)
        {
            return agentList.Where(t => t.TeamName.Contains("Management")).ToList();
        }

        public static List<ViewLateShiftSchedule> GetLateShift()
        {
            var weekStart = WFMConsole.Classes.DateTimeExtensions.StartOfWeek(DateTime.Now, DayOfWeek.Monday);
            var weekEnd = weekStart.AddDays(5);
            using (var db = new OnyxEntities())
            {
                var weekLateShift = db.BUS_WFMDashboard_LateShiftManager.Where(t => t.Date >= weekStart && t.Date <= weekEnd).OrderBy(t => t.Date).Select(t => new ViewLateShiftSchedule() {AgentNo = t.AgentNo, Date = t.Date, Id = t.Id, ManagerName = t.FirstName + " " + t.LastName }).ToList();
                return weekLateShift;
            }
        }

        public static ViewRepeatingItem GetRepeatingEventInfo(int eventId)
        {
            using (var db = new OnyxEntities())
            {
                var eventItem = db.BUS_WFMDashboard_Repeating_Event.Where(t => t.EventId == eventId).FirstOrDefault();
                return new ViewRepeatingItem(eventItem);
            }
        }

        public static bool SubmitLateShiftForm(string date, int agentNo, out string msg)
        {
            try
            {
                msg = "Successfully saved";
                Agent agent;
                using (var dbS = new inContact_NGEntities())
                {
                    agent = dbS.Agents.Where(t => t.AgentNo == agentNo).FirstOrDefault();
                }

                using (var db = new OnyxEntities())
                {
                    var dateObj = DateTime.Parse(date).Date;
                    var prevRow = db.BUS_WFMDashboard_LateShiftManager.Where(t => t.Date == dateObj).FirstOrDefault();
                    if(prevRow == null)
                    {
                        var newRow = new BUS_WFMDashboard_LateShiftManager();
                        newRow.LastName = agent.LastName;
                        newRow.FirstName = agent.FirstName;
                        newRow.AgentNo = agentNo;
                        newRow.Date = dateObj;
                        db.BUS_WFMDashboard_LateShiftManager.Add(newRow);
                    }
                    else
                    {
                        prevRow.LastName = agent.LastName;
                        prevRow.FirstName = agent.FirstName;
                        prevRow.AgentNo = agentNo;
                    }
                    db.SaveChanges();
                }
                return true;

            }
            catch(Exception ex)
            {
                log.Error("Error in SubmitLastShiftForm", ex);
                msg = ex.ToString();
                return false;
            }
        }

        public static Dictionary<int, List<ViewIcmSchedule>> GetIcmSchedule()
        {
            using (var db = new OnyxEntities())
            {
                var monthNumber = DateTime.Now.Month;
                var yearNumber = DateTime.Now.Year;
                //var icmScheduleList = db.BUS_WFMDashboard_ICM.Where(t => t.Month >= monthNumber && t.Year >= yearNumber).ToList();
                var icmScheduleList = db.BUS_WFMDashboard_ICM;

                var results = icmScheduleList.Select(t => new ViewIcmSchedule()
                {
                    AgentNo = t.AgentNo,
                    Month = t.Month,
                    Year = t.Year,
                    ManagerName = t.Manager
                }).OrderBy(t => t.Month).GroupBy(x => x.Year).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

                return results;
            }
        }
        public static Dictionary<DateTime, List<ViewMowSchedule>> GetMowSchedule(DateTime monday)
        {
            using (var db = new OnyxEntities())
            {
                var weekStart = monday;
                var weekEnd = weekStart.AddDays(5);
                var mowScheduleList = db.BUS_WFMDashboard_WFO_Schedule.Where(t => t.StartTime >= weekStart && t.StartTime <= weekEnd).ToList();

                var results = mowScheduleList
                    .OrderBy(t => t.StartTime)
                    .Select(t => new ViewMowSchedule() {
                        Id = t.Id,
                        AgentNo = t.AgentNo,
                        Task = t.Task,
                        Date = t.StartTime.Date,
                        StartTime = t.StartTime.ToShortTimeString(),
                        EndTime = t.EndTime.ToShortTimeString(),
                        FirstName = t.FirstName,
                        LastName = t.LastName
                    })
                    .GroupBy(x => x.Date)
                    .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

                return results;
                //return mowScheduleList.Select(t => new ViewMowSchedule(t)).ToList();
            }
        }

        public static ViewIcmLatest GetLatestIcmInfo(ViewIcmSchedule lastSchedule)
        {
            ViewIcmLatest latestIcm = new ViewIcmLatest(lastSchedule);
            
            return latestIcm;
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
        public static List<ViewEvent> SubmitEventForm(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, EventForm inputForm, string user, out string msg)
        {
            try
            {
                List<ViewEvent> eventItemList = new List<ViewEvent>();
                ViewEvent eventItem = new ViewEvent();

                var startDate = $"{inputForm.startDate} {inputForm.startTime}";
                var endDate = $"{inputForm.endDate} {inputForm.endTime}";

                var startDateTime = DateTime.Parse(startDate);
                var endDateTime = DateTime.Parse(endDate);

                if (endDateTime < startDateTime)
                {
                    if(inputForm.fullDay)
                    {
                        endDateTime = startDateTime.AddDays(1);
                    }
                    else
                    {
                        msg = "Start time must be before end time";
                        return null;
                    }
                }
                if(inputForm.notes == null)
                {
                    inputForm.notes = inputForm.title;
                }

                if (inputForm.eventId == 0)
                {
                    msg = "Successfully created event";
                    Agent staffMember;
                    using (var db = new inContact_NGEntities())
                    {
                        staffMember = db.Agents.Where(t => t.AgentNo == inputForm.agentId).FirstOrDefault();
                    }
                    string lastName = staffMember.LastName;
                    string firstName = staffMember.FirstName;

                    var googleEventId = SubmitTimeOff_GoogleCalendar(googleAuth, inputForm, startDateTime, endDateTime, staffMember);

                    eventItem = CreateEvent(googleEventId, inputForm, startDateTime, endDateTime, staffMember, user);
                    eventItemList.Add(eventItem);
                    //SubmitTimeOff_Nice();

                    //return new List<ViewEvent>eventItem;
                }
                else
                {
                    msg = "Successfully updated event";
                    eventItem = UpdateEvent(inputForm, startDateTime, endDateTime, user);
                    eventItemList.Add(eventItem);
                    //TODO: Make sure recurring events edited work properly.
                    //If its an issue, just delete the event, make a new one with the recurring thing, and save that ID to the object again.

                    var message = UpdateEvent_GoogleCalendar(googleAuth, eventItem, inputForm);
                    if(!(message.ToLower().Contains("success")))
                    {
                        msg = message;
                        return null;
                    }
                }
                if(inputForm.repeatingEvent)
                {
                    using (var db = new OnyxEntities())
                    {
                        //TODO: we should have these variables somewhere, let's use them instead of re-grabbing them from the database
                        var item = db.BUS_WFMDashboard_Repeating_Event.Where(t => t.EventId == eventItem.Id).FirstOrDefault();
                        var editingEvent = db.BUS_WFMDashboard_Event.Where(t => t.Id == eventItem.Id).FirstOrDefault();
                        var dateList = GetRepeatingEventDateList(item, DateTime.Now.Date, DateTime.Now.Date.AddMonths(1));
                        foreach (var dateItem in dateList)
                        {
                            var newEvent = new ViewEvent(editingEvent, dateItem);
                            eventItemList.Add(newEvent);
                        }
                    }
                }
                return eventItemList;
            }
            catch(Exception ex)
            {
                //Log error
                msg = ex.ToString();
                return null;
            }
        }

        public static bool DeleteEvent(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, int id, string user, out string msg)
        {
            try
            {
                using (var db = new OnyxEntities())
                {
                    var eventItem = db.BUS_WFMDashboard_Event.Where(t => t.Id == id).FirstOrDefault();
                    var repeatingItem = db.BUS_WFMDashboard_Repeating_Event.Where(t => t.EventId == eventItem.Id).FirstOrDefault();
                    
                    var googleCalId = eventItem.CalendarEventId;
                    
                    var before = ObjectPrintHelper.PrintEvent(eventItem, repeatingItem);
                    var after = "-----";
                    CreateActionHistory(eventItem.Id, 0, null, "delete", $"Event deleted by {user}", before, after, user);

                    if (repeatingItem != null)
                        db.BUS_WFMDashboard_Repeating_Event.Remove(repeatingItem);
                    db.BUS_WFMDashboard_Event.Remove(eventItem);
                    db.SaveChanges();

                    var gcalResult = DeleteEvent_GoogleCalendar(googleAuth, googleCalId);

                    
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

        public static Dictionary<int, List<ViewIcmSchedule>> SubmitIcmForm(int month, int year, int agentNo, out string msg, out bool success)
        {
            try
            {
                success = true;
                msg = "Successfully added ICM row";
                string managerName = "";
                using (var db = new inContact_NGEntities())
                {
                    var manager = db.Agents.Where(t => t.AgentNo == agentNo).FirstOrDefault();
                    managerName = manager.FirstName + " " + manager.LastName;
                }
                using (var db = new OnyxEntities())
                {
                    var existingOne = db.BUS_WFMDashboard_ICM.Where(t => t.Month == month && t.Year == year).FirstOrDefault();
                    if (existingOne == null)
                    {
                        var newOne = new BUS_WFMDashboard_ICM()
                        {
                            AgentNo = agentNo,
                            Manager = managerName,
                            Month = month,
                            Year = year
                        };
                        db.BUS_WFMDashboard_ICM.Add(newOne);
                    }
                    else
                    {
                        existingOne.AgentNo = agentNo;
                        existingOne.Manager = managerName;
                    }
                    db.SaveChanges();
                }
                return GetIcmSchedule();

            }
            catch(Exception ex)
            {
                log.Error("Error creating ICM row", ex);
                msg = "Error creating ICM row";
                success = false;
                return null;
            }
        }

        public static Dictionary<DateTime, List<ViewMowSchedule>> SubmitMowForm(MowFormInput Item, string MondayString, out string msg, out bool success)
        {
            try
            {
                success = true;
                msg = "Successfully added MOW row(s)";
                using (var dbN = new inContact_NGEntities())
                using (var db = new OnyxEntities())
                {
                    foreach (var inputItem in Item.InputItems)
                    {
                        string agentName = "";

                        var agent = dbN.Agents.Where(t => t.AgentNo == inputItem.agentNo).FirstOrDefault();
                        agentName = agent.FirstName + " " + agent.LastName;
                        

                        if(inputItem.rowId != 0)
                        {
                            var oldRow = db.BUS_WFMDashboard_WFO_Schedule.Where(t => t.Id == inputItem.rowId).FirstOrDefault();
                            var startDate = oldRow.StartTime.Date.ToShortDateString();
                            var endDate = oldRow.EndTime.Date.ToShortDateString();
                            var startTime = DateTime.Parse(startDate + " " + inputItem.shiftStart);
                            var endTime = DateTime.Parse(endDate + " " + inputItem.shiftEnd);
                            oldRow.AgentNo = agent.AgentNo;
                            oldRow.Task = inputItem.task;
                            oldRow.FirstName = agent.FirstName;
                            oldRow.LastName = agent.LastName;
                            oldRow.StartTime = startTime;
                            oldRow.EndTime = endTime;
                        }
                        else
                        {
                            var startTime = DateTime.Parse(Item.Date + " " + inputItem.shiftStart);
                            var endTime = DateTime.Parse(Item.Date + " " + inputItem.shiftEnd);
                            var newRow = new BUS_WFMDashboard_WFO_Schedule()
                            {
                                AgentNo = agent.AgentNo,
                                Task = inputItem.task,
                                FirstName = agent.FirstName,
                                LastName = agent.LastName,
                                StartTime = startTime,
                                EndTime = endTime
                            };
                            db.BUS_WFMDashboard_WFO_Schedule.Add(newRow);
                        }

                        
                    }
                    db.SaveChanges();
                }
                return GetMowSchedule(DateTime.Parse(MondayString));
            }
            catch (Exception ex)
            {
                log.Error("Error creating MOW rows", ex);
                msg = "Error creating MOW rows";
                success = false;
                return null;
            }
            
        }

        public static bool DeleteMowRow(int rowId, out string msg)
        {
            try
            {
                using (var db = new OnyxEntities())
                {
                    var deleteItem = db.BUS_WFMDashboard_WFO_Schedule.Where(t => t.Id == rowId).FirstOrDefault();
                    db.BUS_WFMDashboard_WFO_Schedule.Remove(deleteItem);
                    db.SaveChanges();
                }
                msg = "Successfully deleted schedule row";
                return true;
            }
            catch(Exception ex)
            {
                log.Error("error deleting Mow row", ex);
                msg = ex.ToString();
                return false;
            }
        }

        public static DownByReport CreateDownByReport(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, out string msg)
        {
            try
            {
                //var report = GenerateDownByReport_Old(googleAuth);

                ScanGoogleCalendar(googleAuth);
                var report = GenerateDownByReport(googleAuth);

                msg = "Success";

                return report;


     

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
                msg = ex.ToString();
                log.Error("Error in CreateDownByReport", ex);
                return null;
            }
        }

        private static void ScanGoogleCalendar(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth)
        {
            //DownByReport report = new DownByReport();
            var calendarId = ConfigurationManager.AppSettings["CalendarId"];

            var service = CreateCalendarService(googleAuth);

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List(calendarId);

            request.TimeMin = DateTime.Today.Date; //Start of day
            request.TimeMax = DateTime.Today.Date.AddHours(23).AddMinutes(59); //End of day
            //request.TimeMax = DateTime.Today.AddDays(30);

            request.ShowDeleted = false;
            request.SingleEvents = true;
            //request.MaxResults = 20;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events
            Events events = request.Execute();

            CreateDatabaseEvents(events);

            //return report;

            return;
        }
        public static DownByReport GenerateDownByReport_Old(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth)
        {
            //DownByReport report = new DownByReport();
            var calendarId = ConfigurationManager.AppSettings["CalendarId"];

            var service = CreateCalendarService(googleAuth);

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List(calendarId);

            request.TimeMin = DateTime.Today.AddDays(15); ; //Start of day
            //request.TimeMax = DateTime.Today.Date.AddHours(23).AddMinutes(59); //End of day
            request.TimeMax = DateTime.Today.AddDays(30);

            request.ShowDeleted = false;
            request.SingleEvents = true;
            //request.MaxResults = 20;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();

            DownByReport report = FillDownByReport(events);

            //return report;

            return null;
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

            //report = FillDownByReportSectionFullDay(report, fullDayTraining, "Training");
            //report = FillDownByReportSectionFullDay(report, fullDayOther, "Other");
            //report = FillDownByReportSectionFullDay(report, paidPto, "PTO");

            return report;
        }


        public static DownByReport GenerateDownByReport(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth)
        {
            var timeMin = DateTime.Today.Date; //Start of day
            var timeMax = DateTime.Today.Date.AddHours(23).AddMinutes(59); //End of day
            using (var db = new OnyxEntities())
            {
                var events = db.BUS_WFMDashboard_Event.Where(t => t.StartTime <= timeMax && t.EndTime >= timeMin && !t.Description.ToLower().Contains("cancelled")).ToList();
                DownByReport report = FillDownByReport(events);
                return report;
            }
        }

        public static DownByReport FillDownByReport(List<BUS_WFMDashboard_Event> events)
        {
            var report = new DownByReport();

            var ptoDateEnd = DateTime.Now.AddDays(1).Date;
            var partialTypeList =  new string [] { "PTO (Unplanned)", "PTO (Planned)", "Unpaid Time Off" };
            var partialEvents = events.Where(t => !t.FullDay && t.StartTime >= DateTime.Now.Date && t.EndTime <= ptoDateEnd && partialTypeList.Contains(t.EventType)).ToList();
            //var partialEvents = events.Where(t => !t.FullDay && t.StartTime >= DateTime.Now.Date && t.EndTime <= ptoDateEnd).ToList();
            var fullDayEvents = events.Where(t => t.FullDay && t.EndTime >= ptoDateEnd).ToList();
            report = FillDownByReportSectionFullDay(report, fullDayEvents);
            report = FillDownByReportSectionPartialDay(report, partialEvents);
            var rangeEvents = events.Where(t => !t.FullDay).OrderBy(t => t.StartTime.TimeOfDay).ToList();
            foreach (var item in rangeEvents)
            {
                var downByEvent = new DownByEvent(item.Color);
                var startString = item.StartTime.ToShortTimeString();
                var endString = item.EndTime.ToShortTimeString();
                downByEvent.DateRange = $"{startString} - {endString}";
                downByEvent.StartTime = item.StartTime;
                downByEvent.Title = item.Description;
                report.Events.Add(downByEvent);
            }

            return report;
        }
        public static DownByReport FillDownByReportSectionFullDay(DownByReport report, List<BUS_WFMDashboard_Event> events)
        {
            List<BUS_WFMDashboard_Event> eventList = new List<BUS_WFMDashboard_Event>();
            using (var db = new OnyxEntities())
            using (var dbS = new inContact_NGEntities())
            {
                foreach (var item in events)
                {
                    var teamName = item.TeamName;
                    var staffName = item.LastName;
                    var eventType = "";
                    switch (item.EventType)
                    {
                        case "PTO (Unplanned)":
                        case "PTO (Planned)":
                            eventType = "PTO";
                            break;
                        case "Training":
                            eventType = "Training";
                            break;
                        case "LOA":
                            eventType = "LOA";
                            break;
                        default:
                            eventType = "Other";
                            break;
                    }
                    bool foundTeam = false;
                    if (teamName.Contains("RA"))
                    {
                        report.Sections[eventType].RA = report.Sections[eventType].RA.TrimStart('0');
                        report.Sections[eventType].RA += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].RA = report.Sections[eventType].RA.TrimStart(',').TrimStart(' ');
                        foundTeam = true;
                    }
                    if (teamName.Contains("AD"))
                    {
                        report.Sections[eventType].AD = report.Sections[eventType].AD.TrimStart('0');
                        report.Sections[eventType].AD += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].AD = report.Sections[eventType].AD.TrimStart(',').TrimStart(' ');
                        foundTeam = true;
                    }
                    if (teamName.Contains("IRC"))
                    {
                        report.Sections[eventType].IRC = report.Sections[eventType].IRC.TrimStart('0');
                        report.Sections[eventType].IRC += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].IRC = report.Sections[eventType].IRC.TrimStart(',').TrimStart(' ');
                        foundTeam = true;
                    }
                    if (teamName.ToLower().Contains("printer"))
                    {
                        report.Sections[eventType].PrinterOps = report.Sections[eventType].PrinterOps.TrimStart('0');
                        report.Sections[eventType].PrinterOps += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].PrinterOps = report.Sections[eventType].PrinterOps.TrimStart(',').TrimStart(' ');
                        foundTeam = true;
                    }
                    if (teamName.Contains("MGR") || teamName.ToLower().Contains("manage"))
                    {
                        report.Sections[eventType].MGR = report.Sections[eventType].MGR.TrimStart('0');
                        report.Sections[eventType].MGR += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].MGR = report.Sections[eventType].MGR.TrimStart(',').TrimStart(' ');
                        foundTeam = true;
                    }
                    if (teamName.Contains("POA"))
                    {
                        report.Sections[eventType].POA = report.Sections[eventType].POA.TrimStart('0');
                        report.Sections[eventType].POA += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
                        report.Sections[eventType].POA = report.Sections[eventType].POA.TrimStart(',').TrimStart(' ');
                        foundTeam = true;
                    }
                    if(foundTeam)
                    {
                        report.TotalDown++;
                        report.Sections[eventType].TotalDown++;
                    }
                }
            }
            return report;
        }

        public static DownByReport FillDownByReportSectionPartialDay(DownByReport report, List<BUS_WFMDashboard_Event> events)
        {
            List<BUS_WFMDashboard_Event> eventList = new List<BUS_WFMDashboard_Event>();
            using (var db = new OnyxEntities())
            using (var dbS = new inContact_NGEntities())
            {
                foreach (var item in events)
                {
                    var eventType = "";
                    var teamName = item.TeamName;
                    var staffName = item.LastName;
                    switch (item.EventType)
                    {
                        case "PTO (Unplanned)":
                        case "PTO (Planned)":
                            eventType = "PTO";
                            break;
                        case "Training":
                            eventType = "Training";
                            break;
                        case "LOA":
                            eventType = "LOA";
                            break;
                        default:
                            eventType = "Other";
                            break;
                    }

                    TimeSpan diff = item.EndTime - item.StartTime;
                    double hours = diff.TotalHours;
                    var fraction = Math.Round((hours / 8.0), 1);
                    bool foundTeam = false;
                    if (teamName.Contains("RA"))
                    {
                        foundTeam = true;
                        report.Sections[eventType].RA = report.Sections[eventType].RA.TrimStart('0');
                        report.Sections[eventType].RA += $", {staffName.TrimStart(' ').TrimEnd(' ')} ({item.StartTime.ToShortTimeString()}-{item.EndTime.ToShortTimeString()})";
                        report.Sections[eventType].RA = report.Sections[eventType].RA.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.Contains("AD"))
                    {
                        foundTeam = true;
                        report.Sections[eventType].AD = report.Sections[eventType].AD.TrimStart('0');
                        report.Sections[eventType].AD += $", {staffName.TrimStart(' ').TrimEnd(' ')} ({item.StartTime.ToShortTimeString()}-{item.EndTime.ToShortTimeString()})";
                        report.Sections[eventType].AD = report.Sections[eventType].AD.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.Contains("IRC"))
                    {
                        foundTeam = true;
                        report.Sections[eventType].IRC = report.Sections[eventType].IRC.TrimStart('0');
                        report.Sections[eventType].IRC += $", {staffName.TrimStart(' ').TrimEnd(' ')} ({item.StartTime.ToShortTimeString()}-{item.EndTime.ToShortTimeString()})";
                        report.Sections[eventType].IRC = report.Sections[eventType].IRC.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.ToLower().Contains("printer"))
                    {
                        foundTeam = true;
                        report.Sections[eventType].PrinterOps = report.Sections[eventType].PrinterOps.TrimStart('0');
                        report.Sections[eventType].PrinterOps += $", {staffName.TrimStart(' ').TrimEnd(' ')} ({item.StartTime.ToShortTimeString()}-{item.EndTime.ToShortTimeString()})";
                        report.Sections[eventType].PrinterOps = report.Sections[eventType].PrinterOps.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.Contains("MGR"))
                    {
                        foundTeam = true;
                        report.Sections[eventType].MGR = report.Sections[eventType].MGR.TrimStart('0');
                        report.Sections[eventType].MGR += $", {staffName.TrimStart(' ').TrimEnd(' ')} ({item.StartTime.ToShortTimeString()}-{item.EndTime.ToShortTimeString()})";
                        report.Sections[eventType].MGR = report.Sections[eventType].MGR.TrimStart(',').TrimStart(' ');
                    }
                    if (teamName.Contains("POA"))
                    {
                        foundTeam = true;
                        report.Sections[eventType].POA = report.Sections[eventType].POA.TrimStart('0');
                        report.Sections[eventType].POA += $", {staffName.TrimStart(' ').TrimEnd(' ')} ({item.StartTime.ToShortTimeString()}-{item.EndTime.ToShortTimeString()})";
                        report.Sections[eventType].POA = report.Sections[eventType].POA.TrimStart(',').TrimStart(' ');
                    }
                    if (foundTeam)
                    {
                        report.TotalDown += fraction;
                        report.Sections[eventType].TotalDown += fraction;
                    }
                }
            }
            return report;
        }


        //public static DownByReport FillDownByReport(List<BUS_WFMDashboard_Event> events)
        //{
        //    var report = new DownByReport();
        //    //var ptoDateEnd = DateTime.Now.AddDays(1).Date.ToString("yyyy-MM-dd"); 
        //    var ptoDateEnd = DateTime.Now.AddDays(1).Date;

        //    var paidPto = events.Where(t => (t.Color == "10" || t.Color == "9") && t.FullDay && t.EndTime >= ptoDateEnd).ToList();

        //    var fullDayTraining = events.Where(t => t.Color == "7" && t.FullDay && t.EndTime >= ptoDateEnd).ToList();
        //    fullDayTraining = fullDayTraining.Where(t => !t.Description.ToLower().Contains("cancelled")).ToList();
        //    var fullDayOther = events.Where(t => t.Color == "11" && t.FullDay && t.EndTime >= ptoDateEnd).ToList();

        //    //var partialTraining = events.Items.Where(t => t.ColorId == "7" && t.End.DateTime != null).ToList();

        //    //CreateDatabaseEvents(events);

        //    var rangeEvents = events.Where(t => !t.FullDay).OrderBy(t => t.StartTime.TimeOfDay).ToList();
        //    foreach (var item in rangeEvents)
        //    {
        //        var downByEvent = new DownByEvent(item.Color);
        //        var startString = item.StartTime.ToShortTimeString();
        //        var endString = item.EndTime.ToShortTimeString();
        //        downByEvent.DateRange = $"{startString} - {endString}";
        //        downByEvent.StartTime = item.StartTime;
        //        downByEvent.Title = item.Description;
        //        report.Events.Add(downByEvent);
        //    }
        //    var darkRedEvents = events.Where(t => t.Color == "11").ToList();
        //    var lightRedEvents = events.Where(t => t.Color == "4").ToList();

        //    report = FillDownByReportSectionFullDay(report, fullDayTraining, "Training");
        //    report = FillDownByReportSectionFullDay(report, fullDayOther, "Other");
        //    report = FillDownByReportSectionFullDay(report, paidPto, "PTO");

        //    return report;
        //}

        //public static DownByReport FillDownByReportSectionFullDay(DownByReport report, List<BUS_WFMDashboard_Event> events, string eventType)
        //{
        //    List<BUS_WFMDashboard_Event> eventList = new List<BUS_WFMDashboard_Event>();
        //    using (var db = new OnyxEntities())
        //    using (var dbS = new inContact_NGEntities())
        //    {
        //        foreach (var item in events)
        //        {
        //            var teamName = item.TeamName;
        //            var staffName = item.LastName;

        //            report.TotalDown++;
        //            report.Sections[eventType].TotalDown++;
        //            if (teamName.Contains("RA"))
        //            {
        //                report.Sections[eventType].RA = report.Sections[eventType].RA.TrimStart('0');
        //                report.Sections[eventType].RA += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
        //                report.Sections[eventType].RA = report.Sections[eventType].RA.TrimStart(',').TrimStart(' ');
        //            }
        //            if (teamName.Contains("AD"))
        //            {
        //                report.Sections[eventType].AD = report.Sections[eventType].AD.TrimStart('0');
        //                report.Sections[eventType].AD += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
        //                report.Sections[eventType].AD = report.Sections[eventType].AD.TrimStart(',').TrimStart(' ');
        //            }
        //            if (teamName.Contains("IRC"))
        //            {
        //                report.Sections[eventType].IRC = report.Sections[eventType].IRC.TrimStart('0');
        //                report.Sections[eventType].IRC += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
        //                report.Sections[eventType].IRC = report.Sections[eventType].IRC.TrimStart(',').TrimStart(' ');
        //            }
        //            if (teamName.ToLower().Contains("printer"))
        //            {
        //                report.Sections[eventType].PrinterOps = report.Sections[eventType].PrinterOps.TrimStart('0');
        //                report.Sections[eventType].PrinterOps += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
        //                report.Sections[eventType].PrinterOps = report.Sections[eventType].PrinterOps.TrimStart(',').TrimStart(' ');
        //            }
        //            if (teamName.Contains("MGR"))
        //            {
        //                report.Sections[eventType].MGR = report.Sections[eventType].MGR.TrimStart('0');
        //                report.Sections[eventType].MGR += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
        //                report.Sections[eventType].MGR = report.Sections[eventType].MGR.TrimStart(',').TrimStart(' ');
        //            }
        //            if (teamName.Contains("POA"))
        //            {
        //                report.Sections[eventType].POA = report.Sections[eventType].POA.TrimStart('0');
        //                report.Sections[eventType].POA += $", {staffName.TrimStart(' ').TrimEnd(' ')}";
        //                report.Sections[eventType].POA = report.Sections[eventType].POA.TrimStart(',').TrimStart(' ');
        //            }
        //        }
        //    }
        //    return report;
        //}


        public static DownByReport FillDownByReportSectionFullDayOld(DownByReport report, List<Event> events, string eventType)
        {
            List<BUS_WFMDashboard_Event> eventList = new List<BUS_WFMDashboard_Event>();
            using (var db = new OnyxEntities())
            using (var dbS = new inContact_NGEntities())
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



        public static bool CreateDatabaseEvents(Events events)
        {
            using (var db = new OnyxEntities())
            using (var dbS = new inContact_NGEntities())
            {
                foreach (var item in events.Items)
                {
                    string eventType = "Other";

                    var lowerEventTitle = item.Summary.ToLower();

                    if(lowerEventTitle.Contains("pto"))
                    {
                        if (lowerEventTitle.Contains("unplanned"))
                            eventType = "PTO (Unplanned)";
                        else
                            eventType = "PTO (Planned)";
                    }
                    if (lowerEventTitle.Contains("unpaid"))
                    {
                        eventType = "Unpaid Time Off";
                    }
                    if (lowerEventTitle.Contains("training"))
                    {
                        eventType = "Training";
                    }
                    if (lowerEventTitle.Contains("loa"))
                    {
                        eventType = "LOA";
                    }

                    var sections = item.Summary.Split('-');
                    if(sections.Length > 1)
                    {
                        var teamName = sections[0];
                        var staffName = sections[1];
                        bool fullDay = true;

                        var staffMember = dbS.Agents.Where(t => staffName.Contains(t.LastName.ToLower()) && t.Status == "Active").FirstOrDefault();
                        if (staffMember != null)
                        {
                            DateTime startTime = DateTime.Now;
                            DateTime endTime = DateTime.Now;
                            if (item.End.Date != null)
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
                                var description = item.Description;
                                if (description == null)
                                    description = "";
                                var newEvent = new BUS_WFMDashboard_Event()
                                {
                                    FirstName = staffMember.FirstName,
                                    Notes = description,
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
                                    Color = item.ColorId,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = "Google Calendar",
                                    UpdatedAt = DateTime.Now,
                                    UpdatedBy = "Google Calendar"
                                };
                                db.BUS_WFMDashboard_Event.Add(newEvent);
                                //db.SaveChanges();
                            }
                            else
                            {
                                var description = item.Description;
                                if (description == null)
                                    description = "";
                                prevEvent.FirstName = staffMember.FirstName;
                                prevEvent.Notes = description;
                                prevEvent.CalendarEventId = item.Id;
                                prevEvent.AgentNo = staffMember.AgentNo;
                                prevEvent.StartTime = startTime;
                                prevEvent.EndTime = endTime;
                                prevEvent.EventType = eventType;
                                prevEvent.FullDay = fullDay;
                                prevEvent.TeamId = staffMember.TeamNo;
                                prevEvent.TeamName = staffMember.TeamName;
                                prevEvent.LastName = staffName;
                                prevEvent.Description = item.Summary;
                                prevEvent.Color = item.ColorId;
                                prevEvent.UpdatedAt = DateTime.Now;
                                prevEvent.UpdatedBy = "Google Calendar";
                            }
                        }
                    }
                    
                }
                db.SaveChanges();
            }
            return true;
        }

        
        public static MowReport CreateMOWReport(out string msg)
        {
            try
            {
                msg = "Success";
                var report =  new MowReport();
                var now = DateTime.Now;
                using (var db = new OnyxEntities())
                {
                    var startTime = now.Date;
                    var endTime = now.Date.AddDays(1);
                    var icm = db.BUS_WFMDashboard_ICM.Where(t => t.Month == now.Month && t.Year == now.Year).FirstOrDefault();
                    report.ICM = $"({now.ToString("MMMM")}) {icm.Manager}";
                    var lateShift = db.BUS_WFMDashboard_LateShiftManager.Where(t => t.Date >= startTime && t.Date <= endTime).FirstOrDefault();
                    if (lateShift == null)
                        report.LateShiftManager = " --- ";
                    else
                        report.LateShiftManager = lateShift.FirstName + " " + lateShift.LastName;
                    
                    var wfos = db.BUS_WFMDashboard_WFO_Schedule.Where(t => t.StartTime >= startTime && t.EndTime <= endTime && t.Task == "MOW").OrderBy(t => t.StartTime).ToList();
                    foreach (var item in wfos)
                    {
                        report.WfoList.Add($"{item.LastName} - {item.StartTime.ToShortTimeString()} to {item.EndTime.ToShortTimeString()}");
                    }
                }

                return report;
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                log.Error("Error in CreateMOWReport", ex);
                return null;
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



        //Private support methods


        private static bool SubmitTimeOff_Nice()
        {
            bool success = false;
            return success;
        }


        private static ViewEvent CreateEvent(string googleEventId, EventForm inputForm, DateTime startDateTime, DateTime endDateTime, Agent agent, string user)
        {
            try
            {
                BUS_WFMDashboard_Event busEvent = new BUS_WFMDashboard_Event()
                {
                    AgentNo = agent.AgentNo,
                    TeamName = agent.TeamName,
                    TeamId = agent.TeamNo,
                    FullDay = inputForm.fullDay,
                    EventType = inputForm.eventType,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    Description = inputForm.title,
                    Notes = inputForm.notes,
                    LastName = agent.LastName,
                    FirstName = agent.FirstName,
                    CalendarEventId = googleEventId,
                    Color = inputForm.color,
                    CreatedAt = DateTime.Now,
                    CreatedBy = user,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = user
                };
                using (var db = new OnyxEntities())
                {
                    db.BUS_WFMDashboard_Event.Add(busEvent);
                    db.SaveChanges();
                    if(inputForm.repeatingEvent)
                    {
                        inputForm.eventId = busEvent.Id; //TODO: make sure we actually have the Id here.
                        CalculateRepeatingEvents(inputForm, db);
                    }
                }
                return new ViewEvent(busEvent, false, inputForm.repeatingEvent);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static BUS_WFMDashboard_Repeating_Event CalculateRepeatingEvents(EventForm inputForm, OnyxEntities db)
        {
            try
            {
                bool newItem = false;
                BUS_WFMDashboard_Repeating_Event repeatItem = db.BUS_WFMDashboard_Repeating_Event.Where(t => t.EventId == inputForm.eventId).FirstOrDefault();
                if(repeatItem == null)
                {
                    newItem = true;
                    repeatItem = new BUS_WFMDashboard_Repeating_Event();
                    if(inputForm.repeatEndDate != null)
                        repeatItem.EndDate = DateTime.Parse(inputForm.repeatEndDate);
                    repeatItem.EndAfterOccurences = inputForm.repeatEndAfterNumber;
                    repeatItem.RepeatType = inputForm.repeatType;
                    repeatItem.RepeatEveryNumber = inputForm.repeatEveryNumber;
                    repeatItem.RepeatOnDays = inputForm.repeatOnDays;
                    repeatItem.RepeatSummary = inputForm.repeatSummary;
                    repeatItem.StartDate = DateTime.Parse(inputForm.startDate);
                    repeatItem.EndType = inputForm.repeatEndType;
                    repeatItem.EventId = inputForm.eventId;
                }
                else
                {
                    if(inputForm.repeatEndType != null)
                    {
                        if (inputForm.repeatEndDate != null)
                            repeatItem.EndDate = DateTime.Parse(inputForm.repeatEndDate);

                        repeatItem.EndAfterOccurences = inputForm.repeatEndAfterNumber;
                        repeatItem.RepeatType = inputForm.repeatType;
                        repeatItem.RepeatEveryNumber = inputForm.repeatEveryNumber;
                        repeatItem.RepeatOnDays = inputForm.repeatOnDays;
                        repeatItem.RepeatSummary = inputForm.repeatSummary;
                        repeatItem.EndType = inputForm.repeatEndType;
                        if (inputForm.repeatEndType == "date")
                            repeatItem.EndAfterOccurences = null;
                        else
                            repeatItem.EndDate = null;
                    }
                }
                repeatItem.StartDate = DateTime.Parse(inputForm.startDate);
                if (repeatItem.EndType.ToLower() != "never")
                {
                    if(repeatItem.EndType.ToLower() == "number")
                    {
                        var startDate = repeatItem.StartDate;
                        var workingDate = startDate;
                        var repeatEvery = repeatItem.RepeatEveryNumber;
                        switch (repeatItem.RepeatType)
                        {
                            case "0": //Daily
                                repeatItem.CalculatedEndDate = repeatItem.StartDate.AddDays(repeatItem.EndAfterOccurences.Value * repeatEvery.Value);
                                break;
                            case "1": // Weekdays
                                repeatItem.RepeatOnDays = "MO,TU,WE,TH,FR";
                                repeatItem.CalculatedEndDate = repeatItem.StartDate.AddBusinessDays(repeatItem.EndAfterOccurences.Value);
                                break;
                            case "2": // M W F
                                repeatItem.RepeatOnDays = "MO,WE,FR";
                                repeatItem.CalculatedEndDate = WFMConsole.Classes.DateTimeExtensions.GetSpecifiedDaysFromDate(workingDate, new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, repeatItem.EndAfterOccurences.Value, repeatEvery.Value).Last();
                                break;
                            case "3": // TU TH
                                repeatItem.RepeatOnDays = "TU,TH";
                                repeatItem.CalculatedEndDate = WFMConsole.Classes.DateTimeExtensions.GetSpecifiedDaysFromDate(workingDate, new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }, repeatItem.EndAfterOccurences.Value, repeatEvery.Value).Last();
                                break;
                            case "4": // Weekly on certain days
                                var specifiedDays = repeatItem.RepeatOnDays.TrimEnd(',').Split(',');
                                var importantDays = new List<DayOfWeek>();
                                foreach (var item in specifiedDays)
                                {
                                    switch (item)
                                    {
                                        case "SU": importantDays.Add(DayOfWeek.Sunday); break;
                                        case "MO": importantDays.Add(DayOfWeek.Monday); break;
                                        case "TU": importantDays.Add(DayOfWeek.Tuesday); break;
                                        case "WE": importantDays.Add(DayOfWeek.Wednesday); break;
                                        case "TH": importantDays.Add(DayOfWeek.Thursday); break;
                                        case "FR": importantDays.Add(DayOfWeek.Friday); break;
                                        case "SA": importantDays.Add(DayOfWeek.Saturday); break;
                                    }
                                }
                                repeatItem.CalculatedEndDate = repeatItem.CalculatedEndDate = WFMConsole.Classes.DateTimeExtensions.GetSpecifiedDaysFromDate(workingDate, importantDays, repeatItem.EndAfterOccurences.Value, repeatEvery.Value).Last();
                                break;
                            case "5": //Monthly
                                repeatItem.CalculatedEndDate = repeatItem.StartDate.AddMonths(repeatItem.EndAfterOccurences.Value * repeatEvery.Value);
                                break;
                            case "6": //Annually
                                repeatItem.CalculatedEndDate = repeatItem.StartDate.AddYears(repeatItem.EndAfterOccurences.Value * repeatEvery.Value);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        repeatItem.CalculatedEndDate = repeatItem.EndDate;
                    }
                }
                if(newItem)
                    db.BUS_WFMDashboard_Repeating_Event.Add(repeatItem);

                db.SaveChanges();
                return repeatItem;
            }
            catch(Exception ex)
            {
                log.Error("Error with calculating repeating event", ex);
                return null;
            }
        }

        private static ViewEvent UpdateEvent(EventForm inputForm, DateTime startDateTime, DateTime endDateTime, string user)
        {
            try
            {
                using (var db = new OnyxEntities())
                {
                    BUS_WFMDashboard_Event busEvent = db.BUS_WFMDashboard_Event.Where(t => t.Id == inputForm.eventId).FirstOrDefault();
                    var eventRepeatItem = db.BUS_WFMDashboard_Repeating_Event.Where(t => t.EventId == busEvent.Id).FirstOrDefault();
                    var before = ObjectPrintHelper.PrintEvent(busEvent, eventRepeatItem);
                    busEvent.FullDay = inputForm.fullDay;
                    busEvent.EventType = inputForm.eventType;
                    busEvent.StartTime = startDateTime;
                    busEvent.EndTime = endDateTime;
                    busEvent.Description = inputForm.title;
                    busEvent.Notes = inputForm.notes;
                    busEvent.Color = inputForm.color;
                    busEvent.UpdatedAt = DateTime.Now;
                    busEvent.UpdatedBy = user;
                    db.SaveChanges();
                    
                    if (inputForm.repeatingEvent)
                    {
                        eventRepeatItem = CalculateRepeatingEvents(inputForm, db);
                    }
                    
                    var after = ObjectPrintHelper.PrintEvent(busEvent, eventRepeatItem);

                    //TODO: Show repeating stuff in action history here...
                    CreateActionHistory(busEvent.Id, 0, null, "delete", $"Event deleted by {user}", before, after, user);
                    return new ViewEvent(busEvent, false, inputForm.repeatingEvent);

                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string SubmitTimeOff_GoogleCalendar(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, EventForm inputForm, DateTime startDateTime, DateTime endDateTime, Agent agent)
        {
            try
            {
                String calendarId = ConfigurationManager.AppSettings["CalendarId"];
                var team = agent.TeamName;
                team = ProcessTeamName(team);

                var lastName = agent.LastName;

                string summary = inputForm.title;
                string colorId = inputForm.color;

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
                if (inputForm.fullDay)
                {
                    start.Date = startDateTime.ToString("yyyy-MM-dd");
                    end.Date = endDateTime.ToString("yyyy-MM-dd");
                }
                else
                {
                    start.DateTime = startDateTime;
                    end.DateTime = endDateTime;
                }

                var service = CreateCalendarService(googleAuth);

                Event newEvent = new Event()
                {
                    Summary = summary,
                    Description = inputForm.notes,
                    Start = start,
                    ColorId = colorId,
                    End = end,
                    Attendees = new EventAttendee[] {
                        new EventAttendee() { Email = "rtyszka@kmbs.konicaminolta.us" },
                    },
                };

                if(inputForm.repeatingEvent)
                {
                    var rRule = CreateGCalRecurringString(inputForm);
                    newEvent.Recurrence = new String[] {
                          rRule
                    };
                }

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

        private static string CreateGCalRecurringString(EventForm inputForm )
        {
            var rRule = "RRULE:";
            switch (inputForm.repeatType)
            {
                case "0":
                    rRule += "FREQ=DAILY;";
                    if (inputForm.repeatEveryNumber > 1)
                        rRule += $"INTERVAL={inputForm.repeatEveryNumber};";
                    break;
                case "1":
                    rRule += "FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR;";
                    break;
                case "2":
                    rRule += "FREQ=WEEKLY;BYDAY=MO,WE,FR;";
                    break;
                case "3":
                    rRule += "FREQ=WEEKLY;BYDAY=TU,TH;";
                    break;
                case "4":
                    rRule += $"FREQ=WEEKLY;BYDAY={inputForm.repeatOnDays};";
                    if (inputForm.repeatEveryNumber > 1)
                        rRule += $"INTERVAL={inputForm.repeatEveryNumber};";
                    break;
                case "5":
                    rRule += "FREQ=MONTHLY;";
                    if (inputForm.repeatEveryNumber > 1)
                        rRule += $"INTERVAL={inputForm.repeatEveryNumber};";
                    break;
                case "6":
                    rRule += "FREQ=YEARLY;";
                    if (inputForm.repeatEveryNumber > 1)
                        rRule += $"INTERVAL={inputForm.repeatEveryNumber};";
                    break;
                default:
                    break;
            }

            switch (inputForm.repeatEndType.ToLower())
            {
                case "number":
                    rRule += $"COUNT={inputForm.repeatEndAfterNumber}";
                    break;
                case "date":
                    rRule += $"UNTIL={DateTime.Parse(inputForm.repeatEndDate).ToString("yyyyMMdd")}";
                    break;
                default:
                    break;
            }
            return rRule;
        }


        private static string UpdateEvent_GoogleCalendar(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, ViewEvent eventItem, EventForm inputForm)
        {
            try
            {
                String calendarId = ConfigurationManager.AppSettings["CalendarId"];
                //string colorId = color; //Color for unplanned PTO

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

                if (eventItem.allDay)
                {
                    start.Date = DateTime.Parse(eventItem.StartDate).ToString("yyyy-MM-dd");
                    end.Date = DateTime.Parse(eventItem.StartDate).ToString("yyyy-MM-dd");
                }
                else
                {
                    start.DateTime = DateTime.Parse(eventItem.start);
                    end.DateTime = DateTime.Parse(eventItem.end);
                }

                var service = CreateCalendarService(googleAuth);

                Event newEvent = new Event()
                {
                    Description = eventItem.Notes,
                    Summary = eventItem.title,
                    Start = start,
                    ColorId = eventItem.ColorId,
                    End = end
                };
                if(inputForm.repeatingEvent && inputForm.repeatEndType != null && inputForm.repeatSummary != null)
                {
                    var rRule = CreateGCalRecurringString(inputForm);
                    newEvent.Recurrence = new String[] {
                        rRule
                    };
                }
                
                EventsResource.PatchRequest request = service.Events.Patch(newEvent, calendarId, eventItem.CalendarEventId);
                Event updatedEvent = request.Execute();
                if (updatedEvent != null)
                {
                    return "Successfully updated event";
                }
                else
                {
                    return "Error updating event on google calendar";
                }
                ////bool success = false;
                //return createdEvent.Id;
            }
            catch (Exception ex)
            {
                log.Error("Error in SubmitTimeOff_GoogleCalendar", ex);
                //throw ex;
                return ex.ToString();
            }
        }


        private static string DeleteEvent_GoogleCalendar(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, string eventId)
        {
            try
            {
                String calendarId = ConfigurationManager.AppSettings["CalendarId"];

                var service = CreateCalendarService(googleAuth);

                EventsResource.DeleteRequest request = service.Events.Delete(calendarId, eventId);
                var result = request.Execute();
                //bool success = false;
                return result;
            }
            catch (Exception ex)
            {
                log.Error("Error in SubmitTimeOff_GoogleCalendar", ex);
                return ex.ToString();
            }
        }

        private static string ProcessTeamName(string team)
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

        private static CalendarService CreateCalendarService(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth)
        {
            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleAuth.Credential,
                ApplicationName = ApplicationName,
                ApiKey = ConfigurationManager.AppSettings["ApiKey"],
            });
        }

        private static List<DateTime> GetRepeatingEventDateList(BUS_WFMDashboard_Repeating_Event repeatItem, DateTime minDate, DateTime maxDate)
        {
            var dateList = new List<DateTime>();

            var startDate = repeatItem.StartDate;
            var workingDate = startDate;
            var repeatEvery = repeatItem.RepeatEveryNumber;

            var repeatEndDate = repeatItem.CalculatedEndDate;
            if (repeatEndDate > maxDate)
                repeatEndDate = maxDate;
            var repeatEndType = repeatItem.EndType;
            var repeatOccurences = repeatItem.EndAfterOccurences;

            if(repeatEndType == "never")
            {
                repeatEndType = "date";
                repeatEndDate = maxDate;
            }

            switch (repeatItem.RepeatType)
            {
                case "0": //Daily
                    dateList = WFMConsole.Classes.DateTimeExtensions.GetSpecifiedDailyDaysFromDateWithMaxDate(workingDate, maxDate, repeatEvery.Value);
                    break;
                case "1": // Weekdays
                    dateList = WFMConsole.Classes.DateTimeExtensions.GetSpecifiedDaysFromDateWithMaxDate(workingDate, new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday}, maxDate, repeatEvery.Value);
                    break;
                case "2": // M W F
                    dateList = WFMConsole.Classes.DateTimeExtensions.GetSpecifiedDaysFromDateWithMaxDate(workingDate, new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, maxDate, repeatEvery.Value);
                    break;
                case "3": // TU TH
                    dateList = WFMConsole.Classes.DateTimeExtensions.GetSpecifiedDaysFromDateWithMaxDate(workingDate, new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }, maxDate, repeatEvery.Value);
                    break;
                case "4": // Weekly on certain days
                    var specifiedDays = repeatItem.RepeatOnDays.TrimEnd(',').Split(',');
                    var importantDays = new List<DayOfWeek>();
                    foreach (var item in specifiedDays)
                    {
                        switch (item)
                        {
                            case "SU": importantDays.Add(DayOfWeek.Sunday); break;
                            case "MO": importantDays.Add(DayOfWeek.Monday); break;
                            case "TU": importantDays.Add(DayOfWeek.Tuesday); break;
                            case "WE": importantDays.Add(DayOfWeek.Wednesday); break;
                            case "TH": importantDays.Add(DayOfWeek.Thursday); break;
                            case "FR": importantDays.Add(DayOfWeek.Friday); break;
                            case "SA": importantDays.Add(DayOfWeek.Saturday); break;
                        }
                    }
                    dateList = WFMConsole.Classes.DateTimeExtensions.GetSpecifiedDaysFromDateWithMaxDate(workingDate, importantDays, maxDate, repeatEvery.Value);
                    break;
                case "5": //Monthly
                    while(workingDate < maxDate)
                    {
                        dateList.Add(workingDate);
                        workingDate = workingDate.AddMonths(1 * repeatEvery.Value);
                    }
                    break;
                case "6": //Annually
                    while (workingDate < maxDate)
                    {
                        dateList.Add(workingDate);
                        workingDate = workingDate.AddYears(1 * repeatEvery.Value);
                    }
                    break;
                default:
                    break;
            }

            return dateList;
        }


        private static bool CreateActionHistory(int eventId, int scheduleId, string scheduleType, string action, string description, string before, string after, string performedBy)
        {
            using (var db = new OnyxEntities())
            {
                var newRow = new BUS_WFMDashboard_Action_History();
                if (eventId > 0)
                    newRow.EventId = eventId;
                else
                {
                    newRow.ScheduleId = scheduleId;
                    newRow.ScheduleType = scheduleType;
                }
                newRow.Action = action;
                newRow.Description = description;
                newRow.Before = before;
                newRow.After = after;
                newRow.PerformedAt = DateTime.Now;
                newRow.PerformedBy = performedBy;
                db.BUS_WFMDashboard_Action_History.Add(newRow);
                db.SaveChanges();
            }
            return true;
        }
    }
}