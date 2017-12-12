using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WFMConsole.ViewModels
{
    public class TeamInfo
    {
        public string PTO { get; set; }
        public string Planned { get; set; }
        public string Unplanned { get; set; }
        public string Training { get; set; }
        public string Unpaid { get; set; }
        public string LOA { get; set; }
        public string Other { get; set; }
        public string TotalDown { get; set; }
        public string TeamName { get; set; }
        public HiddenTeamInfo HiddenTeamInfo { get; set; }
        public TeamInfo()
        {
            PTO = "0";
            Training = "0";
            Unpaid = "0";
            LOA = "0";
            Other = "0";
            TotalDown = "0";
            HiddenTeamInfo = new HiddenTeamInfo();
        }
    }
    public class HiddenTeamInfo
    {
        public string PlannedString { get; set; }
        public string UnplannedString { get; set; }
        public string TrainingString { get; set; }
        public string UnpaidString { get; set; }
        public string LOAString { get; set; }
        public string OtherString { get; set; }
        public HiddenTeamInfo()
        {

        }
    }
}