using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WFMConsole.Classes
{
    public class TeamInfo
    {
        public string PTO { get; set; }
        public string Training { get; set; }
        public string LOA { get; set; }
        public string Other { get; set; }
        public string TotalDown { get; set; }
        public string TeamName { get; set; }
        public TeamInfo()
        {
            PTO = "0";
            Training = "0";
            LOA = "0";
            Other = "0";
            TotalDown = "0";
        }
    }
}