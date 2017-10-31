using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WFMConsole.ViewModels
{

    public class MowReport
    {
        public string ReportDate { get; set; }
        public string ReportTime { get; set; }
        public string ICM { get; set; }
        public string LateShiftManager { get; set; }
        public string WfoTimeRange { get; set; }
        public List<string> WfoList { get; set; }
        public MowReport()
        {
            ReportDate = DateTime.Now.ToString("dddd - MM/dd/yyyy");
            ReportTime = DateTime.Now.ToShortTimeString();
            WfoList = new List<string>();
        }

    }
    public class DownByReport
    {
        public string Title { get; set; }
        public Dictionary<string, DownBySection> Sections { get; set; }
        public List<DownByEvent> Events { get; set; }
        public int TotalDown { get; set; }
        public string ReportDate { get; set; }
        public string ReportTime { get; set; }
        public DownByReport()
        {
            Sections = new Dictionary<string,DownBySection>();
            TotalDown = 0;
            ReportTime = DateTime.Now.ToShortTimeString();
            ReportDate = DateTime.Now.ToString("dddd - MM/dd/yyyy");

            Sections["PTO"] = new DownBySection()
            {
                Name = "PTO"
            };
            Sections["Training"] = (new DownBySection()
            {
                Name = "Training"
            });
            Sections["LOA"] = (new DownBySection()
            {
                Name = "LOA"
            });
            Sections["Other"] = (new DownBySection()
            {
                Name = "Other"
            });

            Events = new List<DownByEvent>();
        }
    }

    
    public class DownBySection
    {
        public string Name { get; set; }
        public int TotalDown { get; set; }
        public string AD { get; set; }
        public string RA { get; set; }
        public string POA { get; set; }
        public string IRC { get; set; }
        public string PrinterOps { get; set; }
        public string MGR { get; set; }
        public DownBySection()
        {
            TotalDown = 0;
            AD = "0";
            RA = "0";
            POA = "0";
            IRC = "0";
            PrinterOps = "0";
            MGR = "0";
        }
    }

    
    public class DownByEvent
    {
        private static Dictionary<string, string> colorList = new Dictionary<string, string>() { { "1", "#a4bbfc" }, { "2", "" }, { "3", "" }, { "4", "#ff887c" }, { "5", "" }, { "6", "" }, { "7", "#46d6db" }, { "8", "" }, { "9", "#5484ed" }, { "10", "#51b749" }, { "11", "#dc2127" }, { "12", "" }, };
        public string DateRange { get; set; }
        public DateTime StartTime { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
        public string ColorId { get; set; }
        public string FullTitle { get; set; }
        public DownByEvent(string colorString)
        {
            if (colorString != null)
            {
                ColorId = colorString;
                Color = colorList[colorString];
            }
            else
            {
                ColorId = "0";
                Color = "white";
            }
        }
    }
}