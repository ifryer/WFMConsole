﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WFMConsole.Models;

namespace WFMConsole.ViewModels
{
    public class ViewEvent
    {
        public string start { get; set; }
        public string end { get; set; }
        public string title { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TeamName { get; set; }
        public string EventType { get; set; }
        public bool allDay { get; set; }
        public string LastName { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }

        public string textColor { get; set; }

        private static Dictionary<string, string> colorList = new Dictionary<string, string>() { { "1", "#a4bbfc" }, { "2", "" }, { "3", "" }, { "4", "#ff887c" }, { "5", "" }, { "6", "" }, { "7", "#46d6db" }, { "8", "" }, { "9", "#5484ed" }, { "10", "#51b749" }, { "11", "#dc2127" }, { "12", "" }, };
        private static Dictionary<string, string> colorListPast = new Dictionary<string, string>() { { "1", "#d6e1ff" }, { "2", "" }, { "3", "" }, { "4", "#ffdbd7" }, { "5", "" }, { "6", "" }, { "7", "#c7f3f4" }, { "8", "" }, { "9", "#dde6fb" }, { "10", "#dcf1db" }, { "11", "#ecafb1" }, { "12", "" }, };

        public ViewEvent(BUS_WFMDashboard_Event item, bool past)
        {
            
            title = item.Description;
            TeamName = item.TeamName;
            LastName = item.LastName;
            allDay = item.FullDay;
            EventType = item.EventType;
            if (allDay)
            {
                StartTime = item.StartTime.ToShortDateString();
                EndTime = item.EndTime.ToShortDateString();
                start = DateTime.Today.ToShortDateString();
                end = item.EndTime.ToShortDateString();

            }
            else
            {
                StartTime = item.StartTime.ToShortDateString() + " " + item.StartTime.ToShortTimeString();
                EndTime = item.EndTime.ToShortDateString() + " " + item.EndTime.ToShortTimeString();
                start = item.StartTime.ToShortDateString() + " " + item.StartTime.ToShortTimeString();
                end = item.EndTime.ToShortDateString() + " " + item.EndTime.ToShortTimeString();
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
    }
}