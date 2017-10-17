using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WFMConsole.Classes
{
    public class WFMEventInfo
    {
        public string TeamName { get; set; }
        public string EventType { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool FullDay { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
    }
}