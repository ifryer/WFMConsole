using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WFMConsole.Models;

namespace WFMConsole.ViewModels
{
    public class ViewAgent
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int AgentNo { get; set; }
        public string TeamName { get; set; }

        public ViewAgent(Agent agent)
        {
            LastName = agent.LastName;
            FirstName = agent.FirstName;
            AgentNo = agent.AgentNo;
            TeamName = agent.TeamName.Replace("Team", "").Replace(" ", "");
        }
    }

    public class ViewInvitee
    {
        public int AgentNo { get; set; }
        public string Email { get; set; }
        public int ReportTo { get; set; }
        public string ReportToEmail { get; set; }
        public string ReportToName { get; set; }
        public string ReportToFirstName { get; set; }
        public string ReportToLastName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName {get;set;}

        public ViewInvitee(Agent agent, string reportToEmail, string reportToFirstName, string reportToLastName)
        {
            AgentNo = agent.AgentNo;
            FirstName = agent.FirstName;
            LastName = agent.LastName;
            FullName = agent.FirstName + " " + agent.LastName;
            Email = agent.Email;
            ReportTo = agent.ReportTo.Value;
            ReportToEmail = reportToEmail;
            ReportToFirstName = reportToFirstName;
            ReportToLastName = reportToLastName;
            ReportToName = reportToFirstName + " " + reportToLastName;
        }
        public ViewInvitee(string email)
        {
            Email = email;
            AgentNo = 0;
            FullName = email;
            FirstName = email;
            LastName = "";
        }
    }
}