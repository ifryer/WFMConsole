using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WFMConsole.Models;

namespace WFMConsole.ViewModels
{
    public class ViewEvent
    {
        public int Id { get; set; }
        //The lowercase fields are for full calendar
        public string start { get; set; }
        public string end { get; set; }
        public string title { get; set; }
        public bool allDay { get; set; }
        public bool repeating { get; set; }
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

        private static Dictionary<string, string> colorList = new Dictionary<string, string>() { { "1", "#a4bbfc" }, { "2", "" }, { "3", "" }, { "4", "#ff887c" }, { "5", "#fbd75b" }, { "6", "" }, { "7", "#46d6db" }, { "8", "" }, { "9", "#5484ed" }, { "10", "#51b749" }, { "11", "#dc2127" }, { "12", "" }, };
        private static Dictionary<string, string> colorListPast = new Dictionary<string, string>() { { "1", "#d6e1ff" }, { "2", "" }, { "3", "" }, { "4", "#ffdbd7" }, { "5", "rgb(255, 240, 190)" }, { "6", "" }, { "7", "#c7f3f4" }, { "8", "" }, { "9", "#dde6fb" }, { "10", "#dcf1db" }, { "11", "#ecafb1" }, { "12", "" }, };
        //TODO: change Past color ID 5 to be more accurate to the actual color

        public ViewEvent(BUS_WFMDashboard_Event item, bool past)
        {
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
                end = EndDate = item.EndTime.ToShortDateString();
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

        }

        public ViewEvent(BUS_WFMDashboard_Event item, DateTime inputDate)
        {
            ColorId = item.Color;
            Id = item.Id;
            title = item.Description;
            TeamName = item.TeamName;
            LastName = item.LastName;
            allDay = item.FullDay;
            EventType = item.EventType;
            Notes = item.Notes;
            CalendarEventId = item.CalendarEventId;

            var daysDifference = (inputDate - item.StartTime).Days;

            if (allDay)
            {
                StartDate = start = inputDate.ToShortDateString();
                EndDate = end = item.EndTime.AddDays(daysDifference).ToShortDateString();
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
}