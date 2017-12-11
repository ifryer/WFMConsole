using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WFMConsole.Models;

namespace WFMConsole.ViewModels
{
    //This item is for the calendar. We convert it to JSON and it gets passed to the calendar on the page.
    public class ViewEvent
    {
        public int Id { get; set; }
        //The lowercase fields are for full calendar
        public string start { get; set; }
        public string end { get; set; }
        public string title { get; set; }
        public bool allDay { get; set; }
        public bool repeating { get; set; }
        public bool repeatedEvent { get; set; }
        public bool Cancelled { get; set; }
        public string OriginalStartDate { get; set; }
        public string OriginalEndDate { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public string textColor { get; set; }
        public string Notes { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TeamName { get; set; }
        public string EventType { get; set; }
        public string LastName { get; set; }
        public string CalendarEventId { get; set; }
        public string ColorId { get; set; }
        public List<ViewNotificationItem> Notifications { get; set; }
        public List<ViewInviteeItem> Invitees { get; set; }


        private static Dictionary<string, string> colorList = new Dictionary<string, string>() { { "1", "#a4bbfc" }, { "2", "#7ae7bf" }, { "3", "#dbadff" }, { "4", "#ff887c" }, { "5", "#fbd75b" }, { "6", "#ffb878" }, { "7", "#46d6db" }, { "8", "#e1e1e1" }, { "9", "#5484ed" }, { "10", "#51b749" }, { "11", "#dc2127" }, { "12", "" }, };
        private static Dictionary<string, string> colorListPast = new Dictionary<string, string>() { { "1", "#d6e1ff" }, { "2", "#d7f8ec" }, { "3", "#f4e6ff" }, { "4", "#ffdbd7" }, { "5", "rgb(255, 240, 190)" }, { "6", "#ffead6" }, { "7", "#c7f3f4" }, { "8", "#f6f6f6" }, { "9", "#dde6fb" }, { "10", "#dcf1db" }, { "11", "#ecafb1" }, { "12", "" }, };
        //TODO: change Past color ID 5 to be more accurate to the actual color
        public ViewEvent(){}
        public ViewEvent(BUS_WFMDashboard_Event item, bool past, bool Repeating, List<BUS_WFMDashboard_Event_Notification> notifications, List<BUS_WFMDashboard_Event_Invitee> invitees)
        {
            repeating = Repeating;
            Cancelled = item.Cancelled;
            ColorId = item.Color;
            Id = item.Id;
            title = item.Description;
            TeamName = item.TeamName;
            LastName = item.LastName;
            allDay = item.FullDay;
            EventType = item.EventType;
            Notes = item.Notes;
            CalendarEventId = item.CalendarEventId;
            if (allDay)
            {
                start = StartDate = item.StartTime.ToShortDateString();
                end = item.EndTime.ToShortDateString();
                EndDate = item.EndTime.AddDays(-1).ToShortDateString();
            }
            else
            {
                StartDate = item.StartTime.ToShortDateString();
                EndDate = item.EndTime.ToShortDateString();
                StartTime = item.StartTime.ToShortTimeString();
                EndTime = item.EndTime.ToShortTimeString();
                start = StartDate + " " + StartTime;
                end = EndDate + " " + EndTime;
            }
            if (item.Color != null)
            {
                if (past)
                {
                    textColor = "grey";
                    borderColor = "grey";
                    backgroundColor = colorListPast[item.Color];
                }
                else
                {
                    textColor = "black";
                    borderColor = "black";
                    backgroundColor = colorList[item.Color];
                }
            }
            else
            {
                backgroundColor = "#3a87ad";
            }
            Notifications = new List<ViewNotificationItem>();
            foreach (var notification in notifications)
            {
                Notifications.Add(new ViewNotificationItem() { Id = notification.Id, notificationTime = notification.NotificationTime, notificationType = notification.NotificationType });
            }
            Invitees = new List<ViewInviteeItem>();
            foreach(var invitee in invitees)
            {
                Invitees.Add(new ViewInviteeItem() { Id = invitee.Id, agentNo = invitee.AgentNo, email = invitee.Email, firstName = invitee.FirstName, lastName = invitee.LastName });
            }

        }

        //This constructor is only for repeated events (i.e. the original event will not use this constructor but any repeats of it will use this)
        public ViewEvent(BUS_WFMDashboard_Event item, DateTime inputDate)
        {
            OriginalStartDate = item.StartTime.ToShortDateString();
            OriginalEndDate = item.EndTime.ToShortDateString();
            ColorId = item.Color;
            Id = item.Id;
            title = item.Description;
            TeamName = item.TeamName;
            LastName = item.LastName;
            allDay = item.FullDay;
            EventType = item.EventType;
            Notes = item.Notes;
            CalendarEventId = item.CalendarEventId;
            repeatedEvent = true;
            Cancelled = item.Cancelled;
            var daysDifference = (inputDate - item.StartTime.Date).Days;

            if (allDay)
            {
                StartDate = start = inputDate.ToShortDateString();
                end = item.EndTime.AddDays(daysDifference).ToShortDateString();
                EndDate = item.EndTime.AddDays(daysDifference).AddDays(-1).ToShortDateString();
                OriginalEndDate = item.EndTime.AddDays(-1).ToShortDateString();
            }
            else
            {
                StartDate = inputDate.ToShortDateString();
                EndDate = item.EndTime.AddDays(daysDifference).ToShortDateString();
                StartTime = item.StartTime.ToShortTimeString();
                EndTime = item.EndTime.ToShortTimeString();
                start = StartDate + " " + StartTime;
                end = EndDate + " " + EndTime;
            }
            if (item.Color != null)
            {
                textColor = "black";
                borderColor = "black";
                backgroundColor = colorList[item.Color];
            }
            else
            {
                backgroundColor = "#3a87ad";
            }

        }
    }

    //This class is for filling in the repeating form when editing an event
    public class ViewRepeatingItem
    {
        public string repeatType { get; set; }
        public int repeatEveryNumber { get; set; }
        public string repeatOnDays { get; set; }
        public string repeatEndType { get; set; }
        public string repeatEndDate { get; set; }
        public int repeatEndAfterNumber { get; set; }

        public ViewRepeatingItem(BUS_WFMDashboard_Repeating_Event inputEvent)
        {
            repeatType = inputEvent.RepeatType;
            repeatEveryNumber = inputEvent.RepeatEveryNumber.Value;
            repeatOnDays = inputEvent.RepeatOnDays;
            repeatEndType = inputEvent.EndType;
            if(inputEvent.EndDate != null)
                repeatEndDate = inputEvent.EndDate.Value.ToShortDateString();
            if(inputEvent.EndAfterOccurences != null)
                repeatEndAfterNumber = inputEvent.EndAfterOccurences.Value;
        }
    }

    //This class is for input from edit/create event form, and also gets passed to the edit form
    public class ViewNotificationItem
    {
        public int Id { get; set; }
        public string notificationType { get; set; }
        public int notificationTime { get; set; }
    }

    //This class is for input from edit/create event form, and also gets passed to the edit form
    public class ViewInviteeItem
    {
        public int Id { get; set; }
        public string email { get; set; }
        public int agentNo { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
}